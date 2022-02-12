using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NV.DRF.Controls;
using SerialPortController;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndPreheat : Window
    {
        private int _preheatMinutes;
        private TimeSpan _span;
        private int _warmUpStep = 0;
        System.Windows.Threading.DispatcherTimer _timer;

        public WndPreheat(bool isAuto)
        {
            InitializeComponent();

            InitilizeControlSystem();

            if (isAuto)
            {
                this.Loaded += StartMC110WarmUp;
            }
            else
            {
                this.Loaded += StartPreheat;
            }

            this.Closing += WndPreheat_Closing;
        }


        /// <summary>
        /// 初始化串口控制器
        /// </summary>
        private void InitilizeControlSystem()
        {
            try
            {
                MainWindow.ControlSystem.XRayOnChanged += ControlSystem_XRayOnChanged;
                MainWindow.ControlSystem.VoltageChanged += ControlSystem_VoltageChanged;
                MainWindow.ControlSystem.CurrentChanged += ControlSystem_CurrentChanged;
                MainWindow.ControlSystem.FilamentMonitorChanged += ControlSystem_FilamentMonitorChanged;
                MainWindow.ControlSystem.StateReported += ControlSystem_StateReported;
                MainWindow.ControlSystem.XRayEnableChanged += ControlSystem_XRayOnChanged;
            }
            catch (Exception)
            {
            }
        }

        private void ControlSystem_XRayOnChanged(bool arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                runState.Text = arg ? "正在预热" : "--";
            }));
        }

        private void ControlSystem_StateReported(string report)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                runState.Text = report;
            }));
        }

        private void ControlSystem_FilamentMonitorChanged(uint arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbFila.Text = arg.ToString();
            }));
        }

        private void ControlSystem_CurrentChanged(uint arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                double kv = 0.0f;
#if DEBUG
                Console.WriteLine("监控的电压和电流 tbKV {0} current {1}",tbKV.Text, arg);
#endif                
                if (!double.TryParse(tbKV.Text, out kv )) {
                    tbPower.Text = "Unknown";
                    return;
                }
                double current_mA = arg * 0.1 * 0.001;
                // 功率 W
                tbPower.Text = (current_mA * kv).ToString("f1");

            }));
        }

        private void ControlSystem_VoltageChanged(double arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbKV.Text = arg.ToString("f1");
            }));
        }
        /// <summary>
        /// 开始预热操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartPreheat(object sender, RoutedEventArgs e)
        {
            NV.Config.HVGeneratorParam preheat = NV.Config.HVGeneratorParam.Instance;

            Console.WriteLine("开始预热 电压 {0}, 功率 {1},时间 {2} ", preheat.PreheatKV, preheat.PreheatPower, preheat.PreheatMinutes);

            MainWindow.ControlSystem.Preheat(preheat.PreheatKV, preheat.PreheatPower);

            _preheatMinutes = preheat.PreheatMinutes;
            _span = TimeSpan.FromMinutes(_preheatMinutes);
            runPreheatTime.Text = _preheatMinutes.ToString();
            tbTimeSpan.Text = _span.Minutes.ToString("d2") + ":" + _span.Seconds.ToString("d2");

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
            _timer.Start();
        }
        /// <summary>
        /// 倒计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            if (_span <= TimeSpan.Zero)
            {
                MainWindow.ControlSystem.XRayOff();
                btnOK.Content = runState.Text = "完成";
                System.Media.SystemSounds.Exclamation.Play();

                if (this.Visibility == Visibility.Hidden)
                {
                    this.Visibility = Visibility.Visible;
                }
                tbTimeSpan.Foreground = System.Windows.Media.Brushes.Green;
                return;
            }

            _span = _span.Add(new TimeSpan(0, 0, -1));
            tbTimeSpan.Text = _span.Minutes.ToString("d2") + ":" + _span.Seconds.ToString("d2");
        }
        /// <summary>
        /// 倒计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MC110WarmUpTimerTick(object sender, EventArgs e)
        {
            if (_span <= TimeSpan.Zero)
            {
                _warmUpStep = 0;
                MainWindow.ControlSystem.XRayOff();
                btnOK.Content = runState.Text = "完成";
                System.Media.SystemSounds.Exclamation.Play();

                if (this.Visibility == Visibility.Hidden)
                {
                    this.Visibility = Visibility.Visible;
                }
                tbTimeSpan.Foreground = System.Windows.Media.Brushes.Green;
                return;
            }
            _span = _span.Add(new TimeSpan(0, 0, -1));
            tbTimeSpan.Text = _span.Minutes.ToString("d2") + ":" + _span.Seconds.ToString("d2");
            
            if (_span.Minutes == 14 && _warmUpStep == 1)
            {
                MainWindow.ControlSystem.MC110UpdateCMD(40, 7.5);
                _warmUpStep += 1;
            }
            else if (_span.Minutes == 11 && _warmUpStep == 2)
            {
                MainWindow.ControlSystem.MC110UpdateCMD(60, 9);
                _warmUpStep += 1;
            }
            else if (_span.Minutes == 8 && _warmUpStep == 3)
            {
                MainWindow.ControlSystem.MC110UpdateCMD(80, 12);
                _warmUpStep += 1;
            }
            else if (_span.Minutes == 4 && _warmUpStep == 4)
            {
                MainWindow.ControlSystem.MC110UpdateCMD(100, 15);
                _warmUpStep += 1;
            }


        }
        /// <summary>
        /// 取消预热
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AbortPreheat(object sender, RoutedEventArgs e)
        {
            MainWindow.ControlSystem.XRayOff();
            _span = TimeSpan.Zero;
            this.Close();
        }

        private void WndPreheat_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //未预热完成，开始后台预热
                if (_span > TimeSpan.Zero)
                {
                    e.Cancel = true;
                    if (CMessageBox.Show("预热将在后台继续进行,预热完成后将提示您。", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    else
                    {
                        this.Visibility = Visibility.Hidden;
                    }
                }
                else//预热完成，关闭计时器
                {
                    _timer.Stop();
                }
            }
            catch (Exception)
            {
            }
        }


        public void StartMC110WarmUp(object sender, RoutedEventArgs e) {
            /*
             20kV 5.0W 2min
             40kV 7.5W 3min
             60kV 9.0W 3min
             80kV 12.0W 4min
             100kV 15W 4min
             */
            _warmUpStep = 1;
            MainWindow.ControlSystem.Preheat(20, 5.0);

            _preheatMinutes = 16;
            _span = TimeSpan.FromMinutes(_preheatMinutes);
            runPreheatTime.Text = _preheatMinutes.ToString();
            tbTimeSpan.Text = _span.Minutes.ToString("d2") + ":" + _span.Seconds.ToString("d2");

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += MC110WarmUpTimerTick;
            _timer.Start();
        }



    }
}
