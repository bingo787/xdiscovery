using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NV.DRF.Controls;
using NV.DRF.Core.Common;

namespace Detector
{

    /// <summary>
    /// 探测器操作辅助类
    /// </summary>
    public class DetectorController
    {

        public bool bGainAcqFinished = false;   // 采集gain亮场图像结束标识
        public bool bDefectAcqFinished = false; // 采集defect亮场图像结束标识
        public bool bOffsetTemplateOk = false; // 生成offset模板成功标识
        public bool bGainsetTemplateOk = false; // 生成gain模板成功标识
        public bool bDefectTemplateOk = false; // 生成defect模板成功标识
        public bool bDownloadTemplateOk = false; // 下载模板成功标识
        EnumGENERATE_TEMPLATE_TYPE enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.OFFSET_TEMPLATE;
        emTFILE_MODE m_emfiletype;
        RegCfgInfo m_pLastRegCfg; //记录固件所有配置数据  1024字节的结构体
        IMAGE_CORRECT_ENABLE m_pCorrect = new IMAGE_CORRECT_ENABLE();
        //    DOWNLOAD_FILE m_pdownloadmode = new DOWNLOAD_FILE();
        FPD_AQC_MODE m_stMode = new FPD_AQC_MODE();

        public string log_path = System.Environment.CurrentDirectory;

        /// <summary>
        /// 是否丢包
        /// </summary>
        public Int32 _MissedPacketNum;

        /// <summary>
        /// 是否存储图像
        /// </summary>`
        public bool IsStored { get; set; }
        /// <summary>
        /// 是否多帧叠加
        /// </summary>
        public bool IsMultiFramesOverlay { get; set; }
        /// <summary>
        /// 是否多帧平均叠加
        /// </summary>
        public bool IsMultiFramesOverlayByAvg { get; set; }
        /// <summary>
        /// 多帧叠加数量
        /// </summary>
        public int MultiFramesOverlayNumber { get; set; }
        /// <summary>
        /// 数据位数,图像宽，高
        /// </summary>
        private int _bits, _detectorWidth, _detectorHeight, _imageWidth, _imageHeight;
        /// <summary>
        /// 数据位数
        /// </summary>
        public int Bits { get { return _bits; } }
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int ImageWidth { get { return _imageWidth; } }
        /// <summary>
        /// 图像高度
        /// </summary>
        public int ImageHeight { get { return _imageHeight; } }
        /// <summary>
        /// 对外提供的播放缓存队列
        /// </summary>
        public Queue<ushort[]> PlayBuffer = new Queue<ushort[]>();
        /// <summary>
        /// 多帧叠加队列
        /// </summary>
        private Queue<ushort[]> _multiFramesOverlayBuffer = new Queue<ushort[]>();
        /// <summary>
        /// 内部存储图像缓存
        /// </summary>
        private List<ushort[]> _imageBuffer = new List<ushort[]>();
        public List<ushort[]> ImageBuffer { get { return _imageBuffer; } }
        /// <summary>
        /// 延时采集时间
        /// </summary>
        public int Delay = 0;
        /// <summary>
        /// 单例模式，提供辅助类实例
        /// </summary>
        private static readonly DetectorController _instance = new DetectorController();
        /// <summary>
        /// 外部实例获取属性
        /// </summary>
        public static DetectorController Instance { get { return _instance; } }

        private DetectorController()
        {
            ShowImageCallBack = new ImageCallbackHandler(ReceiveImage);
            AcqMaxFrameEvent = new ExcutedCallbackHandler(AcqMaxFrame);
            TemperatureCallBack = new TemperatureCallbackHandler(TemperatureChanged);
            SystemStatusCallBack = new SystemStatusCallbackHandler(SystemStatusChanged);
            ConnBreakCallBack = new ExcutedCallbackHandler(ConnBreak);
            FinishedOffsetEvent = new ExcutedFinishCallbackHandler(FinishedOffset);
            FinishedDetectEvent = new ExcutedFinishCallbackHandler(FinishedDetect);
            FinishedGainEvent = new ExcutedFinishCallbackHandler(FinishGain);
            OpenXRayEvent = new ExcutedCallbackHandlerWithValue(OpenXRay);
            CloseXRayEvent = new ExcutedCallbackHandler(CloseXRay);

            IsStored = true;
            IsMultiFramesOverlay = false;
            IsMultiFramesOverlayByAvg = true;
            MultiFramesOverlayNumber = 2;

                //add by zhao 
            HBIEventCallback = new USER_CALLBACK_HANDLE_ENVENT(RecieveImageAndEvent);
            HBIProcessCallback = new USER_CALLBACK_HANDLE_PROCESS(HandleProcessCallback);


        }

        /// <summary>
        /// 程序当前线程Dispatcher
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher { get { return Application.Current.Dispatcher; } }
        /// <summary>
        /// 获取上次出错信息
        /// </summary>
        /// <returns></returns>
        public string GetLastError(int ret)
        {
            //  StringBuilder _error = new StringBuilder(1024);
            //   NVDentalSDK.NV_LastErrorMsg(_error, 1024);
            //  return _error.ToString();
            return HBI_FPD_DLL.GetErrorMsgByCode(ret);
        }

        public bool GetTemperature(out float temperatrue1, out float temperature2)
        {
            // HBI没有找到温度相关的接口
            //float a, b;
            //if (NVDentalSDK.NV_GetTemperature(out a, out b) == NV_StatusCodes.NV_SC_SUCCESS)
            //{
            //    temperatrue1 = a;
            //    temperature2 = b;
            //    return true;
            //}
            temperatrue1 = 0;
            temperature2 = 0;
            return false;
        }
        /// <summary>
        /// 开始采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool StartAcq()
        {
            ShowMessage("DEBUG ");
            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;
            //  亦可不用切换到软触发模式下，发送连续采集1帧即可完成单帧采集
            FPD_AQC_MODE stMode = new FPD_AQC_MODE();
            stMode.aqc_mode = EnumIMAGE_ACQ_MODE.DYNAMIC_ACQ_DEFAULT_MODE;
            stMode.nLiveMode = 2;     // 1-固件做offset模板并上图；2-只上图；3-固件做只做offset模板。
            stMode.ndiscard = 0;     // 这里默认位0，不抛弃前几帧图像
            stMode.nframeid = 0;     // 这里默认位0
            stMode.nframesum = MaxFrames;    // 0-表示一直采图，20表示采集20帧图结束。这里默认采集20帧
            stMode.ngroupno = 0;     // 这里默认位0
            stMode.bSimpleGT = false; // 表示不启用快速生成模板
            stMode.isOverLap = false; // 不要做叠加
            stMode.nGrayBit = emGRAY_MODE.GRAY_16;
            int ret = HBI_FPD_DLL.HBI_LiveAcquisition(HBI_FPD_DLL._handel, stMode);


            if (ret != 0)
            {
                ShowMessage("StartAcq Failed——" + GetLastError(ret), true);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 停止采集图像
        /// </summary>
        /// <returns></returns>
        public bool StopAcq()
        {
            ShowMessage("DEBUG ");

            int ret = HBI_FPD_DLL.HBI_StopAcquisition(HBI_FPD_DLL._handel);
            if (ret !=0)
            {
                ShowMessage("StopAcq Failed——" + GetLastError(ret), true);
                return false;
            }

            _waitHandle.Set();
            return true;
        }

        int count = 0;
        /// <summary>
        /// 采集图像事件同步锁
        /// </summary>
        private System.Threading.EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        /// <summary>
        /// 本底校正事件同步锁
        /// </summary>
        private System.Threading.EventWaitHandle _offsetWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        /// <summary>
        /// 增益校正事件锁
        /// </summary>
        private System.Threading.EventWaitHandle _gainWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        /// <summary>
        /// 探测器采集图像回调
        /// </summary>
        public static ImageCallbackHandler ShowImageCallBack;
        /// <summary>
        /// 采集到最大帧回调
        /// </summary>
        public static ExcutedCallbackHandler AcqMaxFrameEvent;
        /// <summary>
        /// 温度监控
        /// </summary>
        public static TemperatureCallbackHandler TemperatureCallBack;
        /// <summary>
        /// 系统状态
        /// </summary>
        public static SystemStatusCallbackHandler SystemStatusCallBack;
        /// <summary>
        /// 断开委托
        /// </summary>
        public static ExcutedCallbackHandler ConnBreakCallBack;

        public event TemperatureCallbackHandler TemperatureChangedEvent;
        public event SystemStatusCallbackHandler SystemStatusChangedEvent;
        public event ExcutedCallbackHandler ConnBreakEvent;
        public event ExcutedCallbackHandler AcqMaxFramesEvent;
        public int MaxFrames = 0;

        public static USER_CALLBACK_HANDLE_ENVENT HBIEventCallback;
        public static USER_CALLBACK_HANDLE_PROCESS HBIProcessCallback;


        /// <summary>
        /// 初始化探测器
        /// </summary>
        public bool InitDetector(out string res)
        {
            res = string.Empty;

            // 因为要重新连接，所以需要先销毁
            HBI_FPD_DLL.HBI_Destroy(HBI_FPD_DLL._handel);
            HBI_FPD_DLL._handel = HBI_FPD_DLL.HBI_Init();

            // 然后再注册回调函数

            int ret = HBI_FPD_DLL.HBI_RegEventCallBackFun(HBI_FPD_DLL._handel, HBIEventCallback);
            if (ret != 0)
                res += ("HBI_RegEventCallBackFun Failed");
            else
                ShowMessage("HBI_RegEventCallBackFun success.");


            string local_ip = "192.168.10.20";
            string remote_ip = "192.168.10.40";
            ret = HBI_FPD_DLL.HBI_ConnectDetector(HBI_FPD_DLL._handel, remote_ip, 0x8081, local_ip, 0x8080);

            ShowMessage("local ip: " + local_ip + " <---> " + remote_ip);

            if (ret != 0)
            {
                res += "探测器连接失败。" + GetLastError(ret);
                return false;
            }
            else
            {
                //if (NVDentalSDK.NV_MonitorTemperature(TemperatureCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                //    res += ("Temperature failed");
                //if (NVDentalSDK.NV_MonitorSystemStatus(SystemStatusCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                //    res += ("SystemStatus failed");
                //if (NVDentalSDK.NV_MonitorConnbreak(ConnBreakCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                //    res += ("ConnBreak failed");


                IMAGE_PROPERTY img_pro = new IMAGE_PROPERTY();
                ret = HBI_FPD_DLL.HBI_GetImageProperty(HBI_FPD_DLL._handel, ref img_pro);

                if (ret != (int)HBIRETCODE.HBI_SUCCSS)
                {
                    res += ("HBI_GetImageProperty failed");
                }
                else
                {
                    _detectorHeight = 3072;
                    _detectorWidth = 3072;

                    _imageWidth = (int)img_pro.nwidth;
                    _imageHeight = (int)img_pro.nheight;
                    if (img_pro.ndatabit == 0)
                    {
                        _bits = 16;
                    }
                    else if (img_pro.ndatabit == 1)
                    {
                        _bits = 14;
                    }
                    else if (img_pro.ndatabit == 2)
                    {
                        _bits = 12;
                    }
                    else if (img_pro.ndatabit == 3)
                    {
                        _bits = 8;
                    }
                    else {
                        _bits = 16;
                    }
                  

                    ShowMessage("W:"+_imageWidth.ToString() + " H:"+_imageHeight.ToString() +" B:"+ _bits.ToString());


                }
                res += (string.Format("Size w:{0} h:{1} bits:{2}", ImageWidth, ImageHeight, Bits));
                res += "探测器已连接。";

                btnGetFirmwareCfg_Click();
                btnImageProperty_Click();
            }
            return true;
        }

        private void ConnBreak()
        {
            ShowMessage("DEBUG ");
            if (ConnBreakEvent != null)
            {
                ConnBreakEvent();
            }
        }

        private void SystemStatusChanged(int a, int b)
        {
            if (SystemStatusChangedEvent != null)
            {
                SystemStatusChangedEvent(a, b);
            }
        }

        private void TemperatureChanged(float a, float b)
        {
            if (TemperatureChangedEvent != null)
            {
                TemperatureChangedEvent(a, b);
            }
        }


        /// <summary>
        /// HBI 的事件回调函数，这个接口内部有众多事件需要判断
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="buff"></param>
        /// <param name="len"></param>
        /// <param name="nID"></param>
        /// <returns></returns>
        private int RecieveImageAndEvent(byte cmd, IntPtr buff, int len, int nID)
        {
            eCallbackEventCommType command = (eCallbackEventCommType)cmd;

            if ((command == eCallbackEventCommType.ECALLBACK_TYPE_SINGLE_IMAGE) || (command == eCallbackEventCommType.ECALLBACK_TYPE_MULTIPLE_IMAGE) ||
                (command == eCallbackEventCommType.ECALLBACK_TYPE_ROM_UPLOAD) || (command == eCallbackEventCommType.ECALLBACK_TYPE_RAM_UPLOAD))
            {
                if (buff == null || len == 0)
                {
                    ShowMessage("注册回调函数参数异常!");
                    return 0;
                }
            }

            ShowMessage("================== RecieveImageAndEvent cmd " + cmd.ToString());


            switch (command)
            {
                case eCallbackEventCommType.ECALLBACK_TYPE_NET_ERR_MSG:
                    {

                        if (len <= 0 && len >= -7)
                        {

                            // 连接断开了

                            ConnBreakCallBack();

                            if (len == 0)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG, Err: 网络未连接！", true);
                            else if (len == -1)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,Err:参数异常!", true);
                            else if (len == -2)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,Err:准备就绪的描述符数返回失败!", true);
                            else if (len == -3)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,Err:接收超时!", true);
                            else if (len == -4)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,Err:接收失败!", true);
                            else if (len == -5)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,Err:端口不可读!", true);
                            else if (len == -6)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,network card unusual!", true);
                            else if (len == -7)
                                ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,network card ok!", true);


                        }
                        else if (len == 1)
                            ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,开始监听!");
                        else if (len == 2)
                            ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,ready!");
                        else if (len == 3)
                            ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,busy!");
                        else if (len == 4)
                            ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG,prepare!");
                        else
                            ShowMessage("ECALLBACK_TYPE_NET_ERR_MSG, unknown err {}");

                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_ROM_UPLOAD:
                    {
                        ShowMessage("ECALLBACK_TYPE_ROM_UPLOAD...");
                        //if (INSTANCE->m_pLastRegCfg != NULL)
                        //{
                        //    memset(INSTANCE->m_pLastRegCfg, 0x00, sizeof(RegCfgInfo));
                        //    memcpy_s(INSTANCE->m_pLastRegCfg, sizeof(RegCfgInfo), (unsigned char *)buff, sizeof(RegCfgInfo));

                        //    XLOG_INFO("Serial Number : {}", INSTANCE->m_pLastRegCfg->m_SysBaseInfo.m_cSnNumber);

                        //}
                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_SINGLE_IMAGE:
                case eCallbackEventCommType.ECALLBACK_TYPE_MULTIPLE_IMAGE:
                    {
                        ShowMessage("ECALLBACK_TYPE_SINGLE_IMAGE or ECALLBACK_TYPE_MULTIPLE_IMAGE");
                        ShowMessage("image size " + len.ToString() + " nID " + nID.ToString());
                        NV_ImageInfo _ImageInfo;
                        
                        //todo: ZQB 像素格式这里需要确认下是怎么搞？
                        _ImageInfo.iPixelType = 0;	///< 像素格式[没有用到]
                        _ImageInfo.iSizeX = _imageWidth;             ///< 图像宽
                        _ImageInfo.iSizeY = _imageHeight;             ///< 图像高
                        _ImageInfo.iImageSize = len ;         ///< 图像所占的字节数

                        unsafe {
                            _ImageInfo.pImageBuffer = (ushort*)buff;
                                }		///< 图像数据指针
                        _ImageInfo.iTimeStamp = System.DateTime.Now.Second;			///< 时间戳
                        _ImageInfo.iMissingPackets = 0; // _MissedPacketNum;    ///< 丢失的包数量
                        _ImageInfo.iAnnouncedBuffers = 0;  ///< 声明缓存区大小[暂为0]
                        _ImageInfo.iQueuedBuffers = 0;     ///< 队列缓存区大小[暂为0]
                        _ImageInfo.iOffsetX = 0;           ///< x方向偏移量[暂未设置]
                        _ImageInfo.iOffsetY = 0;           ///< y方向偏移量[暂未设置]
                        _ImageInfo.iAwaitDelivery = 0;     ///< 等待传送的帧数[暂为0]
                        _ImageInfo.iBlockId = 0;          ///< GVSP协议的block-id

                        ShowImageCallBack(0, _ImageInfo);

                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_THREAD_EVENT:
                    {
                        if (len == 112)// offset使能，校正反馈信息
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},offset calibrate:success!");
                        else if (len == 113)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},offset calibrate:failed!");
                        else if (len == 114)// gain使能，校正反馈信息
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},gain calibrate:success!");
                        else if (len == 115)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},gain calibrate:failed!");
                        else if (len == 116)// defect使能，校正反馈信息
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},defect calibrate:success!");
                        else if (len == 117)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},defect calibrate:failed!");
                        else
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT{},other feedback message!");
                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_FPD_STATUS:
                    {
                        ShowMessage("TYPE_FPD_STATUS, command= " + cmd.ToString());
                        if (len == 5)
                        {
                             ShowMessage("aqc mode" + m_stMode.aqc_mode.ToString() + " " + m_stMode.nLiveMode.ToString() + " " + m_stMode.bSimpleGT.ToString());

                            if (m_stMode.aqc_mode == EnumIMAGE_ACQ_MODE.DYNAMIC_ACQ_DEFAULT_MODE && m_stMode.nLiveMode == 3)//Only Template
                            {
                                bOffsetTemplateOk = true; // 表示生成offset模板成功
                                Log(string.Format("\tECALLBACK_TYPE_FPD_STATUS,bOffsetTemplateOk is true!aqc_mode:{0}\n", (int)(m_stMode.aqc_mode)));
                                FinishedOffsetEvent(true);    
                            }
                            //
                            if (m_stMode.aqc_mode == EnumIMAGE_ACQ_MODE.DYNAMIC_ACQ_BRIGHT_MODE)
                            {
                                bGainAcqFinished = true;// 表示defect采图成功
                                Log(string.Format("\tECALLBACK_TYPE_FPD_STATUS,bGainAcqFinished is true!aqc_mode:{0}\n", (int)(m_stMode.aqc_mode)));
                                FinishedDetectEvent(true);
                            }
                            //
                            if (m_stMode.aqc_mode == EnumIMAGE_ACQ_MODE.DYNAMIC_DEFECT_ACQ_MODE && m_stMode.bSimpleGT)
                            {
                                bDefectAcqFinished = true;// 表示defect采图成功
                                Log(string.Format("\tECALLBACK_TYPE_FPD_STATUS,bDefectAcqFinished is true!aqc_mode:{0}\n", (int)(m_stMode.aqc_mode)));
                                FinishedDetectEvent(false);
                            }

                        }
                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_GAIN_ERR_MSG:
                    {
                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_DEFECT_ERR_MSG:
                    {
                        break;
                    }
                case eCallbackEventCommType.ECALLBACK_TYPE_PACKET_MISS:
                case eCallbackEventCommType.ECALLBACK_TYPE_PACKET_MISS_MSG:
                    {
                        ShowMessage("Packet miss  " + len.ToString());
                        _MissedPacketNum = len;
                        break;
                    }
                default:
                    {
                        ShowMessage("ECALLBACK_TYPE_INVALID, command " + cmd.ToString(), true);
                        break;
                    }
            }
            return 0;
        }
        /// <summary>
        /// HBI 的处理回调函数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="retcode"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        private unsafe int HandleProcessCallback(byte cmd, int retcode, IntPtr buff) {
            Log(cmd.ToString() + " " + retcode.ToString());
            return 0;
        }

        /// <summary>
        /// 实时处理探测器采集到的图像
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="image"></param>
        private unsafe void ReceiveImage(byte wnd, NV_ImageInfo image)
        {
            ShowMessage("DEBUG ");
            if (image.iMissingPackets > 0)
            {
                return;//跳过丢包图像 
            }

            /* 探测器分辨率大小 */
            int WIDTH = _imageWidth;
            int HEIGHT = _imageHeight;
            if ((image.pImageBuffer == null) || ((image.iImageSize/sizeof(ushort)) != (WIDTH * HEIGHT)))
            {
                ShowMessage("图像数据异常 ",true);
                return ;
            }


            ShowMessage("DEBUG " + " iSizeX " + image.iSizeX.ToString() + " iSizeY" + image.iSizeY.ToString() );
            ushort[] buffer = new ushort[image.iSizeX * image.iSizeY];

            for (int i = 0; i < image.iSizeX * image.iSizeY; i++)
            {
                
                buffer[i] = image.pImageBuffer[i];
              //  ShowMessage(buffer[i].ToString());
            }

            PlayBuffer.Enqueue(buffer);
            if (IsStored)
            {
                ImageBuffer.Add(buffer);
            }

            count++;

            // 达到最大帧数量的时候，通知。
            if (count == MaxFrames) {
                AcqMaxFrameEvent();
            }
        }
        /// <summary>
        /// 多帧叠加
        /// </summary>
        /// <param name="buffer"></param>
        private void MultiFramesOverlay(ref ushort[] buffer)
        {
            ShowMessage("DEBUG ");
            _multiFramesOverlayBuffer.Enqueue(buffer);
            //低于降噪帧数直接返回
            if (_multiFramesOverlayBuffer.Count < MultiFramesOverlayNumber)
            {
                return;
            }
            //多于降噪帧数，抛弃早期图像
            while (_multiFramesOverlayBuffer.Count > MultiFramesOverlayNumber)
            {
                _multiFramesOverlayBuffer.Dequeue();
            }

            ushort[] res = new ushort[buffer.Length];

            if (_multiFramesOverlayBuffer.Count == MultiFramesOverlayNumber)
            {

                Parallel.For(0, buffer.Length,
                     (i) =>
                     {
                         int sum = 0;
                         foreach (var data in _multiFramesOverlayBuffer)
                         {
                             sum += data[i];
                         }

                         if (IsMultiFramesOverlayByAvg)
                         {
                             res[i] = (ushort)(sum / MultiFramesOverlayNumber);
                         }
                         else
                         {
                             if (sum > 65534)
                                 sum = 65534;
                             res[i] = (ushort)(sum);
                         }
                     });

                buffer = res;
            }


        }


        /// <summary>
        /// 采集到最大帧
        /// </summary>
        private void AcqMaxFrame()
        {
            ShowMessage("DEBUG ");
            
            if (AcqMaxFramesEvent != null)
            {
                AcqMaxFramesEvent.Invoke();
            }
            _waitHandle.Set();
        }

        /// <summary>
        /// 展示信息
        /// </summary>
        /// <param name="p"></param>
        /// <param name="isShowMessageBox"></param>
        public void ShowMessage(string p, bool isShowMessageBox = false)
        {
            Log(p);

            if (isShowMessageBox)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    CMessageBox.Show(p);
                }));
            }
        }

        private void Log(string p)
        {

            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
            string file = st.GetFrame(1).GetFileName();
            int line = st.GetFrame(1).GetFileLineNumber();
            string method = st.GetFrame(1).GetMethod().ToString();

            try
            {
                //如果日志目录不存在,则创建该目录
                if (!Directory.Exists(log_path))
                {
                    Directory.CreateDirectory(log_path);
                }
                string logFileName = log_path + "\\调试日志_" + DateTime.Now.ToString("yyyy_MM_dd_HH") + ".log";
                StringBuilder logContents = new StringBuilder();
                logContents.AppendLine(p + " " + method + " " + file +":" + line.ToString());
                //当天的日志文件不存在则新建，否则追加内容
                StreamWriter sw = new StreamWriter(logFileName, true, System.Text.Encoding.Unicode);
                sw.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:sss") + " " + logContents.ToString());
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 设置探测器采集时间、增益
        /// </summary>
        /// <param name="nullable1"></param>
        /// <param name="nullable2"></param>
        public string SetAcqPara(int? exp, string g, string binning = "1X1")
        {
            string res = string.Empty;
            int expTime = 168;
            //  NV_Gain gain = NV_Gain.NV_GAIN_07;
            //NV_BinningMode bin = binning == "2X2" ? NV_BinningMode.NV_BINNING_2X2 : NV_BinningMode.NV_BINNING_1X1;

            byte bin = 1;
            if (binning == "1X1")
            {
                bin = 1;
            } else if (binning == "2X2") {
                bin = 2;
            } else if (binning == "4X4") {
                bin = 4;
            }

            int ret = HBI_FPD_DLL.HBI_SetBinning(HBI_FPD_DLL._handel, bin);

            if (ret != 0)
            {
                res += ("Set BinningMode Failed:" + GetLastError(ret) + "\n");
            }

            //if (NVDentalSDK.NV_SetExpTime((int)exp) != NV_StatusCodes.NV_SC_SUCCESS)
            //{
            //    res += ("Set Exptime Failed\n");
            //}

            int gain = 6;
            switch (g)
            {
                case "0.6pC": gain = 1; break;
                case "1.2pC": gain = 2; break;
                case "2.4pC": gain = 3; break;
                case "3.6pC": gain = 4; break;
                case "4.8pC": gain = 5; break;
                case "7.2pC": gain = 6; break; //默认7.2pC
                case "9.6pC": gain = 7; break;
                default: break;

            }
            if (HBI_FPD_DLL.HBI_SetGainMode(HBI_FPD_DLL._handel, gain) != (int)HBIRETCODE.HBI_SUCCSS)
            {
                res += ("Set Gain Failed\n");
            }
            int x, y, w = 0, h = 0;
            //NVDentalSDK.NV_GetImageRange(out x, out y, out w, out h);

            //NVDentalSDK.NV_GetGain(out gain);
            //NVDentalSDK.NV_GetExpTime(out expTime);
            res += ("exp:" + expTime + "," + g + "," + binning + "," + w + "x" + h);
            return res;
        }
        #region 校正
        /// <summary>
        /// 本底校正回调
        /// </summary>
        public static ExcutedFinishCallbackHandler FinishedOffsetEvent;
        /// <summary>
        /// detect校正回调
        /// </summary>
        public static ExcutedFinishCallbackHandler FinishedDetectEvent;
        /// <summary>
        /// Gain校正回调
        /// </summary>
        public static ExcutedFinishCallbackHandler FinishedGainEvent;
        /// <summary>
        /// 开光
        /// </summary>
        public static ExcutedCallbackHandlerWithValue OpenXRayEvent;
        /// <summary>
        /// 闭光
        /// </summary>
        public static ExcutedCallbackHandler CloseXRayEvent;

        /// <summary>
        /// 本底校正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartOffset()
        {
            count = 0;

            m_stMode.aqc_mode = EnumIMAGE_ACQ_MODE.DYNAMIC_DEFECT_ACQ_MODE;
            m_stMode.bSimpleGT = true;
            m_stMode.nLiveMode = 3;
            bOffsetTemplateOk = false;
            //
            enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.OFFSET_TEMPLATE;
            int ret = HBI_FPD_DLL.HBI_GenerateTemplateEx(HBI_FPD_DLL._handel, enumTemplateType);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplateEx failed!" + ret.ToString(),true);
                return;
            }
            else
            {
               
                ShowMessage("Do pre-offset template success!",true);
            }

        }
        /// <summary>
        /// 校正完毕，设置校正模式软件模式
        /// </summary>
        /// <param name="isSuccess"></param>
        private void FinishedOffset(bool isSuccess)
        {
            ShowMessage("本底校正" + isSuccess, true);
            _offsetWaitHandle.Set();
        }

        /// <summary>
        /// detect校正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartAutoDetect()
        {
            count = 0;

            m_stMode.aqc_mode = EnumIMAGE_ACQ_MODE.DYNAMIC_DEFECT_ACQ_MODE;
            m_stMode.bSimpleGT = true;
            m_stMode.nLiveMode = 2;
            bDefectAcqFinished = false;
            bDefectTemplateOk = false;
            bDownloadTemplateOk = false;
            // 第一步：第一组亮场(剂量要求：正常高压，毫安秒调节正常的 10%)-发送采集命令
            enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.DEFECT_TEMPLATE_GROUP1;
            int ret = HBI_FPD_DLL.HBI_GenerateTemplateEx(HBI_FPD_DLL._handel, enumTemplateType);   //6
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplateEx failed!" + ret.ToString(),true);
                return;
            }
            else
            {
                ShowMessage("Do defect group1 success!",true);
            }
            // 第二步：第二组亮场(剂量要求：正常高压，毫安秒调节正常的 50%)-发送采集命令
            enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.DEFECT_TEMPLATE_GROUP2;
            ret = HBI_FPD_DLL.HBI_GenerateTemplateEx(HBI_FPD_DLL._handel, enumTemplateType);   //6
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplateEx failed!" + ret.ToString(),true);
                return;
            }
            else
            {
                ShowMessage("Do defect group2 success!",true);
            }
            // 第三步：第三组亮场(剂量要求：正常高压，毫安秒调节正常的 100%)-发送采集命令
            enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.DEFECT_TEMPLATE_GROUP3;
            ret = HBI_FPD_DLL.HBI_GenerateTemplateEx(HBI_FPD_DLL._handel, enumTemplateType);   //6
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplateEx failed!" + ret.ToString(),true);
                return;
            }
            else
            {
                ShowMessage("Do defect group2 success and Generate Template success!",true);
            }
            // 第四步：注册回调函数
            ret = HBI_FPD_DLL.HBI_RegProgressCallBack(HBI_FPD_DLL._handel, DownloadCallBack, HBI_FPD_DLL._handel);
            if (ret != 0)
            {
                ShowMessage("err:HBI_RegProgressCallBack failed,TimeOut!", true);
                return;
            }
            else {
                ShowMessage("HBI_RegProgressCallBack success!");
            }
               
            // 第五步：将defect模板下载到固件
            m_emfiletype = emTFILE_MODE.DEFECT_T;
            HBI_FPD_DLL.HBI_DownloadTemplateEx(HBI_FPD_DLL._handel, m_emfiletype);
            if (ret != 0)
            {
                ShowMessage("HBI_DownloadTemplateEx:gain template failed!ret:[{0}]" + ret.ToString(),true);
                return;
            }
            // 第六步：更新矫正使能
            m_pCorrect.bFeedbackCfg = true;  // true  - ECALLBACK_TYPE_ROM_UPLOAD Event,false - ECALLBACK_TYPE_SET_CFG_OK Event
            m_pCorrect.ucOffsetCorrection = (char)3;
            m_pCorrect.ucGainCorrection = (char)2;
            m_pCorrect.ucDefectCorrection = (char)2;
            m_pCorrect.ucDummyCorrection = (char)0; // 暂时不支持
            // 打印
            Log("HBI_UpdateCorrectEnable\n");
            Log(String.Format("\tm_pCorrect.ucOffsetCorrection={0}\n", (int)m_pCorrect.ucOffsetCorrection));
            Log(String.Format("\tm_pCorrect.ucGainCorrection={0}\n", (int)m_pCorrect.ucGainCorrection));
            Log(String.Format("\tm_pCorrect.ucDefectCorrection={0}\n", (int)m_pCorrect.ucDefectCorrection));
            Log(String.Format("\tm_pCorrect.ucDummyCorrection={0}\n", (int)m_pCorrect.ucDummyCorrection));
            ret = HBI_FPD_DLL.HBI_UpdateCorrectEnable(HBI_FPD_DLL._handel, ref m_pCorrect);
            if (ret == 0)
            {
                Log("\tHBI_UpdateCorrectEnable success!\n");
            }
            else
            {
                Log("\tHBI_UpdateCorrectEnable failed!");
                return;
            }
        }
        // dwnload template callcallback function
        private int DownloadCallBack(byte command, int code, IntPtr pContext)
        {
            switch (command)
            {
                case (byte)(eCallbackDownloadTemplateFileStatus.ECALLBACK_DTFile_STATUS_PROGRESS): // 进度 code：百分比（0~100）	 初始化进度条	
                    if (code == 100)//最后一包发完成
                    {
                        bDownloadTemplateOk = true;
                    }
                    //
                    if (m_emfiletype == emTFILE_MODE.GAIN_T)
                    {
                        Log(string.Format("gain:ECALLBACK_DTFile_STATUS_PROGRESS,recode={0}\n", code));
                    }
                    else if (m_emfiletype == emTFILE_MODE.DEFECT_T)
                    {
                        Log(string.Format("defect:ECALLBACK_DTFile_STATUS_PROGRESS,recode={0}\n", code));
                    }
                    else
                    {
                        Log(string.Format("err:ECALLBACK_DTFile_STATUS_PROGRESS,tf_mode is not exits,recode={0}\n", code));
                    }
                    break;
                case (byte)(eCallbackDownloadTemplateFileStatus.ECALLBACK_DTFile_STATUS_RESULT):   // 结果
                    Log(string.Format("DownloadCallBack,%{0}\n", code));
                    //最后一包发完成
                    if (code == 100)
                    {
                        bDownloadTemplateOk = true;
                        //
                        if (m_emfiletype == emTFILE_MODE.GAIN_T)
                        {
                            Log(string.Format("gain:ECALLBACK_DTFile_STATUS_RESULT,retcode:[{0}]\n", code));
                        }
                        else if (m_emfiletype == emTFILE_MODE.DEFECT_T)
                        {
                            Log(string.Format("defect:ECALLBACK_DTFile_STATUS_RESULT,retcode:[{0}]\n", code));
                        }
                        else
                        {
                            Log(string.Format("err:ECALLBACK_DTFile_STATUS_RESULT,tf_mode is not exits,retcode:[{0}]\n", code));
                        }
                    }
                    else
                    {
                        Log(string.Format("ECALLBACK_DTFile_STATUS_RESULT,retcode:[{0}]\n", code));
                    }
                    break;
                default: // unusual
                    Log(string.Format("DownloadCallBack,retcode:[{0}]\n", code));
                    break;
            }
            return 1;
        }
        /// <summary>
        /// detect校正返回
        /// </summary>
        /// <param name="isSuccess"></param>
        private void FinishedDetect(bool isSuccess)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowMessage("Detect校正" + isSuccess, true);
            }));
        }
        /// <summary>
        /// Gain校正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartGain()
        {

            m_stMode.aqc_mode = EnumIMAGE_ACQ_MODE.DYNAMIC_ACQ_BRIGHT_MODE;
            m_stMode.bSimpleGT = true;
            m_stMode.nLiveMode = 2;
            bGainAcqFinished = false;
            bGainsetTemplateOk = false;
            bDownloadTemplateOk = false;
            // 第一步：一组亮场(剂量要求：正常高压和电流)-发送采集命令
            // 高压的剂量一般为正常采图的剂量即可，等高压到达稳定值后开始调用生成接口直到采图完成结束。
            enumTemplateType = EnumGENERATE_TEMPLATE_TYPE.GAIN_TEMPLATE;
            int ret = HBI_FPD_DLL.HBI_GenerateTemplateEx(HBI_FPD_DLL._handel, enumTemplateType);   //6
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplateEx failed!" + ret.ToString(),true);
                return;
            }
            else
            {
                ShowMessage("Do gain template success!");
            }
            // 第二步：注册回调函数
            ret = HBI_FPD_DLL.HBI_RegProgressCallBack(HBI_FPD_DLL._handel, DownloadCallBack, HBI_FPD_DLL._handel);
            if (ret != 0)
            {
                ShowMessage("HBI_RegProgressCallBack failed!ret:[{0}]" + ret.ToString(),true);
                return;
            }
            else
                ShowMessage("HBI_RegProgressCallBack success!");
            // 第三步：将gain模板下载到固件
            m_emfiletype = emTFILE_MODE.GAIN_T;
            HBI_FPD_DLL.HBI_DownloadTemplateEx(HBI_FPD_DLL._handel, m_emfiletype);
            if (ret != 0)
            {
                ShowMessage("HBI_DownloadTemplateEx:gain template failed!ret:[{0}]" + ret.ToString());
                return;
            }
            // 第四步：更新矫正使能
            m_pCorrect.bFeedbackCfg = true;  // true  - ECALLBACK_TYPE_ROM_UPLOAD Event,false - ECALLBACK_TYPE_SET_CFG_OK Event
            m_pCorrect.ucOffsetCorrection = (char)3;
            m_pCorrect.ucGainCorrection = (char)2;
            m_pCorrect.ucDefectCorrection = (char)0;
            m_pCorrect.ucDummyCorrection = (char)0; // 暂时不支持
            // 打印
            ShowMessage("HBI_UpdateCorrectEnable\n");
            ShowMessage(String.Format("\tm_pCorrect.ucOffsetCorrection={0}\n", (int)m_pCorrect.ucOffsetCorrection));
            ShowMessage(String.Format("\tm_pCorrect.ucGainCorrection={0}\n", (int)m_pCorrect.ucGainCorrection));
            ShowMessage(String.Format("\tm_pCorrect.ucDefectCorrection={0}\n", (int)m_pCorrect.ucDefectCorrection));
            ShowMessage(String.Format("\tm_pCorrect.ucDummyCorrection={0}\n", (int)m_pCorrect.ucDummyCorrection));
            ret = HBI_FPD_DLL.HBI_UpdateCorrectEnable(HBI_FPD_DLL._handel, ref m_pCorrect);
            if (ret == 0)
            {
                ShowMessage("\tHBI_UpdateCorrectEnable success!\n");
            }
            else
            {
                ShowMessage("\tHBI_UpdateCorrectEnable failed!");
                return;
            }
        }
        /// <summary>
        /// 关闭X光回调
        /// </summary>
        private void CloseXRay()
        {

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (CMessageBox.Show("请关闭 X光后，点击确定", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    //   NVDentalSDK.NV_CancelGainCal();
                }
            }));
        }

        /// <summary>
        /// 开X光回调
        /// </summary>
        private void OpenXRay(int target)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CMessageBox.Show("请打开 X光");
            }));
        }
        /// <summary>
        /// 结束增益校正
        /// </summary>
        /// <param name="isSuccess"></param>
        private void FinishGain(bool isSuccess)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CMessageBox.Show("Gain校正" + isSuccess);
            }));
            _gainWaitHandle.Set();
        }
        #endregion

        /// <summary>
        /// 同步获取指定数量图像数据
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        private List<ushort[]> GetImage(int count = 10)
        {
            this.ImageBuffer.Clear();

            MaxFrames = count;
            StartAcq();
            bool result = _waitHandle.WaitOne(60000, true);
            if (!result)
            {
            }
            return ImageBuffer.ToList();
        }


        /// <summary>
        /// 同步获取未矫正的指定数量图像数据
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public List<ushort[]> GetSourceImage(int count = 10)
        {
            //取消所有校正
            //   NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            //   NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_NO);
            //   NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_NO);

            return GetImage(count);
        }
        /// <summary>
        /// 获取offset后图像
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ushort[]> GetOffsetImage(int count = 10)
        {
            StartOffset();
            _offsetWaitHandle.WaitOne(600000, true);

            Thread.Sleep(5000);
            //NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            //NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_SOFT);
            //NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_NO);

            return GetImage(count);
        }

        /// <summary>
        /// 获取OffsetGain后图像
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ushort[]> GetGainImage(int count = 10, bool runGainCal = true)
        {
            if (runGainCal)
            {
                StartGain();
                _gainWaitHandle.WaitOne(60000, true);
                Thread.Sleep(1000);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    CMessageBox.Show("请打开 X光");
                }));
            }

            //NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            //NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_SOFT);
            //NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_SOFT);
            return GetImage(count);
        }

        //public void SaveFile(List<ushort[]> list, string directory, FileType type = FileType.DICOM)
        //{
        //    try
        //    {
        //        if (!System.IO.Directory.Exists(directory))
        //        {
        //            System.IO.Directory.CreateDirectory(directory);
        //        }
        //        string fileExtention = type == FileType.DICOM ? ".DCM" : ".RAW";
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            SaveFile(System.IO.Path.Combine(directory, i.ToString() + fileExtention), list[i], type);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        //    /// <summary>
        //    /// 保存RAW
        //    /// </summary>
        //    /// <param name="directory"></param>
        //    /// <param name="name"></param>
        //    /// <param name="data"></param>
        //    private void SaveFile(string fileName, ushort[] data, FileType type = FileType.RAW)
        //    {
        //        byte[] buf = new byte[data.Length * 2];
        //        for (int j = 0; j < data.Length; j++)
        //        {
        //            BitConverter.GetBytes(data[j]).CopyTo(buf, j * 2);
        //        }

        //        if (type == FileType.RAW)
        //        {
        //            System.IO.File.WriteAllBytes(fileName, buf);
        //        }
        //        else if (type == FileType.DICOM)
        //        {
        //            ClearCanvas.Dicom.DicomFile file = new ClearCanvas.Dicom.DicomFile();
        //            if (!System.IO.File.Exists("template.dcm"))
        //            {
        //                throw new System.IO.FileNotFoundException("未找到模板文件template.dcm");
        //            }
        //            file.Load("template.dcm");
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.PixelData).Values = buf;
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.Rows).SetUInt16(0, (ushort)_imageHeight);
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.Columns).SetUInt16(0, (ushort)_imageWidth);
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.BitsStored).SetUInt16(0, 16);
        //            file.Save(fileName);
        //        }
        //    }
        //    #endregion

        //    internal static void SaveFile(ImageInfo avgImage, string fileName, FileType type = FileType.DICOM)
        //    {
        //        string dir = System.IO.Path.GetDirectoryName(fileName);
        //        if (!System.IO.Directory.Exists(dir))
        //        {
        //            System.IO.Directory.CreateDirectory(dir);
        //        }

        //        byte[] buf = new byte[avgImage.Width * avgImage.Height * 2];
        //        for (int j = 0; j < avgImage.PixelData.Length; j++)
        //        {
        //            BitConverter.GetBytes(avgImage.PixelData[j]).CopyTo(buf, j * 2);
        //        }

        //        if (type == FileType.RAW)
        //        {
        //            System.IO.File.WriteAllBytes(fileName, buf);
        //        }
        //        else if (type == FileType.DICOM)
        //        {
        //            ClearCanvas.Dicom.DicomFile file = new ClearCanvas.Dicom.DicomFile();
        //            if (!System.IO.File.Exists("template.dcm"))
        //            {
        //                throw new System.IO.FileNotFoundException("未找到模板文件template.dcm");
        //            }
        //            file.Load("template.dcm");
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.PixelData).Values = buf;
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.Rows).SetUInt16(0, avgImage.Height);
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.Columns).SetUInt16(0, avgImage.Width);
        //            file.DataSet.GetAttribute(ClearCanvas.Dicom.DicomTags.BitsStored).SetUInt16(0, avgImage.Bits);

        //            file.Save(fileName);
        //        }
        //    }

        //}
        /// <summary>
        /// 临时图像存储
        /// </summary>
        public class TempImage
        {
            /// <summary>
            /// width
            /// </summary>
            public int SizeX { set; get; }
            /// <summary>
            /// height
            /// </summary>
            public int SizeY { set; get; }
            /// <summary>
            /// pixelData
            /// </summary>
            public ushort[] Pixel { set; get; }
        }

        //public bool NV_SetGain(NV_Gain nV_Gain)
        //{
        //    // HBI_FPD_DLL.HBI_SetGainMode(HBI_FPD_DLL._handel,);
        //    return NVDentalSDK.NV_SetGain(nV_Gain) == NV_StatusCodes.NV_SC_SUCCESS;
        //}
        public bool HB_SetGain(int mode) {

            int ret = HBI_FPD_DLL.HBI_SetGainMode(HBI_FPD_DLL._handel, mode);
            return ret==0;
        }

        public bool HB_SetAqcSpanTime(int p)
        {
            ShowMessage("采集帧率(ms) " + p.ToString() );
            int ret = HBI_FPD_DLL.HBI_SetAcqSpanTm(HBI_FPD_DLL._handel,p);

            return true;
          //  int ret = HBI_FPD_DLL.HBI_SetSinglePrepareTime(HBI_FPD_DLL._handel, p);
          //  return ret == 0;
            // return NVDentalSDK.NV_SetExpTime(p) == NV_StatusCodes.NV_SC_SUCCESS;
        }

   

        public bool HB_SetMaxFrames(int p)
        {
            MaxFrames = p;
            return true;
            //   return NVDentalSDK.NV_SetMaxFrames(p) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool HB_SetBinningMode(byte mode) {

            int ret = HBI_FPD_DLL.HBI_SetBinning(HBI_FPD_DLL._handel, mode);

            switch (mode) {
                case 1: {

                        break;
                    }
                case 2:
                    {
                        _imageWidth = _detectorWidth / 2;
                        _imageHeight = _detectorHeight / 2;
                        break;
                    }
                case 4:
                    {
                        _imageWidth = _detectorWidth / 4;
                        _imageHeight = _detectorHeight / 4;
                        break;
                    }
                default:
                    {
                        ShowMessage("Unsupport bining mode");
                        break;
                    }

            }

            return ret == 0;
        }


        //public bool NV_SetBinningMode(NV_BinningMode nV_BinningMode)
        //{
        //    bool res = NVDentalSDK.NV_SetBinningMode(nV_BinningMode) == NV_StatusCodes.NV_SC_SUCCESS;
        //    if (nV_BinningMode == NV_BinningMode.NV_BINNING_2X2)
        //    {
        //        _imageWidth = _detectorWidth / 2;
        //        _imageHeight = _detectorHeight / 2;
        //    }
        //    else
        //    {
        //        _imageWidth = _detectorWidth;
        //        _imageHeight = _detectorHeight;
        //    }

        //    return res;
        //}

        //public bool NV_SetShutterMode(NV_ShutterMode nV_ShutterMode)
        //{
        //    return NVDentalSDK.NV_SetShutterMode(nV_ShutterMode) == NV_StatusCodes.NV_SC_SUCCESS;
        //}

      

        public bool HB_SetAcquisitionMode(int t) {

            ShowMessage("DEBUG  固定设置触发模式为 7");
            // 这里出发模式只能选择07
            int ret = HBI_FPD_DLL.HBI_UpdateTriggerMode(HBI_FPD_DLL._handel,7);
            return ret == 0;    
        }

        //public bool NV_SetAcquisitionMode(NV_AcquisitionMode nV_AcquisitionMode)
        //{
        //    return NVDentalSDK.NV_SetAcquisitionMode(nV_AcquisitionMode) == NV_StatusCodes.NV_SC_SUCCESS;
        //}

        public bool NV_SetOffsetCal(HB_OffsetCorrType nV_CorrType)
        {
            m_pCorrect.ucOffsetCorrection = (char)nV_CorrType;
            return true;
           // return NVDentalSDK.NV_SetOffsetCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetGainCal(HB_CorrType nV_CorrType)
        {

            m_pCorrect.ucGainCorrection = (char)nV_CorrType;
            return true;
           // return NVDentalSDK.NV_SetGainCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetDefectCal(HB_CorrType nV_CorrType)
        {
            m_pCorrect.ucDefectCorrection = (char)nV_CorrType;
            return true;
            //return NVDentalSDK.NV_SetDefectCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool HBUpdateCorrectEnable() {

            
           // int ret = HBI_FPD_DLL.HBI_UpdateCorrectEnable(HBI_FPD_DLL._handel, ref m_pCorrect);
            int ret = HBI_FPD_DLL.HBI_TriggerAndCorrectApplay(HBI_FPD_DLL._handel,7, ref m_pCorrect);
            ShowMessage(" ucDefectCorrection " + ((int)m_pCorrect.ucDefectCorrection).ToString() +
                " ucGainCorrection " + ((int)m_pCorrect.ucGainCorrection).ToString() +
                " ucDefectCorrection  " + ((int)m_pCorrect.ucDefectCorrection).ToString());

            if (ret == 0)
            {
                ShowMessage("\tHBI_UpdateCorrectEnable success!\n");
                return true;
            }
            else
            {
                ShowMessage("\tHBI_UpdateCorrectEnable failed!");
                return false;
            }
        }

        public bool NV_SaveParamFile()
        {
            return true;
            //return NVDentalSDK.NV_SaveParamFile() == NV_StatusCodes.NV_SC_SUCCESS;
        }

        // 固件最新参数数据
        private void btnGetFirmwareCfg_Click()
        {
            //if (!m_bOpen)
            //{
            //    System.Windows.Forms.MessageBox.Show("warnning:disconnect!");
            //    return;
            //}
            //
            m_pLastRegCfg = new RegCfgInfo();//RegCfgInfo 1024
            int _ret = Marshal.SizeOf(m_pLastRegCfg);
            _ret = HBI_FPD_DLL.HBI_GetDevCfgInfo(HBI_FPD_DLL._handel, ref m_pLastRegCfg);       //获取固件参数，连接后即可获取参数
            if (_ret != 0) { Log("Error,HBI_GetDevCfgInfo" + _ret.ToString()); return; }   // WriteLog("HBI_GetDevCfgInfo:\n", img_pro.nwidth, img_pro.nheight);

            // 测试，置零，检查结果
            ushort usValue = (ushort)(((m_pLastRegCfg.m_EtherInfo.m_sDestUDPPort & 0xff) << 8) | ((m_pLastRegCfg.m_EtherInfo.m_sDestUDPPort >> 8) & 0xff));// 高低位需要转换
            Log(string.Format("\tSourceIP:{0}.{1}.{2}.{3}:{4}\n",
               (int)(m_pLastRegCfg.m_EtherInfo.m_byDestIP[0]), (int)(m_pLastRegCfg.m_EtherInfo.m_byDestIP[1]),
                (int)(m_pLastRegCfg.m_EtherInfo.m_byDestIP[2]), (int)(m_pLastRegCfg.m_EtherInfo.m_byDestIP[3]), (ushort)(usValue)));
            usValue = (ushort)(((m_pLastRegCfg.m_EtherInfo.m_sSourceUDPPort & 0xff) << 8) | ((m_pLastRegCfg.m_EtherInfo.m_sSourceUDPPort >> 8) & 0xff));
            Log(string.Format("\tDestIP:{0}.{1}.{2}.{3}:{4}\n", (int)(m_pLastRegCfg.m_EtherInfo.m_bySourceIP[0]), (int)(m_pLastRegCfg.m_EtherInfo.m_bySourceIP[1]),
                (int)(m_pLastRegCfg.m_EtherInfo.m_bySourceIP[2]),
                (int)(m_pLastRegCfg.m_EtherInfo.m_bySourceIP[3]), (ushort)(usValue)));

            if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize >= 0x01 && m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize <= 0x08)
            {
                if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize == 0x01)
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:4343\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
                else if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize == 0x02)
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:3543\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
                else if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize == 0x03)
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:1613\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
                else if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize == 0x04)
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:3030\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
                else if (m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize == 0x05)
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:2530\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
                else
                    Log(string.Format("\tPanelSize:0x{0:X000},fpd type:3025\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
            }
            else
            {
                Log(string.Format("\tErr:fpd property:Do not know!fpd type:0x{0:X000}\n", (int)m_pLastRegCfg.m_SysBaseInfo.m_byPanelSize));
            }
            Log(string.Format("\twidth={0},hight={1}\n", m_pLastRegCfg.m_SysBaseInfo.m_sImageWidth, m_pLastRegCfg.m_SysBaseInfo.m_sImageHeight));
            Log("\tdatatype is unsigned char.\n");
            Log("\tdatabit is 16bits.\n");
            Log("\tdata is little endian.\n");

           
            // 图像分辨率
            _imageWidth = m_pLastRegCfg.m_SysBaseInfo.m_sImageWidth;
            _imageHeight = m_pLastRegCfg.m_SysBaseInfo.m_sImageHeight;
            Log(string.Format("\tImage width={0},hight={1}\n", _imageWidth, _imageHeight));
            // 连续采集时间间隔
            UInt32 value = (UInt32)m_pLastRegCfg.m_SysCfgInfo.m_unSelfDumpingSpanTime;

        }

        // 获取图像数据信息
        private void btnImageProperty_Click()
        {
            Log("get Image property begin!\n");
            IMAGE_PROPERTY img_pro = new IMAGE_PROPERTY();
            int ret = HBI_FPD_DLL.HBI_GetImageProperty(HBI_FPD_DLL._handel, ref img_pro);
            if (ret == 0)
            {
                Log(string.Format("HBI_GetImageProperty:width={0},hight={1}\n", img_pro.nwidth, img_pro.nheight));
                //
                if (img_pro.datatype == 0) Log("\tdatatype is unsigned char.\n");
                else if (img_pro.datatype == 1) Log("\tdatatype is char.\n");
                else if (img_pro.datatype == 2) Log("\tdatatype is unsigned short.\n");
                else if (img_pro.datatype == 3) Log("\tdatatype is float.\n");
                else if (img_pro.datatype == 4) Log("\tdatatype is double.\n");
                else Log("\tdatatype is not support.\n");
                //
                if (img_pro.ndatabit == 0) Log("\tdatabit is 16bits.\n");
                else if (img_pro.ndatabit == 1) Log("\tdatabit is 14bits.\n");
                else if (img_pro.ndatabit == 2) Log("\tdatabit is 12bits.\n");
                else if (img_pro.ndatabit == 3) Log("\tdatabit is 8bits.\n");
                else Log("\tdatatype is unsigned char.\n");
                //
                if (img_pro.nendian == 0) Log("\tdata is little endian.\n");
                else Log("\tdata is bigger endian.\n");
            }
            else
            {
                Log("HBI_GetImageProperty failed!\n");
            }
        }



    }
}
