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
using BrightVisionSDKSample;

namespace Detector
{

    public delegate void ImageCallbackHandler(IntPtr pFrameInShort, ushort iFrameID, int iFrameWidth, int iFrameHeight, IntPtr pParam);
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


        public int MaxFrames = 0;

        /// <summary>
        /// 延时采集时间
        /// </summary>
        public int Delay = 0;

        /// <summary>
        /// 温度报警阈值
        /// </summary>
        public double TempratureThreshold;


        /// <summary>
        /// 倍率系数
        /// </summary>
        public int ScaleRatioFinetuning = 45;

        /// <summary>
        /// 高压控制器
        /// </summary>
        public static Nv_Protocol xRayControler = Nv_Protocol.Instance;



        /// <summary>
        /// 是否存储图像
        /// </summary>`
        public bool IsStored { get; set; }
        /// <summary>
        /// 是否多帧叠加
        /// </summary>

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
        /// 内部存储图像缓存
        /// </summary>
        private List<ushort[]> _imageBuffer = new List<ushort[]>();
        public List<ushort[]> ImageBuffer { get { return _imageBuffer; } }
 
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
            HandleImageCallBack = new ImageCallbackHandler(HandleImage);
            AcqMaxFrameEvent = new ExcutedCallbackHandler(AcqMaxFrame);
            TemperatureCallBack = new TemperatureCallbackHandler(TemperatureChanged);
            SystemStatusCallBack = new SystemStatusCallbackHandler(SystemStatusChanged);
            ConnBreakCallBack = new ExcutedCallbackHandler(ConnBreak);
            FinishedOffsetEvent = new ExcutedFinishCallbackHandler(FinishedOffset);
            FinishedDetectEvent = new ExcutedFinishCallbackHandler(FinishedDetect);
            FinishedGainEvent = new ExcutedFinishCallbackHandler(FinishedGain);
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

            if (BrightVisionSDK.StartStream(null, IntPtr.Zero))
            {
                Console.WriteLine("++++StartStream=OK");
            }
            else
            {
                Console.WriteLine("++++StartStream={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                return false;
            }

            //GetFrame
            ushort iFrameID = 0;
            int iWidth = 0;
            int iHeight = 0;
            int iPixelBits = 0;
            IntPtr pFrame = BrightVisionSDK.GetFrame(ref iFrameID, ref iWidth, ref iHeight, ref iPixelBits);
            if (pFrame != IntPtr.Zero)
            {
                FileStream sFileStream = new FileStream("frame.raw", FileMode.OpenOrCreate, FileAccess.Write);
                BinaryWriter sBinaryWriter = new BinaryWriter(sFileStream);

                int iPixelNum = iWidth * iHeight;
                for (int k = 0; k < iPixelNum; k++)
                {
                    ushort iPixelValue = BrightVisionSDK.GetRawPixelValue(pFrame, iPixelBits, k);
                    sBinaryWriter.Write(iPixelValue);
                }
                sFileStream.Close();
                Console.WriteLine("Save the frame id={0} to frame.raw", iFrameID);
            }
            else
            {
                Console.WriteLine("Fail to get one frame");
            }

            return true;


        }

        static  bool FrameProc(IntPtr pFrameInShort, ushort iFrameID, int iFrameWidth, int iFrameHeight, IntPtr pParam)
        {
            Console.WriteLine("Get a frame in short bytes ID={0} Width={1} Height={2} from the callback function", iFrameID, iFrameWidth, iFrameHeight);

            HandleImageCallBack( pFrameInShort,  iFrameID,  iFrameWidth,  iFrameHeight,  pParam);


            return false;
        }

        FrameProcCbFunc frameCb = new FrameProcCbFunc(FrameProc);

        public bool StartAcq()
        {

            _imageBuffer.Clear();
            count = 0;
            //Start Stream

            if (BrightVisionSDK.StartStream(frameCb, IntPtr.Zero))
            {
                Console.WriteLine("++++StartStream=OK");
                return true;
            }
            else
            {
                Console.WriteLine("++++StartStream={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                return false;
            }
        }

        /// <summary>
        /// 停止采集图像
        /// </summary>
        /// <returns></returns>
        public bool StopAcq()
        {
            ShowMessage("StopAcq ");
            BrightVisionSDK.StopStream();
            Console.WriteLine("++++StopStream=OK");

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
        public static ImageCallbackHandler HandleImageCallBack;
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



        /// <summary>
        /// 初始化探测器
        /// </summary>
        public bool InitDetector(out string res)
        {
            res = string.Empty;

            _detectorHeight = 1200;
            _detectorWidth = 1520;
            _imageHeight = _detectorHeight;
            _imageWidth = _detectorWidth;
            _bits = 16;

            BrightVisionSDK.Init();

            //GetVersion
            //Console.WriteLine("SDK Version: {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetVersionText()));
            Console.WriteLine("SDK Version: {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetVersionText()));

            //SearchforDevice
            uint iDeviceNumber = BrightVisionSDK.SearchforDevice();
            Console.WriteLine("Total number of devices: {0}", iDeviceNumber);

            //Attribute related APIs
            for (uint i = 0; i < iDeviceNumber; i++)
            {
                //Get device MAC
                IntPtr pDeviceMAC = BrightVisionSDK.GetDeviceMAC(i);
                Console.WriteLine("***************************************************************");
                Console.WriteLine("  Device Information:");

                //GetDeviceInfo
                BrightVisionSDK.SDeviceInfo sDeviceInfo = new BrightVisionSDK.SDeviceInfo();
                BrightVisionSDK.GetDeviceInfo(pDeviceMAC, ref sDeviceInfo);
                Console.WriteLine("    MAC={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pMAC));
                Console.WriteLine("    IP={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pIP));
                Console.WriteLine("    Control Port={0:D}", sDeviceInfo.iCtrlPort);
                Console.WriteLine("    Data Port={0:D}", sDeviceInfo.iDataPort);
                Console.WriteLine("    Mask={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pMask));
                Console.WriteLine("    Gateway={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pGateway));
                Console.WriteLine("    VenderName={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pVenderName));
                Console.WriteLine("    ModelName={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pModelName));
                Console.WriteLine("    Version={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pVersion));
                Console.WriteLine("    SerialNumber={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pSerialNumber));
                if (sDeviceInfo.bReachable)
                {
                    Console.WriteLine("    Reachable");
                }
                else
                {
                    Console.WriteLine("    Unreachable");
                }

                //LocalInfo
                BrightVisionSDK.SNICInfo sLocalInfo = new BrightVisionSDK.SNICInfo();
                BrightVisionSDK.GetLocalNICInfo(pDeviceMAC, ref sLocalInfo);
                Console.WriteLine("  Local Network Adapter Information:");
                Console.WriteLine("    MAC={0}", Marshal.PtrToStringAnsi(sLocalInfo.pMAC));
                Console.WriteLine("    IP={0}", Marshal.PtrToStringAnsi(sLocalInfo.pIP));
                Console.WriteLine("    Mask={0}", Marshal.PtrToStringAnsi(sLocalInfo.pMask));
                Console.WriteLine("    InterfaceName={0}", Marshal.PtrToStringAnsi(sLocalInfo.pInterfaceName));
                Console.WriteLine("    Broadcast={0}", Marshal.PtrToStringAnsi(sLocalInfo.pBroadcast));

                //ForceIP
                BrightVisionSDK.SForceInfo sForceInfo = new BrightVisionSDK.SForceInfo();
                sForceInfo.pMAC = sDeviceInfo.pMAC;
                sForceInfo.pIP = sDeviceInfo.pIP;
                sForceInfo.pMask = sLocalInfo.pMask;
                sForceInfo.pGateway = sDeviceInfo.pGateway;
                if (BrightVisionSDK.ForceIP(ref sForceInfo))
                {
                    Console.WriteLine("  ForceIP=OK");
                }
                else
                {
                    Console.WriteLine("  ForceIP={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                //SetCurrentDevice
                if (BrightVisionSDK.SetCurrentDevice(pDeviceMAC))
                {
                    Console.WriteLine("  SetCurrentDevice=OK");
                }
                else
                {
                    Console.WriteLine("  SetCurrentDevice=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //OpenCurrentDevice
                if (BrightVisionSDK.OpenCurrentDevice())
                {
                    Console.WriteLine("  OpenCurrentDevice=OK");
                }
                else
                {
                    Console.WriteLine("  OpenCurrentDevice=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }

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
        /// 实时处理探测器采集到的图像
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="image"></param>
        private unsafe void HandleImage(IntPtr pFrameInShort, ushort iFrameID, int iFrameWidth, int iFrameHeight, IntPtr pParam)
        {
            ShowMessage("HandleImage ");

            if ((pFrameInShort == null))
            {
                ShowMessage("图像数据异常 ",true);
                return ;
            }

            ushort[] buffer = new ushort[iFrameWidth * iFrameHeight];

            for (int i = 0; i < iFrameWidth * iFrameHeight; i++)
            {
                buffer[i] = ((ushort*)pFrameInShort)[i];
            }

            PlayBuffer.Enqueue(buffer);
            if (IsStored)
            {
                ImageBuffer.Add(buffer);
            }

            count++;
            Console.WriteLine("Recieve image count  {0} / {1}, ImageBuffer.Count = {2}",count, MaxFrames, ImageBuffer.Count);
            
            if (MaxFrames == count) {
                // 这里只是为了保存图片
                AcqMaxFrameEvent();
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
        /// 校正完毕，设置校正模式软件模式
        /// </summary>
        /// <param name="isSuccess"></param>
        private void FinishedOffset(bool isSuccess)
        {
            string msg = isSuccess ? "成功" : "失败";
            _offsetWaitHandle.Set();
        }
        private void FinishedGain(bool isSuccess)
        {
            string msg = isSuccess ? "成功" : "失败";

            _offsetWaitHandle.Set();
        }
        private void FinishedDetect(bool isSuccess)
        {
            string msg = isSuccess ? "成功" : "失败";
            _offsetWaitHandle.Set();
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

        #endregion

       public  void SetAcquisitionMode(long iAcquisitionMode ) {
            //Set AcquisitionMode
            // long iAcquisitionMode = 0;

            try
            {
                IntPtr pAcquisitionMode = Marshal.StringToHGlobalAnsi("AcquisitionMode");

                if (BrightVisionSDK.SetAttrInt(pAcquisitionMode, iAcquisitionMode, 0))
                {
                    Console.WriteLine("Set AcquisitionMode to {0:D}", iAcquisitionMode);
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to set AcquisitionMode, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }


                Marshal.FreeHGlobal(pAcquisitionMode);
            }
            catch (Exception ex){
                ShowMessage(ex.Message,true);

            }


        }

        public void SetExposureTime(double fExposureTime) {

            //Set ExposureTime
           // double fExposureTime = 0;
            IntPtr pExposureTime = Marshal.StringToHGlobalAnsi("ExposureTime");

                if (BrightVisionSDK.SetAttrFloat(pExposureTime, fExposureTime, 0))
                {
                    Console.WriteLine("Set ExposureTime to {0:f}", fExposureTime);
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to set ExposureTime, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }

            Marshal.FreeHGlobal(pExposureTime);

        }





    }
}
