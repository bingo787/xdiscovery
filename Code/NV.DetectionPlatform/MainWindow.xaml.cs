using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using NV.DetectionPlatform.Entity;
using NV.DRF.Controls;
using NV.DRF.Core.Common;
using NV.DRF.Core.Ctrl;
using NV.DRF.Core.Global;
using NV.DRF.Core.Interface;
using NV.Infrastructure.UICommon;
using SerialPortController;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using NV.DetectionPlatform.Service;
namespace NV.DetectionPlatform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private Brush _normalForeground = (Brush)new BrushConverter().ConvertFromString("#FFF1F1F1");
        /// <summary>
        /// 高压控制器
        /// </summary>
        public static SerialPortControler_RS232PROTOCOL ControlSystem = SerialPortControler_RS232PROTOCOL.Instance;
        /// <summary>
        /// 采集控件
        /// </summary>
        public UCtrls.PageProductExam _examView;
        /// <summary>
        /// 高压KV UA设置控件
        /// </summary>
        public UCtrls.WndExamSetting _hVView;
        public MainWindow()
        {
            this.Initialized += InitData;
            InitializeComponent();
            this.Loaded += ResizeWindow;
            this.ContentRendered += MainWindow_ContentRendered;
            this.Closing += Shell_Closing;
            this.Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                //ControlSystem.CloseControlSystem();
                _examView.CloseExamThread();
            }
            catch (Exception)
            {
            }
           
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 双屏显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            NV.Config.HVGeneratorParam para = NV.Config.HVGeneratorParam.Instance;
            int left = 0, top = 0, width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                {
                    if(System.Windows.Forms.Screen.AllScreens[i]!=System.Windows.Forms.Screen.PrimaryScreen)
                    {
                        top = System.Windows.Forms.Screen.AllScreens[i].Bounds.Top;
                        left = System.Windows.Forms.Screen.AllScreens[i].Bounds.Left;
                        width = System.Windows.Forms.Screen.AllScreens[i].Bounds.Width;
                        height = System.Windows.Forms.Screen.AllScreens[i].Bounds.Height;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(para.ExternSoft))
                    NV.Infrastructure.UICommon.NativeMethods.OpenWindow(para.ExternSoft, left, top, width, height, this);
            }
        }

        #region 初始化控制器，高压、探测器监控
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitData(object sender, EventArgs e)
        {
#if ROCKEY
            this.Log("开始验证加密狗.");
            try
            {
                CheckRockey();
                this.Log("验证加密狗成功.");
            }
            catch (Exception ex)
            {
                CMessageBox.Show("加密狗模块加载失败，" + ex.Message);
                Process.GetCurrentProcess().Kill();
            }
#endif
            try
            {
                UpdateLanguage();

                this.Log("初始化页面.");
                _hVView = new UCtrls.WndExamSetting();
                this.Log("初始化采集页面.");
                _examView = new UCtrls.PageProductExam();
                this.Log("初始化注册页面.");
                bdrRegister.Child = new UCtrls.UCProductRegister();
                _hVView.StartAcqEvent += _hVView_StartAcqEvent;
                //_hVView.AutoWLEvent += _hVView_AutoWLEvent;
            }
            catch (Exception ex)
            {
                this.Log(ex.ToString());
                MessageBox.Show(ex.Message + ex.ToString());
            }
            this.Log("初始化串口.");
            InitilizeControlSystem();
            this.Log("初始化探测器.");
            InitilizeDetectorSystem();

            Global.MainWindow = this;
        }
        /// <summary>
        /// 自动窗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _hVView_AutoWLEvent(object sender, RoutedEventArgs e)
        {
            _examView.ipUC.AutoWindowLevel();
        }
        /// <summary>
        /// 立即采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _hVView_StartAcqEvent(object sender, RoutedEventArgs e)
        {
            ProductExamStart_Click(null, null);
        }
        /// <summary>
        /// 检查加密狗
        /// </summary>
        private void CheckRockey()
        {
            this.Log("check user");
            DateTime dtDeadline, dtUTC;
            if (NV.DRF.Core.DongleHelper.Login("nanovisiondt"))
            {
                if (NV.DRF.Core.DongleHelper.GetUTCTime(out dtUTC) && NV.DRF.Core.DongleHelper.GetDeadline(out dtDeadline))
                {
                    if (dtUTC >= dtDeadline)
                    {
                        CMessageBox.Show("您的授权已过期，请于供应商联系.");
                    }
                    else
                    {
                        if ((dtDeadline - dtUTC).Days < 10)
                        {
                            CMessageBox.Show("您的授权即将过期（还可使用" + ((dtDeadline - dtUTC).Days).ToString() + "天，为了不影响您的工作，请及时与供应商联系.");
                        }
                        return;
                    }
                }
            }
            else
            {
                this.Log("check user faild");
                var res = NV.DRF.Core.DongleHelper.Error;
                if (res == Dongle.DongleDef.DONGLE_CLOCK_EXPIRE)
                {
                    CMessageBox.Show("您的授权已过期，请于供应商联系.");
                }
                else
                    CMessageBox.Show("未检测到开发商加密锁，请检查加密锁类型并重新插入加密锁.");
            }

            Process.GetCurrentProcess().Kill();
        }
        /// <summary>
        /// 初始化串口控制器
        /// </summary>
        private void InitilizeControlSystem()
        {
            try
            {
                ControlSystem.XRayEnableChanged += ControlSystem_XRayOnChanged;
                ControlSystem.XRayOnChanged += ControlSystem_XRayOnChanged;
                ControlSystem.VoltageChanged += ControlSystem_VoltageChanged;
                ControlSystem.VoltageSettingChanged += ControlSystem_VoltageSettingChanged;
                ControlSystem.CurrentChanged += ControlSystem_CurrentChanged;
                ControlSystem.CurrentSettingChanged += ControlSystem_CurrentSettingChanged;
                ControlSystem.FilamentMonitorChanged += ControlSystem_FilamentMonitorChanged;
                ControlSystem.TemperatureChanged += ControlSystem_TemperatureChanged;
                ControlSystem.StateReported += ControlSystem_StateReported;
                ControlSystem.FaultCleared += ControlSystem_FaultCleared;
                ControlSystem.Connected += ControlSystem_Connected;

                ConnControlSystem();
            }
            catch (Exception)
            {
                CMessageBox.Show("高压通讯串口初始化失败，请检查串口设置\nInitialization of high voltage communication serial port failed. Please check the serial port settings.");
                lblHVConn.Content = "--";
            }
        }

        /// <summary>
        /// 初始化串口控制器
        /// </summary>
        private void ConnControlSystem()
        {
            try
            {
                try
                {
                    ControlSystem.ClosePort();
                }
                catch { }
                ControlSystem.OpenSerialPort();
                lblHVConn.Content = "--";

                ControlSystem.Connect();
                Thread.Sleep(150);

                ControlSystem.SendCommand("MON");
            }
            catch (Exception)
            {
                CMessageBox.Show("高压通讯串口打开失败，请检查串口设置\nHigh-voltage communication serial port failed to open, please check the serial port settings");
                lblHVConn.Content = "--";
            }
        }
        void ControlSystem_Connected()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHVConn.Content = Connected;
            }));
        }
        /// <summary>
        /// 高压复位
        /// </summary>
        void ControlSystem_FaultCleared()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
             {
                 lblHV_Error.Content = string.Empty;
                 lblHV_Error.Foreground = _normalForeground;
             }));
        }
        /// <summary>
        /// 高压出错信息监控
        /// </summary>
        /// <param name="report"></param>
        void ControlSystem_StateReported(string report)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_Error.Content = report;
                lblHV_Error.Foreground = System.Windows.Media.Brushes.Red;
            }));
        }
        /// <summary>
        /// 设定电流改变
        /// </summary>
        /// <param name="arg"></param>
        private void ControlSystem_CurrentSettingChanged(uint arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_SettingCur.Content = arg + "uA";
            }));
        }
        /// <summary>
        /// 设定电压改变
        /// </summary>
        /// <param name="arg"></param>
        private void ControlSystem_VoltageSettingChanged(double arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_SettingKV.Content = arg + "kV";
            }));
        }

        /// <summary>
        /// 高压温度监控
        /// </summary>
        /// <param name="arg"></param>
        void ControlSystem_TemperatureChanged(double arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_Temperature.Content = arg.ToString("f2") + "℃";
            }));
        }
        /// <summary>
        /// 灯丝监控
        /// </summary>
        /// <param name="arg"></param>
        void ControlSystem_FilamentMonitorChanged(uint arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_Fila.Content = arg + "mA";
            }));
        }
        /// <summary>
        /// 电流监控
        /// </summary>
        /// <param name="arg"></param>
        void ControlSystem_CurrentChanged(uint arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_Cur.Content = arg + "uA";
            }));
        }
        /// <summary>
        /// 电压监控
        /// </summary>
        /// <param name="arg"></param>
        void ControlSystem_VoltageChanged(double arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblHV_KV.Content = arg + "kV";
            }));
        }
        /// <summary>
        /// Xray曝光监控
        /// </summary>
        /// <param name="arg"></param>
        void ControlSystem_XRayOnChanged(bool arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (arg)
                {
                    lblHV_XRayState.Content = "XRay ON";
                    lblHV_XRayState.Foreground = Brushes.Yellow;
                }
                else
                {
                    lblHV_XRayState.Content = "XRay OFF";
                    lblHV_XRayState.Foreground = _normalForeground;
                }
            }));

        }
        /// <summary>
        /// 初始化探测器状态监控
        /// </summary>
        private void InitilizeDetectorSystem()
        {
            try
            {
                if (_examView != null && _examView.IsConnected)
                    lblDetector.Content = Connected;
                float t1, t2;
                if (Detector.DetectorController.Instance.GetTemperature(out t1, out  t2))
                    Instance_TemperatureChangedEvent(t1, t2);
                Detector.DetectorController.Instance.TemperatureChangedEvent += Instance_TemperatureChangedEvent;
                Detector.DetectorController.Instance.ConnBreakEvent += Instance_ConnBreakEvent;
                Detector.DetectorController.Instance.AcqMaxFramesEvent += Instance_AcqMaxFramesEvent;
            }
            catch (Exception ex)
            {
                CMessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 采集到最大帧停止采集
        /// </summary>
        void Instance_AcqMaxFramesEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
               _examView.StopAcq(null, null);

            }));
        }
        /// <summary>
        /// 探测器断开连接
        /// </summary>
        void Instance_ConnBreakEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblDetector.Content = "--";
                Instance_TemperatureChangedEvent(0, 0);
                lblWorkState.Content = "--";
                CMessageBox.Show("探测器已断开连接\nDetector disconnected");
            }));
        }

        /// <summary>
        /// 探测器温度监控
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Instance_TemperatureChangedEvent(float a, float b)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblDetector_Temperature.Content = string.Format("{0}℃ , {1}℃", a, b);
            }));
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeWindow(object sender, RoutedEventArgs e)
        {
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight - 40;
            this.Left = 0;
            this.Top = 0;

            vbProductInfo.MaxWidth = this.Width - 2 * menuBar.ActualWidth - 100;
            this.MainView.Content = _examView;

            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                lblVersion.Content = "Ver:" + version.ToString();

                NV.Config.HVGeneratorParam preheat = NV.Config.HVGeneratorParam.Instance;
                if (preheat.IsAutoPreheat)
                {
                    var wnd = new NV.DetectionPlatform.UCtrls.WndPreheat();
                    wnd.tbKV.Text = lblKV.Content.ToString();
                    wnd.tbUA.Text = lblUA.Content.ToString();
                    wnd.tbFila.Text = lblHV_Fila.Content.ToString();
                    wnd.ShowDialogEx();
                }
            }
            catch (Exception ex)
            {
                CMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 退出软件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CMessageBox.Show("您确实要退出软件吗？\nAre you sure you want to Exit?", "提示", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                e.Cancel = true;
                return;
            }
        }

        #endregion

        #region 菜单
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            _examView.OpenFile();
        }
        private void CloseFile(object sender, RoutedEventArgs e)
        {
            _examView.CloseFile();
        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            _examView.SaveFile();
        }
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            _examView.SaveAs();
        }
        private void SaveScreen(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _examView.SaveScreenImage();
        }
        /// <summary>
        /// 设置菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingClick(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            if (tag == "PortSetting")
            {
                UCtrls.WndPortSetting wnd = new UCtrls.WndPortSetting();
                wnd.ShowDialogEx();
            }
            if (tag == "PreheatSetting")
            {
                UCtrls.WndPreheatSetting wnd = new UCtrls.WndPreheatSetting();
                wnd.ShowDialogEx();
            }
            if (tag == "DetectorSetting")
            {
                UCtrls.WndDetectorSetting wnd = new UCtrls.WndDetectorSetting();
                wnd.ShowDialogEx();
            }
            if (tag == "DetectorReConnect")
            {
                lblDetector.Content = "--";
                Instance_TemperatureChangedEvent(0, 0);

                _examView.InitilizeDetector();
                if (_examView.IsConnected)
                {
                    lblDetector.Content = Connected;
                    float t1, t2;
                    if (Detector.DetectorController.Instance.GetTemperature(out t1, out  t2))
                        Instance_TemperatureChangedEvent(t1, t2);
                }

            }
            if (tag == "PortReOpen")
            {
                ConnControlSystem();
            }
            if (tag == "DetectorOffsetCorrection")
            {
                Detector.DetectorController.Instance.StartOffset();
            }
            if (tag == "DetectorGainCorrection")
            {
                Detector.DetectorController.Instance.StartGain();
            }
            if (tag == "DetectorDefectCorrection")
            {
                Detector.DetectorController.Instance.StartAutoDetect();
            }
            if (tag == "GeneralSetting")
            {
                UCtrls.WndGeneralSetting wnd = new UCtrls.WndGeneralSetting();
                wnd.ShowDialogEx();
            }
            if (tag == "SafetySetting")
            {
                UCtrls.WndHVSafetySetting wnd = new UCtrls.WndHVSafetySetting();
                wnd.ShowDialogEx();
            }
            if (tag == "FilmSetting")
            {
                UCtrls.WndFilmSetting wnd = new UCtrls.WndFilmSetting();
                wnd.ShowDialogEx();
            }
        }
        /// <summary>
        /// 帮助菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            switch (tag)
            {
                case "ReportUpdate":
                    {
                        _examView.ReportUpdate();

                    }
                    break;
                case "MultiEnergyAvgGrab":
                    {
                        new NV.DetectionPlatform.UCtrls.WndHVSetting().ShowDialog();
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region 功能区
        /// <summary>
        /// 查询产品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductBrower_Click(object sender, RoutedEventArgs e)
        {
            new UCtrls.WndProductBrowser().ShowDialogEx();
        }

        /// <summary>
        /// 高压复位、预热
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortCMD(object sender, RoutedEventArgs e)
        {
            string cmd = (sender as FrameworkElement).Tag.ToString();
            if (cmd == "ResetHV")
            {
                ControlSystem.ResetHV();
            }
            if (cmd == "Preheat")
            {
                var wnd = new NV.DetectionPlatform.UCtrls.WndPreheat();
                wnd.tbKV.Text = lblKV.Content.ToString();
                wnd.tbUA.Text = lblUA.Content.ToString();
                wnd.tbFila.Text = lblHV_Fila.Content.ToString();
                wnd.ShowDialogEx();
            }
        }
        private void PreheatSetting(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UCtrls.WndPreheatSetting wnd = new UCtrls.WndPreheatSetting();
            wnd.ShowDialogEx();
        }
        /// <summary>
        /// 手动开始采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductExamStart_Click(object sender, RoutedEventArgs e)
        {
            ExamType type = btnSpot.IsChecked == true ? ExamType.Spot : ExamType.Expose;
            _examView.StartAcq(type, (bool)btnIsStored.IsChecked);
        }
        /// <summary>
        /// 手动结束采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductExamStop_Click(object sender, RoutedEventArgs e)
        {
            _examView.StopAcq(null, null);
            btnIsStored.IsChecked = false;
        }
        /// <summary>
        /// 透视环
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductExamRing_Click(object sender, RoutedEventArgs e)
        {
            bool isStored = (sender as ToggleButton).IsChecked == true;
            Detector.DetectorController.Instance.IsStored = isStored;
        }
        /// <summary>
        /// 调节KV UA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductExam_Click(object sender, RoutedEventArgs e)
        {
            string type = (sender as FrameworkElement).Tag.ToString();
            lblExamType.Content = type;
        }
        /// <summary>
        /// 设置曝光方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExamSetting(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as RadioButton).IsChecked = true;
            string type = (sender as FrameworkElement).Tag.ToString();
            _hVView.Title = type;
            _hVView.Visibility = Visibility.Visible;
            StartGrab.IsEnabled = true;
        }
        /// <summary>
        /// 图像操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Excute(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            switch (tag)
            {
                case "SaveImage":
                    {
                        _examView.SaveImage();
                    }
                    break;
                case "OpenFolder":
                    {
                        if (Global.CurrentProduct != null && !string.IsNullOrEmpty(Global.CurrentProduct.ImageFolder))
                        {
                            if (!Directory.Exists(Global.CurrentProduct.ImageFolder))
                            {
                                return;
                            }
                            _examView.OpenCurrentFolder();
                        }
                    }
                    break;
                case "Print":
                    {
                        _examView.PaperPrint();
                    }
                    break;
                case "Report":
                    {
                        _examView.Report();
                    }
                    break;
                case "SaveAVI":
                    {
                        _examView.ExportAVI();
                    }
                    break;
                case "SaveADCM":
                    {
                        SaveFile(null, null);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 视图分格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrintLayout_Checked(object sender, RoutedEventArgs e)
        {
            LayoutType type = (LayoutType)Enum.Parse(typeof(LayoutType), (sender as FrameworkElement).Tag.ToString());
            if (_examView != null)
                _examView.ipUC.SetLayout(type);
            if ((sender as ToggleButton) != null)
                (sender as ToggleButton).IsChecked = false;
        }

        #endregion

        #region IMainWindow 成员

        public void Freeze()
        {

        }

        public void Freeze(string title)
        {

        }

        public void UnFreeze()
        {

        }

        public void ActiveAllFunction()
        {
            wplReport.IsEnabled = true;
        }

        public void NavigateTo(DRF.Core.Global.ModuleMode moduleMapping)
        {

        }

        public bool Exist(DRF.Core.Global.ModuleMode moduleMapping)
        {
            return true;
        }
        /// <summary>
        /// 登记产品开始检查
        /// </summary>
        /// <param name="pro"></param>
        public void UpdateProduct(object pro,bool isSkipSearchDir=false)
        {
            if (pro == null)
                return;
            Product p = pro as Product;
            if (p == null)
                return;

            txtProductName.Text = string.IsNullOrEmpty(p.ProductName) ? "--" : p.ProductName;
            txtProductType.Text = string.IsNullOrEmpty(p.ProductTypeID) ? "--" : p.ProductTypeID;
            txtProductSpe.Text = string.IsNullOrEmpty(p.ProductSpecification) ? "--" : p.ProductSpecification;
            //txtProductKeywords.Text = string.IsNullOrEmpty(p.ProductKeywords) ? "无" : p.ProductKeywords;

            _examView.UpdateProductFolder(true, isSkipSearchDir);
        }
        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="timeout"></param>
        public void NotifyTip(string title, string text, int timeout = 500)
        {
            if (title == "Detector")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (text.Contains("采集"))
                    {
                        lblWorkState.Foreground = Brushes.Yellow;
                        lblWorkState.Content = "Busy";
                    }
                    else if (text.Contains("停止"))
                    {
                        lblWorkState.Foreground = Brushes.Yellow;
                        lblWorkState.Content = "Stopped";
                    }
                    else
                    {
                        lblWorkState.Foreground = _normalForeground;
                        lblWorkState.Content = "Ready";
                    }
                }));
            }
            else if (title == "Param")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Global.CurrentParam != null)
                    {
                        lblKV.Content = Global.CurrentParam.KV.ToString() + "kV";
                        lblUA.Content = Global.CurrentParam.UA.ToString() + "uA";
                        lblTime.Content = Global.CurrentParam.Time.ToString() + "s";
                        lblFps.Content = Global.CurrentParam.Fps.ToString() + "Fps";
                    }
                }));
            }
            else if (title == "Film")
            {
                _examView.UpdataOverlay();
            }
        }

        #endregion
        /// <summary>
        /// 多能叠加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductExamMultiEnergy_Click(object sender, RoutedEventArgs e)
        {
            new NV.DetectionPlatform.UCtrls.WndHVSetting().ShowDialog();
        }
        /// <summary>
        /// 语言设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageSetting(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            if (tag == "CN")
            {
                Properties.Settings.Default.Language = NV.Infrastructure.UICommon.Language.zh_CN;
            }
            else if (tag == "US")
            {
                Properties.Settings.Default.Language = NV.Infrastructure.UICommon.Language.en_US;
            }

            Properties.Settings.Default.Save();

            UpdateLanguage();
        }
        /// <summary>
        /// 更新程序语言
        /// </summary>
        public void UpdateLanguage()
        {
            var lan = Properties.Settings.Default.Language;
            string path = lan == NV.Infrastructure.UICommon.Language.en_US ? @"Style/en_US.xaml" : @"Style/zh_CN.xaml";
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri(path, UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Remove(rd);
            Application.Current.Resources.MergedDictionaries.Add(rd);

        }

        public string Connected
        {
            get
            {
                if (Properties.Settings.Default.Language == NV.Infrastructure.UICommon.Language.en_US)
                    return "Connected";
                return "已连接";
            }
        }
    }
}
