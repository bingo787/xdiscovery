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

using LionSDK;

namespace Detector
{

    public unsafe struct ImageData
    {
        public uint uwidth;       // image width
        public uint uheight;      // image height
        public uint ndatabits; // 
        public uint uframeid;     // frame id
        public void* databuff;           // buffer address
        public uint datalen;      // buffer size
    }

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


        /// <summary>
        /// 高压控制器
        /// </summary>
        public static SerialPortControler_RS232PROTOCOL xRayControler = SerialPortControler_RS232PROTOCOL.Instance;



        /// <summary>
        /// 是否存储图像
        /// </summary>`
        public bool IsStored { get; set; }

        public int ImageMode { get; set; }

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

        public double ScaleRatio = 0.1;
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
            AcqMaxFrameEvent = new ExcutedCallbackHandler(AcqMaxFrame);
            TemperatureCallBack = new TemperatureCallbackHandler(TemperatureChanged);
            SystemStatusCallBack = new SystemStatusCallbackHandler(SystemStatusChanged);
            ConnBreakCallBack = new ExcutedCallbackHandler(ConnBreak);
            OpenXRayEvent = new ExcutedCallbackHandlerWithValue(OpenXRay);
            CloseXRayEvent = new ExcutedCallbackHandler(CloseXRay);

            IsStored = true;


        }

        /// <summary>
        /// 程序当前线程Dispatcher
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher { get { return Application.Current.Dispatcher; } }


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

        private int AsyncImageCallback(LU_DEVICE device, byte[] pImgData, int nDataBuf, string pFile)
        {

            unsafe
            {

                // 读取文件
                try
                {
                    //Console.WriteLine("{0},{1},{2},{3}",device.uvcIdentity.Id, pImgData.Length, nDataBuf, pFile);

                    if (string.IsNullOrEmpty(pFile)) {
                      //  string message = GetDeviceState();
                        ShowMessage( "文件路径为空" , true);
                    }

                    Console.WriteLine("图片已采集存储完成 " + pFile);
                    BinaryReader br = new BinaryReader(new FileStream("luvc_camera.raw", FileMode.Open));

                    byte[] byteBuffer =  br.ReadBytes((int)(_imageHeight * _imageWidth * 2) );
                    ushort[] uint16Buffer = new ushort[_imageHeight * _imageWidth];

                    for (int i = 0; i < _imageHeight * _imageWidth; i+=2) {
                        uint16Buffer[i] = System.BitConverter.ToUInt16(byteBuffer, i);
                    }

                    PlayBuffer.Enqueue(uint16Buffer);

                    if (IsStored)
                    {
                        ImageBuffer.Add(uint16Buffer);
                    }

                    count++;


                    if (ImageMode == 0) // AC 模式下收到图后就停止
                    {
                        // 单帧的时候，停止
                        AcqMaxFrameEvent();
                    }

                }
                catch (IOException e)
                {
                  
                    string message  = GetDeviceState();
                    Console.WriteLine(message);
                    Console.WriteLine(e.Message + "\n Cannot open file.");
                    ShowMessage(message, true);
                    return 1;

                }

              
            }
            return 0;
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        LionCom.LionImageCallback callback;
        public bool StartSingleShot() {
            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;

            unsafe
            {
               
                if (LionCom.LU_SUCCESS == LionSDK.LionSDK.GetImage(ref luDev, 0, callback))
                {
                    return true;
                }
                else {
                    return false;
                }

            }
        }

        public bool StartAcq()
        {

            _imageBuffer.Clear();
            _multiFramesOverlayBuffer.Clear();
            count = 0;
            return true;

        }

        /// <summary>
        /// 停止采集图像
        /// </summary>
        /// <returns></returns>
        public bool StopAcq()
        {
            ShowMessage("StopAcq ");

          
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

        LU_DEVICE luDev;

        public void SetUVCDeviceParameters(int nImgModel, int nBinning, int nFilter, int nRay) {

            //检测图像时间
            UInt32 nCheckTime = 60 * 1000; //ms
            //获取图像时间
            UInt32 nGetTime = 60 * 1000; //ms

            LU_PARAM param = new LU_PARAM();
            unsafe
            {
                //出图模式
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_MODE;
                param.size = sizeof(UInt32);
                param.data = &nImgModel;
                //
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }
            //
            unsafe
            {
                //Binning模式
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_BINNING;
                param.size = sizeof(UInt32);
                param.data = &nBinning;
                //
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //图像处理标志
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_FILTER;
                param.size = sizeof(UInt32);
                param.data = &nFilter;
                //
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //X-RAY类型
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_XRAY;
                param.size = sizeof(UInt32);
                param.data = &nRay;
                //
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //检测图像时间
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_TRIGGERTIME;
                param.size = sizeof(UInt32);
                param.data = &nCheckTime;
                //
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //获取图像时间
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_READIMAGETIME;
                param.size = sizeof(UInt32);
                param.data = &nGetTime;
                LionSDK.LionSDK.SetDeviceParam(ref luDev, ref param);
            }


        }

        public void GetUVCDeviceParameters() {

            //出图模式
            int nImgModel = (int)LU_MODE.LUMODE_AC;
            //Binning模式
            int nBinning = (int)LUDEV_BINNING.LUDEVBINNING_NO;
            //图像处理标志
            int nFilter = (int)LUDEV_FILTER.LUDEVFILTER_NO;
            //X-RAY类型
            int nRay = 0;

            LU_PARAM param = new LU_PARAM();

            //获取设置
            unsafe
            {
                //出图模式
                nImgModel = 0;
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_MODE;
                param.size = sizeof(UInt32);
                param.data = &nImgModel;
                //
                LionSDK.LionSDK.GetDeviceParam(ref luDev, ref param);
            }
            //
            unsafe
            {
                //Binning模式
                nBinning = 0;
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_BINNING;
                param.size = sizeof(UInt32);
                param.data = &nBinning;
                //
                LionSDK.LionSDK.GetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //图像处理标志
                nFilter = 0;
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_FILTER;
                param.size = sizeof(UInt32);
                param.data = &nFilter;
                //
                LionSDK.LionSDK.GetDeviceParam(ref luDev, ref param);
            }
            unsafe
            {
                //X-RAY类型
                nRay = 0;
                param.param = (UInt16)LUDEV_PARAM.LUDEVPARAM_XRAY;
                param.size = sizeof(UInt32);
                param.data = &nRay;
                //
                LionSDK.LionSDK.GetDeviceParam(ref luDev, ref param);
            }

            Console.WriteLine("mode:{0},bining:{1},filter:{2},Xray:{3}", nImgModel, nBinning, nFilter, nRay);

        }

        public string GetDeviceState() {

            unsafe
            {
                //获取设备状态
                string Text = "";
                LUDEV_STATE state = LUDEV_STATE.LUDEVSTATE_UNOPNE;
                if (LionCom.LU_SUCCESS == LionSDK.LionSDK.GetDeviceState(ref luDev, ref state))
                {
                    //更亲状态信息
                    switch (state)
                    {
                        case LUDEV_STATE.LUDEVSTATE_UNOPNE:             //设备未打开
                            Text = "设备状态: 设备未打开!";
                            break;
                        case LUDEV_STATE.LUDEVSTATE_OPEN:                   //设备打开
                            Text = "设备状态: 设备打开!";
                            break;
                        case LUDEV_STATE.LUDEVSTATE_WAITTRIGGER:                    //等待触发
                            Text = "设备状态: 等待触发!";
                            break;
                        case LUDEV_STATE.LUDEVSTATE_TRIGGERIMAGE:                   //触发获取图像数据
                            Text = "设备状态: 触发获取图像数据!";
                            break;
                        case LUDEV_STATE.LUDEVSTATE_IMAGESAVE:                  //图像保存
                            Text = "设备状态: 图像保存!";
                            break;
                        case LUDEV_STATE.LUDEVSTATE_OVERTIME:                   //超时
                            Text = "设备状态: 获取图像超时";
                            break;
                    }

                }
                return Text;
            }
        }
        
        /// <summary>
        /// 初始化探测器
        /// </summary>
        public bool InitDetector(out string res)
        {
            res = string.Empty;

            _detectorHeight = 2272;
            _detectorWidth = 1660;
            _imageHeight = _detectorHeight;
            _imageWidth = _detectorWidth;
            _bits = 16;
 
            if (LionSDK.LionSDK.GetDeviceCount() < 1)
            {
                res += "没有设备存在！";
                return false;
            }
            else
            {
                // 只使用一个Device 0 
                luDev = new LU_DEVICE();
                if (LionCom.LU_SUCCESS != LionSDK.LionSDK.GetDevice(0, ref luDev)) {
                    res += "获取设备失败！";
                    return false;
                }

                callback = new LionCom.LionImageCallback(AsyncImageCallback);
                res += "设备打开成功！";
                return true;
            }

          
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
        /// 开光
        /// </summary>
        public static ExcutedCallbackHandlerWithValue OpenXRayEvent;
        /// <summary>
        /// 闭光
        /// </summary>
        public static ExcutedCallbackHandler CloseXRayEvent;

      
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
            _offsetWaitHandle.WaitOne(600000, true);

            Thread.Sleep(5000);
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
                _gainWaitHandle.WaitOne(60000, true);
                Thread.Sleep(1000);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    CMessageBox.Show("请打开 X光");
                }));
            }
            return GetImage(count);
        }


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

      
 


        public bool SetMaxFrames(int p)
        {
            MaxFrames = p;
            return true;
        }

 

        public bool SetTriggerMode(int t) {

            return true;
        }

       
        // 获取图像数据信息
        public void btnGetImageProperty()
        {
            Log("get Image property begin!\n");
          
          
        }



    }
}
