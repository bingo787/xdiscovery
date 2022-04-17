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
        /// <summary>
        /// 是否存储图像
        /// </summary>
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
            StringBuilder _error = new StringBuilder(1024);
            NVDentalSDK.NV_LastErrorMsg(_error, 1024);
            return _error.ToString();
        }

        public bool GetTemperature(out float temperatrue1, out float temperature2)
        {
            float a, b;
            if (NVDentalSDK.NV_GetTemperature(out a, out b) == NV_StatusCodes.NV_SC_SUCCESS)
            {
                temperatrue1 = a;
                temperature2 = b;
                return true;
            }
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

            NVDentalSDK.NV_SetMaxFrames(MaxFrames);
            if (NVDentalSDK.NV_StartAcq(AcqMaxFrameEvent) != (int)NV_StatusCodes.NV_SC_SUCCESS)
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
            if (NVDentalSDK.NV_StopAcq() != NV_StatusCodes.NV_SC_SUCCESS)
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

        /// <summary>
        /// 初始化探测器
        /// </summary>
        public bool InitDetector(out string res)
        {
            res = string.Empty;
            NVDentalSDK.NV_CloseDet();

            if (NVDentalSDK.NV_SetAcqCallback(ShowImageCallBack, this) != (int)NV_StatusCodes.NV_SC_SUCCESS)
                res += ("SetAcqCallback Failed");
            else
                ShowMessage("SetAcqCallback success.");
            if (NVDentalSDK.NV_OpenDet() != NV_StatusCodes.NV_SC_SUCCESS)
            {
                StringBuilder buffer = new StringBuilder(1024);
                NVDentalSDK.NV_LastErrorMsg(buffer, 1024);
                res += "探测器连接失败。" + buffer.ToString();
                return false;
            }
            else
            {
                if (NVDentalSDK.NV_MonitorTemperature(TemperatureCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                    res += ("Temperature failed");
                if (NVDentalSDK.NV_MonitorSystemStatus(SystemStatusCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                    res += ("SystemStatus failed");
                if (NVDentalSDK.NV_MonitorConnbreak(ConnBreakCallBack) != NV_StatusCodes.NV_SC_SUCCESS)
                    res += ("ConnBreak failed");

                if (NVDentalSDK.NV_GetSensorSize(out _detectorWidth, out _detectorHeight, out _bits) != (int)NV_StatusCodes.NV_SC_SUCCESS)
                {
                    res += ("GetSize failed");
                }
                else
                {
                    _imageWidth = _detectorWidth;
                    _imageHeight = _detectorHeight;
                    NV_BinningMode binning;
                    NVDentalSDK.NV_GetBinningMode(out binning);
                    res += binning.ToString();
                    if (binning == NV_BinningMode.NV_BINNING_2X2)
                    {
                        NVDentalSDK.NV_SetImageRange(0, 0, ImageWidth / 2, ImageHeight / 2);
                    }
                    else
                        NVDentalSDK.NV_SetImageRange(0, 0, ImageWidth, ImageHeight);
                    NVDentalSDK.NV_SaveParamFile();
                    //NVDentalSDK.NV_LoadParamFile();
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

        }
        /// <summary>
        /// 设置探测器采集时间、增益
        /// </summary>
        /// <param name="nullable1"></param>
        /// <param name="nullable2"></param>
        public static string SetAcqPara(int? exp, string g, string binning = "1X1")
        {
            string res = string.Empty;
            int expTime = 168;
            NV_Gain gain = NV_Gain.NV_GAIN_07;
            NV_BinningMode bin = binning == "2X2" ? NV_BinningMode.NV_BINNING_2X2 : NV_BinningMode.NV_BINNING_1X1;
            if (NVDentalSDK.NV_SetBinningMode(bin) != NV_StatusCodes.NV_SC_SUCCESS)
            {
                StringBuilder buffer = new StringBuilder(1024);
                NVDentalSDK.NV_LastErrorMsg(buffer, 1024);
                res += ("Set BinningMode Failed:" + buffer.ToString() + "\n");
            }

            if (NVDentalSDK.NV_SetExpTime((int)exp) != NV_StatusCodes.NV_SC_SUCCESS)
            {
                res += ("Set Exptime Failed\n");
            }
            switch (g)
            {
                case "0.1pF":
                    gain = NV_Gain.NV_GAIN_01;
                    break;
                case "0.4pF":
                    gain = NV_Gain.NV_GAIN_04;
                    break;
                case "0.7pF":
                    gain = NV_Gain.NV_GAIN_07;
                    break;
                case "1.0pF":
                    gain = NV_Gain.NV_GAIN_10;
                    break;
                default:
                    break;
            }
            if (NVDentalSDK.NV_SetGain(gain) != NV_StatusCodes.NV_SC_SUCCESS)
            {
                res += ("Set Gain Failed\n");
            }
            int x, y, w, h;
            NVDentalSDK.NV_GetImageRange(out x, out y, out w, out h);

            NVDentalSDK.NV_GetGain(out gain);
            NVDentalSDK.NV_GetExpTime(out expTime);
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
            NVDentalSDK.NV_ResetCorrection();
            NV_StatusCodes result = NVDentalSDK.NV_RunOffsetCalThread(FinishedOffsetEvent);
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
            NVDentalSDK.NV_ResetCorrection();
            NV_StatusCodes result = NVDentalSDK.NV_RunAutoDefectCalThread(FinishedDetectEvent);
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
            NVDentalSDK.NV_ResetCorrection();
            NVDentalSDK.NV_RunGainCalThread(FinishedGainEvent, OpenXRayEvent, CloseXRayEvent);
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
                    NVDentalSDK.NV_CancelGainCal();
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
            NVDentalSDK.NV_SetDefectCal(NV_CorrType.NV_CORR_NO);
            NVDentalSDK.NV_SetGainCal(NV_CorrType.NV_CORR_NO);
            NVDentalSDK.NV_SetOffsetCal(NV_CorrType.NV_CORR_NO);

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
            return NVDentalSDK.NV_SetGain(nV_Gain) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetExpTime(int p)
        {
            return NVDentalSDK.NV_SetExpTime(p) == NV_StatusCodes.NV_SC_SUCCESS;
        }

        public bool NV_SetMaxFrames(int p)
        {
            return NVDentalSDK.NV_SetMaxFrames(p) == NV_StatusCodes.NV_SC_SUCCESS;
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
