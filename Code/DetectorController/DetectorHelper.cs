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
using NV.Config;
using NV.DRF.Controls;
using NV.DRF.Core.Common;
using NV.Infrastructure.UICommon;
using SerialPortController;
namespace Detector
{
    public delegate void ImageCallbackHandler(byte wnd, ImageData image);
    public delegate void TemperatureCallbackHandler(float a, float b);
    public delegate void SystemStatusCallbackHandler(int a, int b);
    public delegate void ExcutedFinishCallbackHandler(bool isSuccess);
    public delegate void ExcutedCallbackHandler();
    public delegate void ExcutedCallbackHandlerWithValue(int targetValue);

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

        RegCfgInfo m_pLastRegCfg; //记录固件所有配置数据  1024字节的结构体
        IMAGE_CORRECT_ENABLE m_pCorrect = new IMAGE_CORRECT_ENABLE();
        //    DOWNLOAD_FILE m_pdownloadmode = new DOWNLOAD_FILE();
        FPD_AQC_MODE m_stMode = new FPD_AQC_MODE();

        /// <summary>
        /// 高压控制器
        /// </summary>
        public static SerialPortControler_RS232PROTOCOL xRayControler = SerialPortControler_RS232PROTOCOL.Instance;

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
        private uint _bits, _detectorWidth, _detectorHeight, _imageWidth, _imageHeight;
        /// <summary>
        /// 数据位数
        /// </summary>
        public uint Bits { get { return _bits; } }
        /// <summary>
        /// 图像宽度
        /// </summary>
        public uint ImageWidth { get { return _imageWidth; } }
        /// <summary>
        /// 图像高度
        /// </summary>
        public uint ImageHeight { get { return _imageHeight; } }
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

        public bool StartSingleShot() {
            ShowMessage("StartSingleShot");
            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;
            int  ret = -1;

            if (true)
            {
                ret = HBI_FPD_DLL.HBI_SinglePrepare(HBI_FPD_DLL._handel);
                if (ret != 0)
                {
                    ShowMessage("HBI_SinglePrepare Failed——" + GetLastError(ret), true);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                FPD_AQC_MODE stMode = new FPD_AQC_MODE();
                stMode.eAqccmd = EnumIMAGE_ACQ_CMD.SINGLE_ACQ_DEFAULT_TYPE;
                stMode.eLivetype = EnumLIVE_ACQUISITION.ONLY_IMAGE;     // 1-固件做offset模板并上图；2-只上图；3-固件做只做offset模板。
                stMode.ndiscard = 0;     // 这里默认位0，不抛弃前几帧图像
                stMode.nframeid = 0;     // 这里默认位0
                HBI_FPD_DLL.HBI_SinglePrepare(HBI_FPD_DLL._handel);
                ret = HBI_FPD_DLL.HBI_SingleAcquisition(HBI_FPD_DLL._handel, stMode);
                if (ret != 0)
                {
                    ShowMessage("HBI_SingleAcquisition Failed——" + GetLastError(ret), true);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool StartAcq()
        {

            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;
            //  亦可不用切换到软触发模式下，发送连续采集1帧即可完成单帧采集
            FPD_AQC_MODE stMode = new FPD_AQC_MODE();
            stMode.eAqccmd = EnumIMAGE_ACQ_CMD.LIVE_ACQ_DEFAULT_TYPE;
            stMode.eLivetype = EnumLIVE_ACQUISITION.ONLY_IMAGE;     // 1-固件做offset模板并上图；2-只上图；3-固件做只做offset模板。
            stMode.ndiscard = 0;     // 这里默认位0，不抛弃前几帧图像
            stMode.nframeid = 0;     // 这里默认位0
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
            ShowMessage("StopAcq ");

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

            _detectorHeight = 3072;
            _detectorWidth = 3072;
            _imageHeight = _detectorHeight;
            _imageWidth = _detectorWidth;
            _bits = 16;

            // 因为要重新连接，所以需要先销毁
            HBI_FPD_DLL.HBI_Destroy(HBI_FPD_DLL._handel);
            HBI_FPD_DLL._handel = HBI_FPD_DLL.HBI_Init();

            // 然后再注册回调函数

            int ret = HBI_FPD_DLL.HBI_RegEventCallBackFun(HBI_FPD_DLL._handel, HBIEventCallback);
            if (ret != 0)
                res += ("HBI_RegEventCallBackFun Failed");
            else
                ShowMessage("HBI_RegEventCallBackFun success.");
            return true;
        }

        private void ConnBreak()
        {
            ShowMessage("ConnBreak ");
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
        // Notice: call back function
        // @USER_CALLBACK_HANDLE_ENVENT
        // @byteEventid:enum eEventCallbackCommType
        // @ufpdId:平板设备ID
        // @PVEventParam1:fpd config or image data buffer addr
        // @nEventParam2:参数2，例如data size或状态
        // @nEventParam3:参数3，例如帧率 frame rate或状态等
        // @nEventParam4:参数4，例如pcie事件id或预留扩展
        private int RecieveImageAndEvent(IntPtr _contex, int nDevId, byte byteEventId, IntPtr pvParam, int nlength, int param3, int param4)
        {
            eCallbackEventCommType command = (eCallbackEventCommType)byteEventId;
            int len = nlength;
            int ret = -1;
            if ((command == eCallbackEventCommType.ECALLBACK_TYPE_ROM_UPLOAD) || (command == eCallbackEventCommType.ECALLBACK_TYPE_RAM_UPLOAD) ||
                (command == eCallbackEventCommType.ECALLBACK_TYPE_FACTORY_UPLOAD))
            {
                if (pvParam == null || nlength == 0)
                {
                    ShowMessage("注册回调函数参数异常!");
                    return ret;
                }

                if (0 != nDevId)
                {
                    ShowMessage("warnning:RecieveImageAndEvent:\n");
                    return ret;
                }
            }
            else if ((command == eCallbackEventCommType.ECALLBACK_TYPE_SINGLE_IMAGE) ||
                (command == eCallbackEventCommType.ECALLBACK_TYPE_MULTIPLE_IMAGE) ||
                (command == eCallbackEventCommType.ECALLBACK_TYPE_PREVIEW_IMAGE) ||
                (command == eCallbackEventCommType.ECALLBACK_TYPE_OFFSET_TMP) ||
                (command == eCallbackEventCommType.ECALLBACK_OVERLAY_16BIT_IMAGE) ||
                (command == eCallbackEventCommType.ECALLBACK_OVERLAY_32BIT_IMAGE))
            {
                if (pvParam == null || nlength == 0)
                {
                    ShowMessage("注册回调函数参数异常!");
                    return ret;
                }

                if (0 != nDevId)
                {
                    ShowMessage("warnning:RecieveImageAndEvent:\n");
                    return ret;
                }
            }

            ShowMessage("================== RecieveImageAndEvent Id = 0X" + byteEventId.ToString("X2"));
            int status = -1;
            ret = 1;
            switch (command)
            {
                case eCallbackEventCommType.ECALLBACK_TYPE_FPD_STATUS:
                    {
                        if (len <= 0 && len >= -11)
                        {
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
                            else if (len == -8)
                                ShowMessage("ECALLBACK_TYPE_FPD_STATUS:update Firmware end!\n", true);
                            else if (len == -9)
                                ShowMessage("ECALLBACK_TYPE_FPD_STATUS:光纤已断开!!\n", true);
                            else if (len == -10)
                                ShowMessage("ECALLBACK_TYPE_FPD_STATUS:read ddr failed,try restarting the PCIe driver!\n", true);
                            else /*if (nlength == -11)*/
                                ShowMessage("ECALLBACK_TYPE_FPD_STATUS:ECALLBACK_TYPE_FPD_STATUS:is not jumb!\n", true);
                            status = (int)EFpdStatusType.FPD_STATUS_DISCONN;

                        }
                        else if (nlength == (int)EFpdStatusType.FPD_CONN_SUCCESS)
                        { // connect
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS,开始监听!\n");
                            status = (int)EFpdStatusType.FPD_CONN_SUCCESS;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_PREPARE_STATUS)
                        { // ready
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS,ready!\n");
                            status = (int)EFpdStatusType.FPD_PREPARE_STATUS;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_READY_STATUS)
                        { // busy
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS,busy!\n");
                            status = (int)EFpdStatusType.FPD_READY_STATUS;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_DOOFFSET_TEMPLATE)
                        { // prepare
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS,prepare!\n");
                            status = (int)EFpdStatusType.FPD_DOOFFSET_TEMPLATE;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_EXPOSE_STATUS)
                        { // busy expose
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Exposing!\n");
                            status = (int)EFpdStatusType.FPD_EXPOSE_STATUS;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_CONTINUE_READY)
                        { // continue ready
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Continue ready!\n");
                            status = (int)EFpdStatusType.FPD_CONTINUE_READY;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_DWONLOAD_GAIN)
                        { // download gain template
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Download gain template ack!\n");
                            status = (int)EFpdStatusType.FPD_DWONLOAD_GAIN;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_DWONLOAD_DEFECT)
                        { // download defect template
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Download defect template ack!\n");
                            status = (int)EFpdStatusType.FPD_DWONLOAD_DEFECT;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_DWONLOAD_OFFSET)
                        { // download offset template
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Download offset template ack!\n");
                            status = (int)EFpdStatusType.FPD_DWONLOAD_OFFSET;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_UPDATE_FIRMARE)
                        { // update firmware
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Update firmware!\n");
                            status = (int)EFpdStatusType.FPD_UPDATE_FIRMARE;
                        }
                        else if (nlength == (int)EFpdStatusType.FPD_RETRANS_MISS)
                        { // update firmware
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS:Retransmission!\n");
                            status = (int)EFpdStatusType.FPD_RETRANS_MISS;
                        }
                        else
                            ShowMessage("ECALLBACK_TYPE_FPD_STATUS,Err:Other error "+ nlength.ToString());


                    }
                    break;
                case eCallbackEventCommType.ECALLBACK_TYPE_SET_CFG_OK:
                    ShowMessage("ECALLBACK_TYPE_SET_CFG_OK:Reedback set rom param succuss!\n");
                    break;
                case eCallbackEventCommType.ECALLBACK_TYPE_ROM_UPLOAD:
                    {
                        ShowMessage("ECALLBACK_TYPE_ROM_UPLOAD");
                      //  RegCfgInfo* pRegCfg = (RegCfgInfo*)pvParam;
                    }
                    break;
                case eCallbackEventCommType.ECALLBACK_TYPE_SINGLE_IMAGE:
                case eCallbackEventCommType.ECALLBACK_TYPE_MULTIPLE_IMAGE:
                    {
                        ShowMessage("ECALLBACK_TYPE_SINGLE_IMAGE or ECALLBACK_TYPE_MULTIPLE_IMAGE");
                        unsafe
                        {
                            ImageData image;
                            image.uwidth = ((ImageData*)pvParam)->uwidth;
                            image.uheight = ((ImageData*)pvParam)->uheight;
                            image.uframeid = ((ImageData*)pvParam)->uframeid;
                            image.ndatabits = ((ImageData*)pvParam)->ndatabits;
                            image.databuff = ((ImageData*)pvParam)->databuff;

                            image.datalen = ((ImageData*)pvParam)->datalen;
                            ShowImageCallBack(0, image);
                        }
                      
                    }
                    break;

                case eCallbackEventCommType.ECALLBACK_TYPE_GENERATE_TEMPLATE:
                    {
                        
                        if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_BEGIN)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_BEGIN\n");
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_INVALVE_PARAM)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_INVALVE_PARAM:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_MALLOC_FAILED)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_MALLOC_FAILED:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_SEND_FAILED)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_SEND_FAILED:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_STATUS_ABORMAL)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_STATUS_ABORMAL:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_FRAME_NUM)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_FRAME_NUM::" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_TIMEOUT)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_TIMEOUT:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_MEAN)
                        {
                            //ECALLBACK_RAW_INFO* ptr = (ECALLBACK_RAW_INFO*)pvParam;
                            //if (ptr != NULL)
                            //{
                            //    ShowMessage("ECALLBACK_TEMPLATE_MEAN:%s,dMean=%0.2f\n", ptr->szRawName, ptr->dMean);
                            //}
                            ShowMessage("ECALLBACK_TEMPLATE_MEAN");
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_GENERATE)
                        {
                            if (param3 == (int)emUPLOAD_FILE_TYPE.OFFSET_TMP)
                                ShowMessage("ECALLBACK_TEMPLATE_GENERATE:OFFSET_TMP\n");
                            else if (param3 == (int)emUPLOAD_FILE_TYPE.GAIN_TMP)
                                ShowMessage("ECALLBACK_TEMPLATE_GENERATE:GAIN_TMP\n");
                            else if (param3 == (int)emUPLOAD_FILE_TYPE.DEFECT_TMP)
                                ShowMessage("ECALLBACK_TEMPLATE_GENERATE:DEFECT_TMP,bad point= "+ nlength.ToString());
                            else
                                ShowMessage("ECALLBACK_TEMPLATE_GENERATE:nid=:" + param3.ToString());
                        }
                        else if (nlength == (int)eCallbackTemplateStatus.ECALLBACK_TEMPLATE_RESULT)
                        {
                            ShowMessage("ECALLBACK_TEMPLATE_RESULT:" + param3.ToString());
                        }
                        else
                        {// other
                            ShowMessage("other");
                        }
                    }
                    break;
                case eCallbackEventCommType.ECALLBACK_OVERLAY_16BIT_IMAGE:
                    {
                        ShowMessage("ECALLBACK_OVERLAY_16BIT_IMAGE");
                    }
                    break;
                case eCallbackEventCommType.ECALLBACK_OVERLAY_32BIT_IMAGE:
                    {
                        ShowMessage("ECALLBACK_OVERLAY_32BIT_IMAGE");
                    }
                    break;
                case (eCallbackEventCommType.ECALLBACK_TYPE_GAIN_ERR_MSG):
                    {
                        if (len == 0)
                        {
                            bGainsetTemplateOk = true; // 表示生成gain模板成功
                            ShowMessage("ECALLBACK_TYPE_GAIN_ERR_MSG,bGainsetTemplateOk is true!\n");
                            FinishedGainEvent(true);
                        }
                    }

                    break;
                case (eCallbackEventCommType.ECALLBACK_TYPE_DEFECT_ERR_MSG):
                    {
                        if (len == 0)
                        {
                            bDefectAcqFinished = true; // 表示生成defect模板成功
                            ShowMessage("ECALLBACK_TYPE_DEFECT_ERR_MSG,bDefectAcqFinished is true!\n");
                            FinishedDetectEvent(true);
                        }
                    }

                    break;
                case eCallbackEventCommType.ECALLBACK_TYPE_PACKET_MISS:
                case eCallbackEventCommType.ECALLBACK_TYPE_PACKET_MISS_MSG:
                    {
                        ShowMessage("Packet miss  " + len.ToString());
                        _MissedPacketNum = len;

                    }
                    break;

                case eCallbackEventCommType.ECALLBACK_TYPE_THREAD_EVENT:
                    {
                        if (len == 100)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,start recv data!\n");
                        else if (len == 101)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,end recv data!\n");
                        else if (len == 104)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Packet Retransmission:start recv data!\n");
                        else if (len == 105)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Frame Retransmission:start recv data!\n");
                        else if (len == 106)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Frame loss retransmission over,end recv data!\n");
                        else if (len == 107)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,image buff is null:end recv data!\n");
                        else if (len == 108)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Generate Offset Template:start thread!\n");
                        else if (len == 109)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Generate Offset Template:end thread!\n");
                        else if (len == 110)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Generate Gain Template:start thread!\n");
                        else if (len == 111)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,Generate Gain Template:end thread!\n");
                        else if (len == 112)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,offset calibrate:success!\n");
                        else if (len == 113)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,offset calibrate:failed!\n");
                        else if (len == 114)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,gain calibrate:success!\n");
                        else if (len == 115)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,gain calibrate:failed!\n");
                        else if (len == 116)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,defect calibrate:success!\n");
                        else if (len == 117)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,defect calibrate:failed!\n");
                        else if (len == 118)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,InitGainTemplate:failed!\n");
                        else if (len == 119)
                            ShowMessage("ECALLBACK_TYPE_THREAD_EVENT,firmare offset calibrate:success!\n");
                        else
                            ShowMessage(string.Format("ECALLBACK_TYPE_THREAD_EVENT,Err:未知错误[{0}]\n", len));
                    }        
                    break;

                default:
                    {
                    //    ShowMessage("ECALLBACK_TYPE_INVALID, command " + cmd.ToString(), true);
                        break;
                    }
            }
            return 0;
        }


        /// <summary>
        /// 实时处理探测器采集到的图像
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="image"></param>
        private unsafe void ReceiveImage(byte wnd, ImageData image)
        {
            ShowMessage("ReceiveImage ");
            /* 探测器分辨率大小 */
            if ((image.databuff == null) || ((image.datalen/sizeof(ushort)) != (_imageWidth * _imageHeight)))
            {
                ShowMessage("图像数据异常 ",true);
                return ;
            }

            ushort[] buffer = new ushort[image.uwidth * image.uheight];

            for (int i = 0; i < image.uwidth * image.uheight; i++)
            {
                buffer[i] = ((ushort*)image.databuff)[i];
            }

            PlayBuffer.Enqueue(buffer);
            if (IsStored)
            {
                ImageBuffer.Add(buffer);
            }

            count++;

            
            if (MaxFrames == 1) {
                // 单帧的时候，停止
                AcqMaxFrameEvent();
            }
        }
        /// <summary>
        /// 多帧叠加
        /// </summary>
        /// <param name="buffer"></param>
        private void MultiFramesOverlay(ref ushort[] buffer)
        {
            ShowMessage("MultiFramesOverlay ");
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
            ShowMessage("AcqMaxFrame ");
            
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
             System.Console.WriteLine("["+ DateTime.Now.ToString("HH:mm:ss.ffff") + "]" + p);
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
            if (HBI_FPD_DLL.HBI_SetPGALevel(HBI_FPD_DLL._handel, gain) != (int)HBIRETCODE.HBI_SUCCSS)
            {
                res += ("Set Gain Failed\n");
            }
            int  w = 0, h = 0;
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
        public void StartCorrectOffsetTemplate()
        {
            count = 0;
            int ret = HBI_FPD_DLL.HBI_GenerateTemplate(HBI_FPD_DLL._handel, EnumIMAGE_ACQ_CMD.OFFSET_TEMPLATE_TYPE);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplate failed!" + ret.ToString(), true);
                return;
            }
            else
            {
                ShowMessage("Do pre-offset template success!",false);
            }

        }
        /// <summary>
        /// 校正完毕，设置校正模式软件模式
        /// </summary>
        /// <param name="isSuccess"></param>
        private void FinishedOffset(bool isSuccess)
        {
            string msg = isSuccess ? "成功" : "失败";
            ShowMessage("Offset 校正" + isSuccess, true);
            _offsetWaitHandle.Set();
        }

        /// <summary>
        /// detect校正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        public bool StartCorrectDetectTemplate_Step1() {
            count = 0;

            bDefectAcqFinished = false;
            bDefectTemplateOk = false;
            bDownloadTemplateOk = false;
           // ShowMessage("第一步：第一组亮场(剂量要求：正常高压，毫安秒调节正常的 10%)",true);
            // 第一步：第一组亮场(剂量要求：正常高压，毫安秒调节正常的 10%)-发送采集命令
            int ret = HBI_FPD_DLL.HBI_GenerateTemplate(HBI_FPD_DLL._handel, EnumIMAGE_ACQ_CMD.DEFECT_TEMPLATE_GROUP1);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplate failed!" + ret.ToString(), true);
                return false;
            }
            else
            {
                ShowMessage("Do defect group1 success!", true);
                return true;
            }
        }
        public bool StartCorrectDetectTemplate_Step2()
        {
            // 第二步：第二组亮场(剂量要求：正常高压，毫安秒调节正常的 50%)-发送采集命令

            int ret = HBI_FPD_DLL.HBI_GenerateTemplate(HBI_FPD_DLL._handel, EnumIMAGE_ACQ_CMD.DEFECT_TEMPLATE_GROUP2);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplate failed!" + ret.ToString(), true);
                return false;
            }
            else
            {
                ShowMessage("Do defect group2 success!", true);
                return true;
            }
        }
        public bool StartCorrectDetectTemplate_Step3()
        {
            // 第三步：第三组亮场(剂量要求：正常高压，毫安秒调节正常的 100%)-发送采集命令
            int ret = HBI_FPD_DLL.HBI_GenerateTemplate(HBI_FPD_DLL._handel, EnumIMAGE_ACQ_CMD.DEFECT_TEMPLATE_GROUP3);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplate failed!" + ret.ToString(), true);
                return false;
            }
            else
            {
                ShowMessage("Do defect group3 success!", true);
                return true;
                
            }
        }


        public void StartCorrectDetectTemplate_Step4()
        {

            // 第四步：注册回调函数
            int ret = HBI_FPD_DLL.HBI_RegProgressCallBack(HBI_FPD_DLL._handel, DownloadCallBack, HBI_FPD_DLL._handel);
            if (ret != 0)
            {
                ShowMessage("err:HBI_RegProgressCallBack failed,TimeOut!", true);
                return;
            }
            else {
                ShowMessage("HBI_RegProgressCallBack success!");
            }
               
            // 第五步：将defect模板下载到固件
            HBI_FPD_DLL.HBI_DownloadTemplateByType(HBI_FPD_DLL._handel, 1);
            if (ret != 0)
            {
                ShowMessage("HBI_DownloadTemplateByType:detect template failed!ret:[{0}]" + ret.ToString());
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
                case (byte)(eCallbackUpdateFirmwareStatus.ECALLBACK_UPDATE_STATUS_START): // 初始化进度条	
                    {
                        ShowMessage(string.Format("DownloadCallBack, 开始下载:[{0}]\n", code));
                    }
                    break;
                case (byte)(eCallbackUpdateFirmwareStatus.ECALLBACK_UPDATE_STATUS_PROGRESS):// 进度 code：百分比（0~100）	
                    {
                        ShowMessage(string.Format("DownloadCallBack, 下载进度:[{0}]\n", code));
                    }
                    break;
                case (byte)(eCallbackUpdateFirmwareStatus.ECALLBACK_UPDATE_STATUS_RESULT):
                    {
                        if ((0 <= code) && (code <= 6))
                        {       // 显示Uploading gain template!
                            if (code == 0)
                            {       // offset模板上传完成
                                ShowMessage(" DownloadCallBack: download offset template!");
                            }
                            else if (code == 1)
                            {  // offset模板上传完成
                                ShowMessage(" DownloadCallBack: download gain template!");
                            }
                            else if (code == 2)
                            {  // gain模板上传完成
                                ShowMessage(" DownloadCallBack: download defect template!");
                            }
                            else if (code == 3)
                            {  // defect模板上传完成
                                ShowMessage(" DownloadCallBack: download offset finish!");
                            }
                            else if (code == 4)
                            {  // defect模板上传完成
                                ShowMessage(" DownloadCallBack: download gain finish!");
                            }
                            else if (code == 5)
                            {  // defect模板上传完成
                                ShowMessage(" DownloadCallBack: download defect finish!");

                            }
                            else/* if (code == 6)*/
                            {  // defect模板上传完成
                                ShowMessage(" DownloadCallBack: Download finish and sucess!");
                            }
                        }
                        else
                        {
                            if (code == -1)
                            {
                                ShowMessage(" DownloadCallBack: wait event other error!");
                            }
                            else if (code == -2)
                            {
                                ShowMessage(" DownloadCallBack: timeout!");
                            }
                            else if (code == -3)
                            {
                                ShowMessage(" DownloadCallBack: downlod offset failed!");
                            }
                            else if (code == -4)
                            {
                                ShowMessage(" DownloadCallBack: downlod gain failed!");
                            }
                            else if (code == -5)
                            {
                                ShowMessage(" DownloadCallBack: downlod defect failed!");
                            }
                            else if (code == -6)
                            {
                                ShowMessage(" DownloadCallBack: Download failed");
                            }
                            else if (code == -7)
                            {
                                ShowMessage(" DownloadCallBack: read offset failed!");
                            }
                            else if (code == -8)
                            {
                                ShowMessage(" DownloadCallBack: read gain failed!");
                            }
                            else if (code == -9)
                            {
                                ShowMessage(" DownloadCallBack: read defect failed!");
                            }
                            else
                            {
                                ShowMessage(" DownloadCallBack: unknown error!");
                            }
                        }
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
                string msg = isSuccess ? "成功" : "失败";
                ShowMessage("defect模板生成 " + isSuccess, true);
            }));
        }
        /// <summary>
        /// Gain校正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartCorrectGainTemplate()
        {
            bGainAcqFinished = false;
            bGainsetTemplateOk = false;
            bDownloadTemplateOk = false;

            xRayControler.XRayOn();
            Thread.Sleep(3000);
            // 第一步：一组亮场(剂量要求：正常高压和电流)-发送采集命令
            // 高压的剂量一般为正常采图的剂量即可，等高压到达稳定值后开始调用生成接口直到采图完成结束。
            int ret = HBI_FPD_DLL.HBI_GenerateTemplate(HBI_FPD_DLL._handel, EnumIMAGE_ACQ_CMD.GAIN_TEMPLATE_TYPE);
            if (ret != 0)
            {
                ShowMessage("HBI_GenerateTemplate failed!" + ret.ToString(),true);
                return;
            }
            

            // 第二步：注册回调函数
            ret = HBI_FPD_DLL.HBI_RegProgressCallBack(HBI_FPD_DLL._handel, DownloadCallBack, HBI_FPD_DLL._handel);
            if (ret != 0)
            {
                ShowMessage("HBI_RegProgressCallBack failed! ret: " + ret.ToString(),true);
                return;
            }
            else
                ShowMessage("HBI_RegProgressCallBack success!");

            xRayControler.XRayOff();
            // 第三步：将gain模板下载到固件
            HBI_FPD_DLL.HBI_DownloadTemplateByType(HBI_FPD_DLL._handel, 0);
            if (ret != 0)
            {
                ShowMessage("HBI_DownloadTemplateByType:gain template failed!ret:[{0}]" + ret.ToString(),true);
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
                ShowMessage("\tHBI_UpdateCorrectEnable failed!",true);
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
                string msg = isSuccess ? "成功" : "失败";
                ShowMessage("Gain校正" + msg,true);

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
            StartCorrectOffsetTemplate();
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
                StartCorrectGainTemplate();
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

        public bool HB_SetGain(int mode) {

            int ret = HBI_FPD_DLL.HBI_SetPGALevel(HBI_FPD_DLL._handel, mode);
            return ret==0;
        }

        public bool HB_SetAqcSpanTime(int p)
        {
            ShowMessage("采集帧率(ms) " + p.ToString() );
            int ret = HBI_FPD_DLL.HBI_SetSelfDumpingTime(HBI_FPD_DLL._handel,p);

            if (ret != 0)
            {
                ShowMessage("HBI_SetSelfDumpingTime Failed——" + GetLastError(ret), true);
                return false;
            }
            else
            {
                return true;

            }

        }

        public bool HBI_SetSinglePrepareTime(int p)
        {
            int ret = HBI_FPD_DLL.HBI_SetSinglePrepareTime(HBI_FPD_DLL._handel, p);
            ShowMessage("HBI_SetSinglePrepareTime " + p.ToString() + " ms" );
            if (ret != 0)
            {
                ShowMessage("HBI_SetSinglePrepareTime Failed——" + GetLastError(ret), true);
                return false;
            }
            else {
                return true;

            }
           
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

        public bool HB_SetTriggerMode(int t) {
            ShowMessage("设置触发模式为  " + t.ToString());
            int ret = HBI_FPD_DLL.HBI_UpdateTriggerMode(HBI_FPD_DLL._handel,t);
            if (ret != 0)
            {
                ShowMessage("  HBI_UpdateTriggerMode Failed", true);
                return false;
            }
            else {
                return true;
            }
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

        public bool HB_UpdateTriggerAndCorrectEnable(int trigger) {

            int ret = HBI_FPD_DLL.HBI_TriggerAndCorrectApplay(HBI_FPD_DLL._handel,trigger, ref m_pCorrect);
           // int ret = HBI_FPD_DLL.HBI_UpdateCorrectEnable(HBI_FPD_DLL._handel, ref m_pCorrect);
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
        public void btnGetFirmwareCfg_Click()
        {
            //if (!m_bOpen)
            //{
            //    System.Windows.Forms.MessageBox.Show("warnning:disconnect!");
            //    return;
            //}
            //
            m_pLastRegCfg = new RegCfgInfo();//RegCfgInfo 1024
            int _ret = Marshal.SizeOf(m_pLastRegCfg);
            _ret = HBI_FPD_DLL.HBI_GetFpdCfgInfo(HBI_FPD_DLL._handel, ref m_pLastRegCfg);       //获取固件参数，连接后即可获取参数
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

            Log(string.Format("\tm_pLastRegCfg.m_SysCfgInfo.m_byWorkMode ={0} \n", m_pLastRegCfg.m_SysCfgInfo.m_byWorkMode));

        }
        // 获取SDK版本信息
        public void btnGetSdkVer_Click()
        {
            System.Text.StringBuilder strSDKVerionbuf = new System.Text.StringBuilder(64);//
            int _ret = HBI_FPD_DLL.HBI_GetSDKVerion(HBI_FPD_DLL._handel, strSDKVerionbuf);
            if (0 != _ret)
            {
                Log("HBI_GetSDKVerion failed!");
                return;
            }
            else
            {
                Log("HBI_GetSDKVerion:" + strSDKVerionbuf.ToString());
            }
        }
        // 获取固件版本信息
        public void btnFirmwareVer_Click()
        {
            System.Text.StringBuilder strFirmwareVersion = new System.Text.StringBuilder(64);//
            int _ret = HBI_FPD_DLL.HBI_GetFirmareVerion(HBI_FPD_DLL._handel, strFirmwareVersion);
            if (0 != _ret)
            {
                Log("HBI_GetFirmareVerion failed!");
                return;
            }
            else
            {
                Log("HBI_GetFirmareVerion:" + strFirmwareVersion.ToString());
            }
        }
        // 获取图像数据信息
        public void btnGetImageProperty()
        {
            Log("get Image property begin!\n");
            IMAGE_PROPERTY img_pro = new IMAGE_PROPERTY();
            int ret = HBI_FPD_DLL.HBI_GetImageProperty(HBI_FPD_DLL._handel, ref img_pro);
            if (ret == 0)
            {
                _detectorHeight = img_pro.nheight;
                _detectorWidth = img_pro.nwidth;
                _imageHeight = _detectorHeight;
                _imageWidth = _detectorWidth;
                Log(string.Format("HBI_GetImageProperty:width={0},hight={1}\n", img_pro.nwidth, img_pro.nheight));
                //
                if (img_pro.datatype == 0) Log("\tdatatype is unsigned char.\n");
                else if (img_pro.datatype == 1) Log("\tdatatype is char.\n");
                else if (img_pro.datatype == 2) Log("\tdatatype is unsigned short.\n");
                else if (img_pro.datatype == 3) Log("\tdatatype is float.\n");
                else if (img_pro.datatype == 4) Log("\tdatatype is double.\n");
                else Log("\tdatatype is not support.\n");
                //
                if (img_pro.ndatabit == 0)
                {
                    _bits = 16;
                    Log("\tdatabit is 16bits.\n");
                }
                else if (img_pro.ndatabit == 1) {
                    _bits = 14;
                    Log("\tdatabit is 14bits.\n"); }
                else if (img_pro.ndatabit == 2) {
                    _bits = 12;
                    Log("\tdatabit is 12bits.\n");
                }
                else if (img_pro.ndatabit == 3) {
                    _bits = 8;
                    Log("\tdatabit is 8bits.\n");
                } 
                else Log("\tdatatype is unsigned char.\n");
                //
                if (img_pro.nendian == 0) Log("\tdata is little endian.\n");
                else Log("\tdata is bigger endian.\n");
            }
            else
            {
                ShowMessage("HBI_GetImageProperty failed!\n",true);
            }
        }



    }
}
