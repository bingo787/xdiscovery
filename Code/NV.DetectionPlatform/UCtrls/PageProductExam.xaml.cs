using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using NV.DRF.Controls;
using NV.DRF.Core.Global;
using NV.DRF.Core.Common;
using NV.Infrastructure.UICommon;
using Detector;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NV.DRF.Core.Ctrl;
using System.Windows.Forms.Integration;
using System.Collections.ObjectModel;
using NV.DetectionPlatform.Service;
using System.Diagnostics;
using NV.DetectionPlatform.Entity;
using System.Drawing;
using NV.Config;
using OpenCvSharp;
using SerialPortController;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using System.Data.SqlTypes;
using BrightVisionSDKSample;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// ImageViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class PageProductExam : Page
    {
        private int _imageCount = 0;
        /// <summary>
        /// 实时采集显示线程
        /// </summary>
        private Thread _playAcqImage;
        /// <summary>
        /// 探测器采集工具类
        /// </summary>
        private DetectorController _detector = DetectorController.Instance;
        private WindowsFormsHost wfh = new WindowsFormsHost();
        private KrayDicomLib.DicomFile _file = new KrayDicomLib.DicomFile();
        private int _curExpTime = 1 ;
        private ExamType _curExpType;
        /// <summary>
        /// 高压控制器
        /// </summary>
        
        public static CustomProtocl PostionReporter = CustomProtocl.Instance;
        double scaleRatio = 0.1;
        double TempratureThreshold = 35.0f;
        private double CalculateScaleRatio() {
            //todo:zhaoqibin
            scaleRatio =  (PostionReporter.AxisZDistance_mm * 2.126526929 + _detector.ScaleRatioFinetuning) /10000.0;

            return scaleRatio;
        }
        private WndUSMSetting usmSetting = new WndUSMSetting();

        public PageProductExam()
        {
            InitializeComponent();

            this.Log("初始化采集--页面.");
            InitilizeUI();
            this.Log("初始化采集--探测器群.");
            InitilizeDetector();
            this.Loaded += PageProductExam_Loaded;
        }

        void PageProductExam_Loaded(object sender, RoutedEventArgs e)
        {
            WndAOISetting.InitAOI();
            usmSetting.InitUSM();
          
            InitilizePlayAcqThread();
        }

        private void InitilizeUI()
        {
            PlatformModels = new ObservableCollection<PlatformFilesModel>();
            _lstSeries.ItemsSource = PlatformModels;

         
           usmSetting.UsmParamChangedEvent += usmSetting_UsmParamChanged;
           usmSetting.BackSettingEvent += usmSetting_Back;
           usmSetting.CloseSettingEvent += usmSetting_Close;


            this.Log("初始化快速窗位窗宽设定");
            var wlSetting = new WndWLSetting();
            bdWL.Child = wlSetting;
           

            wlSetting.WindowWLChangedEvent += wlSetting_WindowWLChangedEvent;
            wlSetting.CloseSettingEvent += wlSetting_CloseSettingEvent;
            var wlsetting = NV.Config.SerializeHelper.LoadFromFile<ImageParam>(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config\\wlsetting.xaml"));
            if (wlsetting.WindowWidth != null)
                _quickWW = (int)wlsetting.WindowWidth;
            if (wlsetting.WindowLevel != null)
                _quickWL = (int)wlsetting.WindowLevel;
            //初始化伪彩文件配置菜单
            this.Log("初始化伪彩文件配置菜单");
            string lutsDir = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Luts");
            string[] luts = Directory.GetFiles(lutsDir, "*.lut");
            for (int i = 0; i < luts.Length; i++)
            {
                MenuItem mi = new MenuItem() { Header = Path.GetFileNameWithoutExtension(luts[i]), Tag = "FalseColor", Uid = luts[i] };
                mi.Click += mi_Click;
                cmenuFalsecolor.Items.Add(mi);
            }

        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            string uid = (sender as FrameworkElement).Uid as string;
            NV.DRF.Core.Model.GeneralSettingHelper.Instance.LutFile = uid;
            NV.DRF.Core.Model.GeneralSettingHelper.Save();
            radioBtnDo(sender, e);
        }

        /// <summary>
        /// 调整窗宽窗位
        /// </summary>
        /// <param name="ww"></param>
        /// <param name="wl"></param>
        void wlSetting_WindowWLChangedEvent(int ww, int wl)
        {
            if (ipUC != null && ipUC.CurrentDv != null)
            {
                ipUC.CurrentDv.SetWindowLevel(ww, wl);
                ipUC.CurrentDv.Invalidate();
            }
        }

        void usmSetting_UsmParamChanged(int amount, int radius, int threshold)
        {
             
            if (ipUC != null && ipUC.CurrentDv != null)
            {
                ipUC.CurrentDv.SaveToStack();
                ushort[] result = usmSetting.UnsharpenMask(ipUC.CurrentDv,amount,radius,threshold);
                ipUC.CurrentDv.GetImageSize(out ushort width, out ushort height, out ushort bits, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
                ipUC.CurrentDv.PutImageData(width, height, bits, ref result[0]);
                ipUC.CurrentDv.RefreshImage();
            }
        }
        void usmSetting_Close() {
        }

        void usmSetting_Back()
        {
            ipUC.CurrentDv.BackFromStack();
            ApplyConfigWL(true);     
        }

        /// <summary>
        /// 实时显示图像线程
        /// </summary>
        private void InitilizePlayAcqThread()
        {
            _playAcqImage = new Thread(new ThreadStart(delegate { PlayBackground(); }));
            _playAcqImage.Start();
        }

       

        /// <summary>
        /// 初始化探测器
        /// </summary>
        public void InitilizeDetector()
        {

            string res;
            if (_detector.InitDetector(out res))
            {

                 var Data = NV.Config.NV1313FPDSetting.Instance;
                _detector.TempratureThreshold = Data.TemperatureThreshold;

                res += "探测器已连接。";
                IsConnected = true;

            }
            else
            {
                this.Log(res);
                IsConnected = false;
                CMessageBox.Show(res);
                this.Log("探测器初始化失败");
            }


        }
        /// <summary>
        /// 后台实时采集显示
        /// </summary>
        private void PlayBackground()
        {
            int clear = 0;
            while (_running)
            {
                clear++;
                if (_detector.PlayBuffer.Count > 0)
                {
                    ushort[] data = _detector.PlayBuffer.Dequeue();
                    ushort W = (ushort)_detector.ImageWidth;
                    ushort H = (ushort)_detector.ImageHeight;
                    ushort Bits = (ushort)_detector.Bits;
                    _imageCount++;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                       // System.Console.WriteLine(String.Format("PlayBackground IsAcqing {0}", IsAcqing));
                       // if (IsAcqing)
                        {
                            //ipUC.PutData(W,H ,Bits,data, true);
                            ipUC.CurrentDv.PutImageData(W, H, Bits, ref data[0]);
                            ipUC.CurrentDv.GetWindowLevel(ref _quickWW, ref _quickWL);
                            ipUC.CurrentDv.SetWindowLevel(_quickWW, _quickWL);
                            ipUC.CurrentDv.RefreshImage();


                            if (_curExpType == ExamType.Spot || _curExpType == ExamType.MultiEnergyAvg)
                            {
                                ApplyConfigWL(true);
                            } 
                          

                        }
                            
                        if (_detector.PlayBuffer.Count > 90)
                        {
                            _detector.PlayBuffer.Dequeue();
                            _detector.PlayBuffer.Dequeue();
                            _imageCount += 2;
                        }
                        sldrImageIndex.Maximum = _imageCount;
                    }), System.Windows.Threading.DispatcherPriority.Normal);

                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(35);
                    if (clear % 150 == 0)
                        GC.Collect();
                }
            }
        }
        /// <summary>
        /// 关闭后台显示图像线程
        /// </summary>
        public void CloseExamThread()
        {
            _running = false;
        }
        /// <summary>
        /// 分格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrintLayout_Checked(object sender, RoutedEventArgs e)
        {
            LayoutType type = (LayoutType)Enum.Parse(typeof(LayoutType), (sender as FrameworkElement).Tag.ToString());
            ipUC.SetLayout(type);
        }

        #region 采集图像
        private const int DETECTOR_READTIME = 132;
        /// <summary>
        ///采集
        /// </summary>
        /// <param name="type"></param>
        public void StartAcq(ExamType type = ExamType.Spot, bool isStored = false, int maxCount = 1, double stepKv = 0, int stepUA = 0)
        {
            System.Windows.Window wnd = System.Windows.Window.GetWindow(this);
            if (wnd != null && (wnd as MainWindow) != null && (wnd as MainWindow)._hVView != null)
            {
                (wnd as MainWindow)._hVView.Visibility = Visibility.Hidden; ;
            }

            if (Global.CurrentProduct == null)
            {
                CMessageBox.Show("当前无产品登记，无法采集。\n Please register the products first.");
                return;
            }
            if (Global.CurrentParam == null)
            {
                CMessageBox.Show("当前无拍摄方案，无法采集。\nPlease select the Solution first");
                return;
            }

            if (IsAcqing)
            {
                Console.WriteLine("正在采集中...... IsAcqing = ture");
                return;
            }







            if (type == ExamType.Spot )
            {
                _detector.IsStored = true;
                _detector.MaxFrames = 1;
              //  _detector.SetAcquisitionMode(2);

            }
            else if (type == ExamType.Expose)
            {
                _detector.IsStored = isStored;
                _detector.MaxFrames = 0; // 连续获取
             //   _detector.SetAcquisitionMode(0);

            }
            else if (type == ExamType.MultiEnergyAvg)
            {
                _detector.IsStored = true;
                _detector.MaxFrames = maxCount;
              //  _detector.SetAcquisitionMode(2);
            }

            _imageCount = 0;
            IsAcqing = true;
            _curExpType = type;
            _curExpTime = (int)(Global.CurrentParam.Time * 1000);

            _detector.SetExposureTime((double)_curExpTime);

            DicomViewer.Current.ClearImage();


            MainWindow.ControlSystem.XRayOn();



            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(_detector.Delay);
                bool ret = false;

                if (_curExpType == ExamType.Spot )
                {

                    ret = _detector.StartSingleShot();
                }
                else if(_curExpType == ExamType.Expose)
                {
                    ret = _detector.StartAcq();
                }
                else if(_curExpType == ExamType.MultiEnergyAvg)
                {
                    for (int i = 0; i < maxCount; i++)
                    {

                            
                            // 采集照片
                            ret = _detector.StartSingleShot();


                            // 重新设置曝光参数
                            Thread.Sleep(_curExpTime);
                            double kv = (double)Global.CurrentParam.KV - i * stepKv;
                            double power = (int)Global.CurrentParam.Power - i * stepUA;
                            MainWindow.ControlSystem.SetKV(kv);
                            Thread.Sleep(150);
                            MainWindow.ControlSystem.SetPower(power);
                            Thread.Sleep(150);
                            System.Console.WriteLine("MultiEnergyAvg i {0}, kv {1}, power {2}", i, kv, power);

                            // 判断图像是否采集完成，没有的话要在此等待
                            while (_detector.ImageBuffer.Count != (i + 1)) {
                            Console.WriteLine("_detector.ImageBuffer.Count = {0}", _detector.ImageBuffer.Count);
                            Thread.Sleep(2 * _curExpTime);
                            }

                    }

                }

                if (ret)
                {
                    if (Global.MainWindow != null)
                    {
                        Global.MainWindow.NotifyTip("Detector", "正在采集");
                    }
                }
                else
                {
                    IsAcqing = false;
                    MainWindow.ControlSystem.XRayOff();
                }
            })).Start();


        }
        /// <summary>
        /// 停止采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopAcq(object sender, RoutedEventArgs e)
        {
           
            MainWindow.ControlSystem.XRayOff();
            if (IsAcqing) {
                IsAcqing = false;

                _detector.StopAcq();  
                if (_curExpType == ExamType.MultiEnergyAvg)//多能合成
                {
                    SaveMultiAvgFiles(_detector.ImageBuffer.ToList());
                }
                else if (_curExpType == ExamType.Spot)
                {
                    SaveFiles(_detector.ImageBuffer.ToList());
                }
                else if (_curExpType == ExamType.Expose)
                {
                    SaveFiles(_detector.ImageBuffer.ToList());
                }

                if (_curExpType == ExamType.MultiEnergyAvg)
                    RecoverHVPara();

                _detector.ImageBuffer.Clear();
            }
           

        }
        /// <summary>
        /// 恢复设定高压参数
        /// </summary>
        private void RecoverHVPara()
        {
            MainWindow.ControlSystem.SetKV((double)Global.CurrentParam.KV);
            System.Threading.Thread.Sleep(150);
            MainWindow.ControlSystem.SetCurrent((int)Global.CurrentParam.UA);
        }
        /// <summary>
        /// 平均图像
        /// </summary>
        /// <param name="list"></param>
        private void SaveMultiAvgFiles(List<ushort[]> list)
        {

            //// HDR 合成
            // string[] imagesFiles = { "memorial0061.png", "memorial0062.png", "memorial0063.png", "memorial0064.png", "memorial0065.png", "memorial0066.png", "memorial0067.png", "memorial0068.png", "memorial0069.png", "memorial0070.png", "memorial0071.png", "memorial0072.png", "memorial0073.png", "memorial0074.png", "memorial0075.png", "memorial0076.png" };
            // float[] exposures = { 0.03125f, 0.0625f, 0.125f, 0.25f, 0.5f, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };
            int num = list.Count;

            //读入图片
            int h = (int)_detector.ImageHeight;
            int w = (int)_detector.ImageWidth;
            Mat[] images = new Mat[num];
            float[] exposures = { 0.03125f, 0.0625f, 0.125f, 0.25f, 0.5f };
            
            for (int i = 0; i < num; i++)
            {
                // string file = @"I:\csharp\images\hdr\" + imagesFiles[i];
                // images[i] = new Mat(file, ImreadModes.Color);

                images[i] = new Mat(h, w, MatType.CV_16UC1, list[i]);
            }

            //估计相机响应
            Mat response = new Mat();
            CalibrateDebevec calibrate = CalibrateDebevec.Create();
            calibrate.Process(images, response, exposures);

            //生成HDR图片
            Mat hdr = new Mat();
            MergeDebevec merge_debevec = MergeDebevec.Create();
            merge_debevec.Process(images, hdr, exposures, response);

            //Tonemap
            Mat ldr = new Mat();
            Tonemap tonemap = Tonemap.Create(2.2f);
            tonemap.Process(hdr, ldr);

            Mat fusion = new Mat();
            MergeMertens merge_mertens = MergeMertens.Create();
            merge_mertens.Process(images, fusion);

            Cv2.ImWrite("fusion.png", fusion * 255);
            Cv2.ImWrite("ldr.png", ldr * 255);
            Cv2.ImWrite("hdr.hdr", hdr);


            ushort[] result = new ushort[w * h];
            System.Threading.Tasks.Parallel.For(0, h, i =>
            {
                for (int j = 0; j < w; j++)
                {
                    result[i * w + j] = hdr.At<ushort>(i, j);
                }
            });



            SaveFiles(new List<ushort[]> { result });

        }
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <param name="p"></param>
        private void SaveFiles(List<ushort[]> p)
        {
            int w = 65535, c = 30000;
            if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
                ipUC.CurrentDv.GetWindowLevel(ref w, ref c);

            DicomViewer viewer = new DicomViewer(wfh);
            string tempFile = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "template.dcm");
            //viewer.LoadFromFile(tempFile);
            _file.OpenFile(tempFile);

            ushort width, height, bits;
            width = (ushort)_detector.ImageWidth;
            height = (ushort)_detector.ImageHeight;
            bits = (ushort)_detector.Bits;

            _file.SopClassUID = KrayDicomLib.tagSOPCLASS_UID.UID_XRAYANGIOGRAPHICIMAGESTORAGE;
            _file.BitsStored = bits;

            _file.PutDicomString(0x18, 0x60, Global.CurrentParam.KV.ToString());//KVP
            _file.PutDicomString(0x18, 0x1151, Global.CurrentParam.Power.ToString());//功率
            _file.PutDicomString(0x18, 0x1150, _curExpTime.ToString());//ExposureTime
            _file.PutDicomString(0x0028, 0x0030, scaleRatio.ToString("f2"));//像素与mm倍率
            _file.PutDicomString(0x0008, 0x1010, NV.DRF.Core.Model.GeneralSettingHelper.Instance.HVName);//高压名称-StationName

            _file.PatientID = Global.CurrentProduct.GUID;
            _file.PatientName = Global.CurrentProduct.ProductName;
            _file.PatientSex = Global.CurrentProduct.ProductTypeID;
            _file.PatientAge = Global.CurrentProduct.ProductSpecification;
            _file.BodyPartExamined = Global.CurrentProduct.ProductKeywords;
            _file.SeriesDate = DateTime.Now.ToString("yyyy-MM-dd");
            _file.SeriesTime = DateTime.Now.ToString("HH:mm:ss");
            _file.Manufacturer = NV.DRF.Core.Model.GeneralSettingHelper.Instance.CompanyName;

            try
            {
                ProgressDialog dia = new ProgressDialog("保存图像");
                dia.CanCancel = true;
                int sum = 0;
                int success = 0;
                PlatformFilesModel model = new PlatformFilesModel() { DcmFiles = new ObservableCollection<string>() };//图像历史列表对象
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += delegate
                {
                    try
                    {
                        Global.CurrentProduct.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Global.SaveProduct(Global.CurrentProduct);

                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { dia.ShowDialogEx(); }));
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            dia.Summary = "正在保存图像文件，请稍候...";
                            dia.MaxValue = p.Count;
                            sum = p.Count;
                            sldrImageIndex.Maximum = sum;
                        }));

                        string folder = System.IO.Path.Combine(Global.CurrentProduct.ImageFolder, DateTime.Now.ToString("yyyy-MM-dd_HH∶mm∶ss"));
                        if (!System.IO.Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        model.DateTime = folder;//时间

                        for (int i = 0; i < p.Count; i++)
                        {
                            string fName = System.IO.Path.Combine(folder, i.ToString("D3") + ".dcm");
                            string pngName = System.IO.Path.ChangeExtension(fName, ".png");
                            if (i == 0)
                                model.Thumbnail = pngName;//缩略图
                            model.DcmFiles.Add(fName);//dcm列表

                            ushort[] data = p[i];
                            bool cancel = false;
                            Thread.Sleep(10);
                            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                viewer.PutImageData(width, height, bits, ref data[0]);
                                viewer.SetWindowLevel(w, c);

                                //viewer.SaveToFile(fName, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL, false);
                                viewer.SaveToDicomFilePtr(_file, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL, false);
                                _file.SaveTo(fName);

                                viewer.SaveImage(width, height, pngName, ImageViewLib.tagIMAGE_TYPE.IT_PNG, false);
                                dia.CurValue++;
                                success++;

                                cancel = dia.Canceled;
                            }));

                            //取消保存
                            if (cancel)
                                break;
                        }
                        this.Log("保存图像完毕，产品名称:" + Global.CurrentProduct.ProductName);
                    }
                    catch (Exception ex)
                    {
                        CMessageBox.Show(ex.Message);
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    if (dia.Canceled)
                        dia.Summary = string.Format("已取消。总数：{0},成功：{1}，失败：{2}", sum, success, sum - success);
                    else
                        dia.Summary = string.Format("保存成功。总数：{0},成功：{1}，失败：{2}", sum, success, sum - success);
                    dia.CanClosed = true;
                    if (sum == success)
                    {
                        dia.Close();
                    }

                    //add new dir to imglist
                    PlatformModels.Add(model);

                    if (PlatformModels.Count > 0)
                        Global.MainWindow.ActiveAllFunction();

                    //update detector state
                    if (Global.MainWindow != null)
                        Global.MainWindow.NotifyTip("Detector", "就绪");

                    if (p.Count > 0)
                    {
                        _lstSeries.SelectedIndex = _lstSeries.Items.Count - 1;
                        _lstSeries.ScrollIntoView(_lstSeries.SelectedItem);
                    }
                };
                bw.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                this.Error(ex, "图像处理-保存失败。");
                MessageBox.Show(ex.Message);
                if (Global.MainWindow != null)
                    Global.MainWindow.NotifyTip("Detector", "就绪");
            }
        }

        private void Tips(string p)
        {
        }


        #endregion


        private List<string> CurrentFiles = new List<string>();
        /// <summary>
        /// 缩略图选择改变事件，打开当前所选图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _lstSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
                {
                    ipUC.CurrentDv.ProcType = ImageViewLib.tagPROC_TYPE.PT_MOVE;
                    ipUC.CurrentDv.DeleteAnnotation(true);
                    ipUC.CurrentDv.Invalidate();
                }

                CurrentFiles.Clear();
                sldrImageIndex.Maximum = 0;

                if (_lstSeries.SelectedItem == null)
                    return;
                PlatformFilesModel model = _lstSeries.SelectedItem as PlatformFilesModel;
                if (model == null)
                    return;
                CurrentFiles = model.DcmFiles.ToList();
                sldrImageIndex.Maximum = CurrentFiles.Count;
                sldrImageIndex_ValueChanged(null, null);

            }
            catch (Exception ex)
            {
                Global.MainWindow.UnFreeze();
                this.Error(ex, "图像处理-打开所选图片失败。");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 打印报告
        /// </summary>
        public void Report()
        {
            try
            {
                if (Global.CurrentProduct == null)
                    return;
                string[] files = CurrentFiles.ToArray();
                if (files.Length == 0)
                    return;
                ImageInfo info = new ImageInfo();
                info.LoadDicomFile(files[0]);
                int w = info.Width;
                int h = info.Height;

                string fnReportExe = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Report", "Report.exe");
                string fnImageXml = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Report", "study_info.xml");
                string fnInfoXml = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Report", "report.xml");
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = false,
                };

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fnImageXml, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Document");
                    writer.WriteStartElement("Images");

                    foreach (string file in files)
                    {
                        writer.WriteStartElement("Image");

                        writer.WriteElementString("Thumbnail", System.IO.Path.ChangeExtension(file, "png"));
                        writer.WriteElementString("FileName", file);
                        writer.WriteStartElement("Crop");
                        writer.WriteElementString("Left", "0");
                        writer.WriteElementString("Top", "0");
                        writer.WriteElementString("Right", w.ToString());
                        writer.WriteElementString("Bottom", h.ToString());
                        writer.WriteEndElement(); //Crop

                        writer.WriteEndElement(); //Image
                    }

                    writer.WriteEndElement();

                    writer.WriteElementString("SaveFileName", "");
                    string savePath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Report") + "\\";
                    writer.WriteElementString("SavePath", savePath);

                    writer.WriteEndElement();   //Document
                    writer.WriteEndDocument();
                }

                string name = "Nanovision";
                if (File.Exists(fnInfoXml))
                {
                    try
                    {
                        using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(fnInfoXml))
                        {
                            reader.ReadStartElement("Report");
                            reader.ReadStartElement("HospitalName");
                            name = reader.ReadString();
                        }
                    }
                    catch { }

                }

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fnInfoXml, settings))
                {
                    writer.WriteStartElement("Report");
                    writer.WriteElementString("PatientID", Global.CurrentProduct.GUID);
                    writer.WriteElementString("PatientName", Global.CurrentProduct.ProductName);
                    writer.WriteElementString("PatientSex", Global.CurrentProduct.ProductTypeID);
                    writer.WriteElementString("PatientAge", Global.CurrentProduct.ProductSpecification);
                    writer.WriteElementString("Department", "");
                    writer.WriteElementString("HospitalName", name);
                    writer.WriteElementString("ReportDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    writer.WriteElementString("StudyDateTime", Global.CurrentProduct.EndTime);
                    writer.WriteElementString("Department", "");
                    writer.WriteElementString("CheckView", "");
                    writer.WriteElementString("CheckResult", "");
                    writer.WriteElementString("HospitalNo", "");
                    writer.WriteElementString("BedNo", "");
                    writer.WriteEndElement();
                }

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = fnReportExe;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.Start();

            }
            catch (Exception ex)
            {
                this.Error(ex, "图像处理-打开报告界面失败。");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 图像显示处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImgProc_Click(object sender, RoutedEventArgs e)
        {

            string strTag = "none none";
            if (e is ExecutedRoutedEventArgs)
                strTag = (e as ExecutedRoutedEventArgs).Parameter.ToString();
            else
                strTag = (sender as FrameworkElement).Tag as string;
            string[] strArr = strTag.Split(' ');

            if (strArr.Length < 2) return;

            ipUC.CurrentDv.Excute(strTag);
        }


        private int _ww = 65535, _wl = 30000, _quickWW = 0, _quickWL = 0;
        private bool _running = true;
        /// <summary>
        /// 完成设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wlSetting_CloseSettingEvent(int ww, int wl)
        {
            _quickWW = ww;
            _quickWL = wl;
            pWL.IsOpen = false;
            btnWL.Tag = "";
            ApplyConfigWL(false);
        }
        /// <summary>
        /// 窗宽窗位设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenWLSetting(object sender, MouseButtonEventArgs e)
        {
            if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
            {
                ipUC.CurrentDv.GetWindowLevel(ref _ww, ref _wl);
            }
            pWL.IsOpen = true;
        }
        /// <summary>
        /// 自动窗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAutoWLClick(object sender, RoutedEventArgs e)
        {
            //ipUC.AutoWindowLevel();
            //2019-3-4 14:39:49 取消自动窗宽窗外，修改为应用/取消 快捷窗宽窗位设置
            var source = sender as FrameworkElement;
            if (source != null)
            {
                string flag = "APPLY";
                if (source.Tag.ToString() != flag)
                {
                    source.Tag = flag;
                    ApplyConfigWL(true);
                }
                else if (source.Tag.ToString() == flag)
                {
                    source.Tag = string.Empty;
                    ApplyConfigWL(false);
                }
            }
        }
        /// <summary>
        /// 快捷窗位窗宽设置
        /// </summary>
        /// <param name="p"></param>
        private void ApplyConfigWL(bool p)
        {
            if (ipUC == null || ipUC.CurrentDv == null || !ipUC.CurrentDv.HasImage)
            {
                return;
            }
            if (p)
            {
                ipUC.CurrentDv.GetWindowLevel(ref _ww, ref _wl);
                ipUC.CurrentDv.SetWindowLevel(_quickWW, _quickWL);
            }
            else
            {
                ipUC.CurrentDv.SetWindowLevel(_ww, _wl);
            }
            ipUC.CurrentDv.Invalidate();
        }
        /// <summary>
        /// 重新加载图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetImage(object sender, RoutedEventArgs e)
        {
            if (ipUC.CurrentDv != null)
            {
                ipUC.CurrentDv.ResetImage();
                ipUC.CurrentDv.SetScaleRatio(CalculateScaleRatio());//暂时保留，后续废除
            }
        }
        /// <summary>
        /// 拖动条浏览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sldrImageIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (CurrentFiles != null && CurrentFiles.Count >= sldrImageIndex.Value)
                {
                    int index = (int)sldrImageIndex.Value - 1;
                    if (ipUC != null && ipUC.CurrentDv != null && index >= 0)
                    {
                        int ww = 0, wl = 0;
                        if (ipUC.CurrentDv.HasImage)
                            ipUC.CurrentDv.GetWindowLevel(ref ww, ref wl);
                        ipUC.CurrentDv.LoadFile(CurrentFiles[index]);
                        if (ww != 0 && wl != 0)
                            ipUC.CurrentDv.SetWindowLevel(ww, wl);
                    }
                    ipUC.CurrentDv.Invalidate();
                    ipUC.CurrentDv.SetScaleRatio(CalculateScaleRatio());//暂时保留，后续废除
                }
            }
            catch (Exception ex)
            {
                CMessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// 切换序列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        /// <summary>
        /// 拖放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                string fileName = ((sender as FrameworkElement).DataContext as PlatformFilesModel).DcmFiles[0];
                DragDrop.DoDragDrop(sender as FrameworkElement, fileName, DragDropEffects.Copy);
            }
        }


        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnDo(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();

            if (ipUC.CurrentDv == null)
                return;
            if (tag != "FalseColor" && tag != "Back")
                ipUC.CurrentDv.SaveToStack();
            switch (tag)
            {
                case "Sharp":
                    if (ipUC.CurrentDv.HasImage) {
                        // ipUC.CurrentDv.SharpImage(1);
                            ushort[] result = usmSetting.UnsharpenMask(ipUC.CurrentDv);
                            ipUC.CurrentDv.GetImageSize(out ushort width, out ushort height, out ushort bits, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
                            ipUC.CurrentDv.PutImageData(width, height, bits, ref result[0]);
                            ipUC.CurrentDv.RefreshImage();
                            ipUC.CurrentDv.Invalidate();
                    }

                    break;
                case "EqualHist":
                    if (ipUC.CurrentDv.HasImage)
                    {
                        ipUC.CurrentDv.EqualHistImage();
                        // ipUC.AutoWindowLevel();
                    }
                    break;
                case "ReduceNoise":
                    if (ipUC.CurrentDv.HasImage)
                        ipUC.CurrentDv.ReduceNoise();
                    break;
                case "FalseColor":
                    bool isApply = (bool)btnFalsecolor.IsChecked;
                    string lutFile = NV.DRF.Core.Model.GeneralSettingHelper.Instance.LutFile;
                    WndGeneralSetting wnd = new WndGeneralSetting();
                    if (!wnd.CheckLut(lutFile))
                    {
                        CMessageBox.Show("伪彩Lut表未配置，请设置伪彩Lut表\nPlease select the colorsolution first.");
                        if (wnd.ShowDialogEx() != true)
                            return;
                    }
                    lutFile = NV.DRF.Core.Model.GeneralSettingHelper.Instance.LutFile;
                    ipUC.CurrentDv.SetFalseColor(isApply, lutFile);
                    ipUC.CurrentDv.Invalidate();
                    break;
                case "EdgeEnhancement":

                    if (ipUC.CurrentDv.HasImage)
                    {
                        ipUC.CurrentDv.EdgeEnhancement();
                    }
                    break;
                case "Cameo":

                    System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Multiselect = true;
                    dialog.Filter = "image files|*.bmp;*.jpg;*.jpeg;*.png";
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ushort width = 0;
                        ushort height = 0;
                        ushort[] result = Stitching(dialog.FileNames,ref width, ref height);

                    
                       // ipUC.CurrentDv.GetImageSize(out ushort width, out ushort height, out ushort bits, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
                        System.Console.WriteLine("result " + width.ToString() + " " + height.ToString());
                        ipUC.PutData(width, height, 16, result, true);
                        ipUC.AutoWindowLevel();
                        ipUC.CurrentDv.Invalidate();
                    }
               
                    break;
                case "Back":
                    ipUC.CurrentDv.BackFromStack();
                    ApplyConfigWL(true);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 仅用于对处理菜单四个按钮的菜单关闭效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CL_Closed(object sender, EventArgs e)
        {
            Popup pup = sender as Popup;
            CheckBox chk = pup.PlacementTarget as CheckBox;
            if (chk != null && !chk.IsMouseOver)
            {
                chk.IsChecked = false;
            }
        }

        private void UnLoaded(object sender, RoutedEventArgs e)
        {
            BrightVisionSDK.Uninit();

        }

        /// <summary>
        /// 浏览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImgProc_Click(object sender, ExecutedRoutedEventArgs e)
        {
            string cmd = e.Parameter.ToString();
            SliderImage(cmd);
        }

        public ObservableCollection<PlatformFilesModel> PlatformModels
        { set; get; }
        /// <summary>
        /// 刷新文件列表
        /// </summary>
        /// <param name="isClear"></param>
        public void UpdateProductFolder(bool isClearImage = false, bool isSkipSearchDir = false)
        {
            PlatformModels.Clear();
            if (isSkipSearchDir)
                return;
            if (isClearImage && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
            {
                ipUC.CurrentDv.ClearImage();
                ipUC.CurrentDv.Invalidate();
            }

            if (Global.CurrentProduct == null || !Directory.Exists(Global.CurrentProduct.ImageFolder))
                return;

            string[] subDir = Directory.GetDirectories(Global.CurrentProduct.ImageFolder);
            if (subDir.Length == 0)
                return;

            foreach (var dir in subDir)
            {
                string[] fils = Directory.GetFiles(dir, "*.dcm");
                if (fils.Length == 0)
                    continue;

                string png = string.Empty;
                string[] pngs = Directory.GetFiles(dir, "*.png");
                if (pngs.Length > 0)
                    png = pngs[0];

                PlatformFilesModel model = new PlatformFilesModel
                {
                    Thumbnail = png,
                    DcmFiles = new ObservableCollection<string>(fils),
                    DateTime = dir,
                };

                PlatformModels.Add(model);
            }

            if (PlatformModels.Count > 0)
                Global.MainWindow.ActiveAllFunction();
        }
        /// <summary>
        /// 浏览按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImgView_Click(object sender, RoutedEventArgs e)
        {
            string cmd = (sender as FrameworkElement).Tag.ToString();
            SliderImage(cmd);
        }
        /// <summary>
        /// 浏览按钮
        /// </summary>
        /// <param name="cmd"></param>
        private void SliderImage(string cmd)
        {
            switch (cmd)
            {
                case "First":
                    sldrImageIndex.Value = sldrImageIndex.Minimum;
                    break;
                case "Prev":
                    int value = (int)sldrImageIndex.Value - 1;
                    sldrImageIndex.Value = value < 1 ? sldrImageIndex.Maximum : value;
                    break;
                case "Next":
                    int value2 = (int)sldrImageIndex.Value + 1;
                    sldrImageIndex.Value = value2 > sldrImageIndex.Maximum ? 1 : value2;
                    break;
                case "Last":
                    sldrImageIndex.Value = sldrImageIndex.Maximum;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// 是否正在采集
        /// </summary>
        public bool IsAcqing { get; set; }
        /// <summary>
        /// 导出视频
        /// </summary>
        internal void ExportAVI()
        {
            if (Global.CurrentProduct == null || CurrentFiles == null || CurrentFiles.Count < 1)
            {
                return;
            }

            Console.WriteLine("===CurrentFiles[0]======== {0}", CurrentFiles[0]);
            if (CurrentFiles[0].Contains(" "))
            {
                try
                {
                    FileInfo fi;
                    string newDir, oldDir;
                    fi = new FileInfo(CurrentFiles[0]);
                    oldDir = fi.DirectoryName;
                    newDir = fi.DirectoryName.Replace(' ', '_');
                    Directory.Move(oldDir, newDir);
                    UpdateProductFolder();
                    CurrentFiles = Directory.GetFiles(newDir, "*.dcm").ToList();
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
            }



            int time = 12;
            string folder = Path.GetDirectoryName(CurrentFiles[0]);
            _file.OpenFile(CurrentFiles[0]);
            string exptime = _file.GetDicomString(0x18, 0x1150);//ExposureTime
            if (!int.TryParse(exptime, out time) || (time==0))
                time = 12;
            System.Windows.Forms.SaveFileDialog dia = new System.Windows.Forms.SaveFileDialog();
            dia.Filter = "*.avi|*.avi";

            Console.WriteLine("===time ======== {0}", time);

            if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int fps = 1000 / time;
                string source = Path.Combine(folder, "%03d.png");
                string output = dia.FileName;
                ExportAVI(fps, source, output);
            }
        }
        /// <summary>
        /// 导出视频
        /// </summary>
        /// <param name="fps"></param>
        /// <param name="source"></param>
        /// <param name="output"></param>
        static void ExportAVI(int fps, string source, string output)
        {

            Console.WriteLine("{0}, {1}", source, output);
            string parameters = string.Format("-r {0} -i {1} -b:v 2048k -vcodec mpeg4 {2} -y", fps, source, output);
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(System.Windows.Forms.Application.StartupPath, "Export\\ffmpeg.exe");
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.Arguments = parameters;
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
            p.Close();
            CMessageBox.Show("导出完成。\nExport completion");
        }



        /// <summary>
        /// 打开当前文件夹
        /// </summary>
        internal void OpenCurrentFolder()
        {
            if (_lstSeries.SelectedItem != null && (_lstSeries.SelectedItem as PlatformFilesModel) != null)
            {
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Global.CurrentProduct.ImageFolder, (_lstSeries.SelectedItem as PlatformFilesModel).DateTime));
            }
        }
        /// <summary>
        /// 删除所有标记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAllAnnotation(object sender, MouseButtonEventArgs e)
        {
            if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
            {
                ipUC.CurrentDv.DeleteAnnotation(true);
                ipUC.CurrentDv.Invalidate();
            }
        }
        /// <summary>
        /// 切换比例尺显示状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImgProcChangeLine_Click(object sender, RoutedEventArgs e)
        {
            if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
            {
                ipUC.CurrentDv.DrawScaleFlag = !ipUC.CurrentDv.DrawScaleFlag;
                ipUC.CurrentDv.Invalidate();
            }
        }
        /// <summary>
        /// 切换标记显隐状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImgProcChangeAnnotationVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (ipUC != null && ipUC.CurrentDv != null && ipUC.CurrentDv.HasImage)
            {
                ipUC.CurrentDv.DrawScaleFlag = !ipUC.CurrentDv.DrawScaleFlag;
                ipUC.CurrentDv.AnnotationVisible = ipUC.CurrentDv.DrawScaleFlag;
                ipUC.CurrentDv.RefreshImage();
            }
        }
        /// <summary>
        /// 纸质打印
        /// </summary>
        internal void PaperPrint()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;
            string fileName = Path.Combine(System.Windows.Forms.Application.StartupPath, "PrintCach.png");
            ushort w, h, b;
            dv.GetImageSize(out w, out h, out b, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
            dv.BurnAnnotation();
            dv.SaveImage(w, h, fileName, ImageViewLib.tagIMAGE_TYPE.IT_PNG, false);
            var pr = new Process
            {
                StartInfo =
                {
                    FileName = "PrintCach.png",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    Verb = "Print"
                }
            };
            pr.Start();
        }
        /// <summary>
        /// 导出图片
        /// </summary>
        internal void SaveImage()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;

            dv.SaveAnnotation(System.IO.Path.ChangeExtension(dv.CurrentFileName, ".xml"));

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "*.bmp|*.bmp|*.png|*.png|*.jpeg|*.jpeg|*.tiff|*.tiff";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string extension = System.IO.Path.GetExtension(dialog.FileName);
                ImageViewLib.tagIMAGE_TYPE type = extension == ".png" ? ImageViewLib.tagIMAGE_TYPE.IT_PNG : extension == ".bmp" ? ImageViewLib.tagIMAGE_TYPE.IT_BMP : extension == ".jpeg" ? ImageViewLib.tagIMAGE_TYPE.IT_JPEG : ImageViewLib.tagIMAGE_TYPE.IT_TIF;

                ushort w, h, b;
                dv.GetImageSize(out w, out h, out b, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
                dv.SaveImage(w, h, dialog.FileName, type, false);
            }
        }
        /// <summary>
        /// 修复报告
        /// </summary>
        internal void ReportUpdate()
        {
            string pathDicom = Path.Combine(System.Windows.Forms.Application.StartupPath, "Report\\ReportDicom.dll");
            string pathView = Path.Combine(System.Windows.Forms.Application.StartupPath, "Report\\ReportView.dll");
            NV.Infrastructure.UICommon.NativeMethods.RegsvrDLL(pathDicom);
            NV.Infrastructure.UICommon.NativeMethods.RegsvrDLL(pathView);
            CMessageBox.Show("修复完成。\nRepair completion");
        }


        /// <summary>
        /// 打开文件菜单
        /// </summary>
        internal void OpenFile()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null)
                return;
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "(*.dcm)|*.dcm";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dv.DeleteAnnotation(true);
                dv.LoadFile(dialog.FileName);
                dv.SetScaleRatio(CalculateScaleRatio());//暂时保留，后续废除
                dv.Invalidate();
            }
        }
        /// <summary>
        /// 关闭文件菜单
        /// </summary>
        internal void CloseFile()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;
            dv.ClearImage();
            dv.Invalidate();
            dv.CurrentFileName = string.Empty;
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        internal void SaveFile()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;
            // dv.BurnAnnotation();
            //dv.DeleteAnnotation(true);
            if (!string.IsNullOrEmpty(dv.CurrentFileName))
            {
                try
                {
                    dv.SaveToFile(dv.CurrentFileName, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL, false);
                    dv.SaveAnnotation(System.IO.Path.ChangeExtension(dv.CurrentFileName, ".xml"));
                    ushort w, h, b;
                    dv.GetImageSize(out w, out h, out b, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
                    dv.SaveImage(w, h, Path.ChangeExtension(dv.CurrentFileName, ".png"), ImageViewLib.tagIMAGE_TYPE.IT_PNG, false);
                }
                catch (Exception)
                {
                }
            }
            dv.Invalidate();
        }
        /// <summary>
        /// 另存
        /// </summary>
        internal void SaveAs()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "*.dcm|*.dcm";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dv.BurnAnnotation();
                dv.SaveToFile(dialog.FileName, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL, false);
                dv.SaveAnnotation(System.IO.Path.ChangeExtension(dv.CurrentFileName, ".xml"));
            }
        }

        /// <summary>
        /// 刷新标注
        /// </summary>
        internal void UpdataOverlay()
        {
            if (ipUC != null && ipUC.CurrentDv != null)
            {
                ipUC.CurrentDv.RefreshImage();
            }
        }

        /// <summary>
        /// 打开AOI设定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAOISetting(object sender, MouseButtonEventArgs e)
        {
            var wnd = new WndAOISetting();
            wnd.Left = SystemParameters.PrimaryScreenWidth - wnd.Width - 20;
            wnd.Top = SystemParameters.PrimaryScreenHeight - wnd.Height - 120;
            wnd.ShowDialog();
        }


        private void OpenUSMSetting(object sender, MouseButtonEventArgs e)
        {
            usmSetting = new WndUSMSetting();
            usmSetting.Left = SystemParameters.PrimaryScreenWidth - usmSetting.Width - 20;
            usmSetting.Top = SystemParameters.PrimaryScreenHeight - usmSetting.Height - 60;

            usmSetting.UsmParamChangedEvent += usmSetting_UsmParamChanged;
            usmSetting.BackSettingEvent += usmSetting_Back;
            
            usmSetting.ShowDialog();
        }


        private ushort[] Stitching(string[] imageFiles, ref ushort width, ref ushort height)
        {

            Stitcher.Mode mode = Stitcher.Mode.Panorama;
            Mat[] imgs = new Mat[imageFiles.Length];

            string names = "";
            //读入图像
            for (int i = 0; i < imageFiles.Length; i++)
            {
                imgs[i] = new Mat(imageFiles[i], ImreadModes.Color);
                names += imageFiles[i] + " \n";

            }

            CMessageBox.Show("开始缝合图片? " + names);
            Mat pano = new Mat();
            Stitcher stitcher = Stitcher.Create(mode);
           // CMessageBox.Show(String.Format("imgs size{0} {0}",imgs.Length, imgs[0].Size()));

            Stitcher.Status status = stitcher.Stitch(imgs, pano);
            width = (ushort)pano.Width;
            height = (ushort)pano.Height;
            ushort[] result = new ushort[width * height];

            if (status != Stitcher.Status.OK)
            {
                CMessageBox.Show(String.Format("缝合失败! 请保证图片大于1张，且每张之间至少具有30%的重复。 error code = {0} ", (int)status));
               // Console.WriteLine("Can't stitch images, error code = {0} ", (int)status);
                return result;
            }
          //  CMessageBox.Show("缝合完毕，准备显示");
           // Cv2.ImWrite("123.bmp",pano);

            // 显示
            // for (int i = 0; i < height; i++)
            System.Threading.Tasks.Parallel.For(0, height, i =>
            {
                for (int j = 0; j < pano.Width; j++)
                {
                    result[i * pano.Width + j] = pano.At<ushort>(i, j);
                }
            });

            return result;

        }


        internal void SaveScreenImage()
        {
            DicomViewer dv = ipUC.CurrentDv;
            if (dv == null || !dv.HasImage)
                return;

            System.Windows.Point start = ipUC.PointToScreen(new System.Windows.Point(0d, 0d));
            System.Windows.Rect area = new System.Windows.Rect(start.X, start.Y, ipUC.ActualWidth, ipUC.ActualHeight);
            Bitmap image = new Bitmap((int)area.Width, (int)area.Height);
            Graphics imgGraphics = Graphics.FromImage(image);
            //设置截屏区域
            imgGraphics.CopyFromScreen((int)area.X, (int)area.Y, 0, 0, new System.Drawing.Size((int)area.Width, (int)area.Height));

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "*.bmp|*.bmp|*.png|*.png|*.jpeg|*.jpeg|*.tiff|*.tiff";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.FileStream fs = new FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

    }
    /// <summary>
    /// 平台图像模型
    /// </summary>
    public class PlatformFilesModel : ObservableModel
    {
        private string _thumbnail;
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail
        {
            get
            {
                return _thumbnail;
            }
            set
            {
                Set(() => Thumbnail, ref _thumbnail, value);
            }
        }

        private ObservableCollection<string> _dcmFiles;
        /// <summary>
        /// dcm文件列表
        /// </summary>
        public ObservableCollection<string> DcmFiles
        {
            get
            {
                return _dcmFiles;
            }
            set
            {
                Set(() => DcmFiles, ref _dcmFiles, value);
            }
        }
        private string _date;
        /// <summary>
        /// 拍摄日期
        /// </summary>
        public string DateTime
        {
            get
            {
                return _date;
            }
            set
            {
                Set(() => DateTime, ref _date, value);
            }
        }
    }


  
}
