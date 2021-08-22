using System;
using System.Collections.Generic;
using System.Linq;
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

        // add by zhao
        public UIntPtr _HBI_Handle { get; set; }
        public int LastHBIReturnValue { get; set; }

        public FPD_AQC_MODE global_aqc_mode;

        public string LocalIpPort;
        public string RemoteIpPort;

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

            unsafe
            {
                //add by zhao 
                HBIEventCallback = new USER_CALLBACK_HANDLE_ENVENT(RecieveImageAndEvent);
                HBIProcessCallback = new USER_CALLBACK_HANDLE_PROCESS(HandleProcessCallback);
            }

        }

        /// <summary>
        /// 程序当前线程Dispatcher
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher { get { return Application.Current.Dispatcher; } }
        /// <summary>
        /// 获取上次出错信息
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            //  StringBuilder _error = new StringBuilder(1024);
            //   NVDentalSDK.NV_LastErrorMsg(_error, 1024);
            //  return _error.ToString();
            return HBSDK.GetErrorMsgByCode(LastHBIReturnValue);
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
            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;


            FPD_AQC_MODE fPD_AQC_MODE_nframecount = MaxFrames;

            LastHBIReturnValue = HBSDK.HBI_SingleAcquisition(_HBI_Handle, fPD_AQC_MODE_nframecount);
            // HBSDK.HBI_LiveAcquisition(_HBI_Handle,fPD_AQC_MODE_nframecount);

            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
            {
                ShowMessage("StartAcq Failed——" + GetLastError(), true);
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


            LastHBIReturnValue = HBSDK.HBI_StopAcquisition(_HBI_Handle);
            if ((HBIRETCODE)LastHBIReturnValue != HBIRETCODE.HBI_SUCCSS)
            {
                ShowMessage("StopAcq Failed——" + GetLastError(), true);
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

            // 首先销毁之前的句柄
            //NVDentalSDK.NV_CloseDet();
            HBSDK.HBI_Destroy(_HBI_Handle);

            _HBI_Handle =  HBSDK.HBI_Init();

            // 然后再注册回调函数
            LastHBIReturnValue = HBSDK.HBI_RegEventCallBackFun(_HBI_Handle, HBIEventCallback);

            unsafe {
                LastHBIReturnValue = HBSDK.HBI_RegProgressCallBack(_HBI_Handle, HBIProcessCallback, null);
            }

            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
                res += ("HBI_RegEventCallBackFun Failed");
            else
                ShowMessage("HBI_RegEventCallBackFun success.");

            string[] local_ip_port = LocalIpPort.Split(':');
            string local_ip = local_ip_port.ElementAt(0);
            Int16 local_port = Convert.ToInt16(local_ip_port.ElementAt(1));

            string[] remote_ip_port = RemoteIpPort.Split(':');
            string remote_ip = remote_ip_port.ElementAt(0);
            Int16 remote_port = Convert.ToInt16(remote_ip_port.ElementAt(1));

            LastHBIReturnValue = HBSDK.HBI_ConnectDetector(_HBI_Handle, 
                local_ip,(ushort)local_port,
                 remote_ip, (ushort)remote_port, 
                 30);
            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
            {
                res += "探测器连接失败。" + GetLastError() + "\n" +
                    local_ip.ToString() +":" + local_port.ToString() + " <--->" + 
                    remote_ip.ToString() + ":" + remote_port.ToString();
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


                IMAGE_PROPERTY iMAGE_PROPERTY;
                LastHBIReturnValue = HBSDK.HBI_GetImageProperty(_HBI_Handle, out iMAGE_PROPERTY);

                if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
                {
                    res += ("HBI_GetImageProperty failed");
                }
                else
                {
                    _detectorHeight = 3072;
                    _detectorWidth = 3072;

                    _imageWidth = (int)iMAGE_PROPERTY.nwidth;
                    _imageHeight = (int)iMAGE_PROPERTY.nheight;
                    _bits = (int)iMAGE_PROPERTY.ndatabit;

                    Byte binning;
                    HBSDK.HBI_GetBinning(_HBI_Handle, out binning);

                    res += binning.ToString();
                    //if (binning == 2)
                    //{
                    //    HBSDK.HBI_SetImageCalibration
                    //    NVDentalSDK.NV_SetImageRange(0, 0, ImageWidth / 2, ImageHeight / 2);
                    //}
                    //else
                    //    NVDentalSDK.NV_SetImageRange(0, 0, ImageWidth, ImageHeight);
                    //NVDentalSDK.NV_SaveParamFile();
                    ////NVDentalSDK.NV_LoadParamFile();
                }
                res += (string.Format("Size w:{0} h:{1} bits:{2}", ImageWidth, ImageHeight, Bits));
                res += "探测器已连接。";
            }
            return true;
        }

        private void ConnBreak()
        {
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
        private unsafe int RecieveImageAndEvent(Byte cmd, void* buff, int len, int nID)
        {
            CALLBACK_EVENT_COMM_TYPE command = (CALLBACK_EVENT_COMM_TYPE)cmd;

            if ((command == CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_SINGLE_IMAGE) || (command == CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_MULTIPLE_IMAGE) ||
                (command == CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_ROM_UPLOAD) || (command == CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_RAM_UPLOAD))
            {
                if (buff == null || len == 0)
                {
                    ShowMessage("注册回调函数参数异常!");
                    return 0;
                }
            }



            switch (command)
            {
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_NET_ERR_MSG:
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
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_ROM_UPLOAD:
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
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_SINGLE_IMAGE:
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_MULTIPLE_IMAGE:
                    {
                        NV_ImageInfo _ImageInfo;
                        
                        //todo: ZQB 像素格式这里需要确认下是怎么搞？
                        _ImageInfo.iPixelType = (Int32)NV_PixelFormat.NV_PF_Mono16;	///< 像素格式
                        _ImageInfo.iSizeX = _imageWidth;             ///< 图像宽
                        _ImageInfo.iSizeY = _imageHeight;             ///< 图像高
                        _ImageInfo.iImageSize = len * sizeof(ushort);         ///< 图像所占的字节数

                        _ImageInfo.pImageBuffer = (ushort*)buff;		///< 图像数据指针
                        _ImageInfo.iTimeStamp = System.DateTime.Now.Second;			///< 时间戳
                        _ImageInfo.iMissingPackets = _MissedPacketNum;    ///< 丢失的包数量
                        _ImageInfo.iAnnouncedBuffers = 0;  ///< 声明缓存区大小[暂为0]
                        _ImageInfo.iQueuedBuffers = 0;     ///< 队列缓存区大小[暂为0]
                        _ImageInfo.iOffsetX = 0;           ///< x方向偏移量[暂未设置]
                        _ImageInfo.iOffsetY = 0;           ///< y方向偏移量[暂未设置]
                        _ImageInfo.iAwaitDelivery = 0;     ///< 等待传送的帧数[暂为0]
                        _ImageInfo.iBlockId = 0;          ///< GVSP协议的block-id

                        ShowImageCallBack(0, _ImageInfo);

                        break;
                    }
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_THREAD_EVENT:
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
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_FPD_STATUS:
                    {
                        ShowMessage("TYPE_FPD_STATUS, command= " + cmd.ToString());
                        if (len == 5)
                        {
                            ShowMessage("Stop Acquisition", true);

                            // 完成OFFSET校正
                            if (global_aqc_mode.nLiveMode == LIVE_MODE.ACQ_OFFSET_T)//Only Template
                            {
                                FinishedOffsetEvent(true);
                            }

                        }
                        break;
                    }
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_GAIN_ERR_MSG:
                    {
                        break;
                    }
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_DEFECT_ERR_MSG:
                    {
                        break;
                    }
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_PACKET_MISS:
                case CALLBACK_EVENT_COMM_TYPE.ECALLBACK_TYPE_PACKET_MISS_MSG:
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
        private unsafe int HandleProcessCallback(Byte cmd, int retcode, ushort* buff) {
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
            if (image.iMissingPackets > 0)
            {
                return;//跳过丢包图像 
            }

            ushort[] buffer = new ushort[image.iSizeX * image.iSizeY];

            for (int i = 0; i < image.iSizeX * image.iSizeY; i++)
            {
                buffer[i] = image.pImageBuffer[i];
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
        private void ShowMessage(string p, bool isShowMessageBox = false)
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
            Console.WriteLine(p);
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

            LastHBIReturnValue = HBSDK.HBI_SetBinning(_HBI_Handle, bin);

            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
            {
                res += ("Set BinningMode Failed:" + GetLastError() + "\n");
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
            if (HBSDK.HBI_SetGainMode(_HBI_Handle, gain) != (int)HBIRETCODE.HBI_SUCCSS)
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


            global_aqc_mode.bSimpleGT = true;
            global_aqc_mode.nLiveMode = LIVE_MODE.ACQ_OFFSET_T;


            LastHBIReturnValue = HBSDK.HBI_LiveAcquisition(_HBI_Handle, global_aqc_mode);

            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS) {
                ShowMessage(GetLastError(), false);
                return;
            }

            // NVDentalSDK.NV_ResetCorrection();
            //  NV_StatusCodes result = NVDentalSDK.NV_RunOffsetCalThread(FinishedOffsetEvent);
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

            CALIBRATE_INPUT_PARAM cALIBRATE_INPUT_PARAM = 1;
            LastHBIReturnValue = HBSDK.HBI_InitDefectMode(_HBI_Handle, cALIBRATE_INPUT_PARAM);

            if (LastHBIReturnValue != (int)HBIRETCODE.HBI_SUCCSS)
            {
                FinishedDetectEvent(false);
            }
            else {
                FinishedDetectEvent(true);
            }

            // NVDentalSDK.NV_ResetCorrection();
            //  NV_StatusCodes result = NVDentalSDK.NV_RunAutoDefectCalThread(FinishedDetectEvent);
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

            // 1. 注册回调函数

            // 2. 设置触发模式为 Firmware PreOffset
            IMAGE_CORRECT_ENABLE iMAGE_CORRECT_ENABLE = false;
            iMAGE_CORRECT_ENABLE.bFeedbackCfg = false;
            iMAGE_CORRECT_ENABLE.ucOffsetCorrection = 3;

            LastHBIReturnValue = HBSDK.HBI_UpdateCorrectEnable(_HBI_Handle, iMAGE_CORRECT_ENABLE);

            // 3.初始化 GAIN 校正模型
            CALIBRATE_INPUT_PARAM cALIBRATE_INPUT_PARAM = 1;
            LastHBIReturnValue = HBSDK.HBI_InitGainMode(_HBI_Handle, cALIBRATE_INPUT_PARAM);



            //NVDentalSDK.NV_ResetCorrection();
            //NVDentalSDK.NV_RunGainCalThread(FinishedGainEvent, OpenXRayEvent, CloseXRayEvent);
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
            NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_SOFT);
            NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_NO);

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

            NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_SOFT);
            NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_SOFT);
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

        public bool NV_SetGain(NV_Gain nV_Gain)
        {
            // HBSDK.HBI_SetGainMode(_HBI_Handle,);
            return NVDentalSDK.NV_SetGain(nV_Gain) == NV_StatusCodes.NV_SC_SUCCESS;
        }
        public bool HB_SetGain(int mode) {

            int ret = HBSDK.HBI_SetGainMode(_HBI_Handle, mode);
            return ret == (int)HBIRETCODE.HBI_SUCCSS;
        }

        public bool HB_SetExpTime(int p)
        {
            int ret = HBSDK.HBI_SetSinglePrepareTime(_HBI_Handle, p);
            return ret == (int)HBIRETCODE.HBI_SUCCSS;
            // return NVDentalSDK.NV_SetExpTime(p) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetMaxFrames(int p)
        {
            return NVDentalSDK.NV_SetMaxFrames(p) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool HB_SetBinningMode(byte mode) {

            int ret = HBSDK.HBI_SetBinning(_HBI_Handle, mode);

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

            return ret == (int)HBIRETCODE.HBI_SUCCSS;
        }

        public bool NV_SetBinningMode(NV_BinningMode nV_BinningMode)
        {
            bool res = NVDentalSDK.NV_SetBinningMode(nV_BinningMode) == NV_StatusCodes.NV_SC_SUCCESS;
            if (nV_BinningMode == NV_BinningMode.NV_BINNING_2X2)
            {
                _imageWidth = _detectorWidth / 2;
                _imageHeight = _detectorHeight / 2;
            }
            else
            {
                _imageWidth = _detectorWidth;
                _imageHeight = _detectorHeight;
            }

            return res;
        }

        public bool NV_SetShutterMode(NV_ShutterMode nV_ShutterMode)
        {
            return NVDentalSDK.NV_SetShutterMode(nV_ShutterMode) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetOffsetCal(NV_CorrType nV_CorrType)
        {
            return NVDentalSDK.NV_SetOffsetCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool HB_SetAcquisitionMode(int t) {

            int ret = HBSDK.HBI_UpdateTriggerMode(_HBI_Handle,t);
            return ret == (int)HBIRETCODE.HBI_SUCCSS;    
        }

        public bool NV_SetAcquisitionMode(NV_AcquisitionMode nV_AcquisitionMode)
        {
            return NVDentalSDK.NV_SetAcquisitionMode(nV_AcquisitionMode) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetGainCal(NV_CorrType nV_CorrType)
        {
            return NVDentalSDK.NV_SetGainCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetDefectCal(NV_CorrType nV_CorrType)
        {
            return NVDentalSDK.NV_SetDefectCal(nV_CorrType) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SaveParamFile()
        {
            return NVDentalSDK.NV_SaveParamFile() == NV_StatusCodes.NV_SC_SUCCESS;
        }


    }
}
