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
       // private int _preheatMinutes;
       // private TimeSpan _span;
        System.Windows.Threading.DispatcherTimer _timer;

        public WndPreheat(int  timeMinutes)
        {
            InitializeComponent();

            InitilizeControlSystem();

            this.Loaded += StartMC110WarmUp;

            this.Closing += WndPreheat_Closing;
        }


        /// <summary>
        /// 初始化串口控制器
        /// </summary>
        private void InitilizeControlSystem()
        {
            try
            {
             //   MainWindow.ControlSystem.XRayOnChanged += ControlSystem_XRayOnChanged;
             //   MainWindow.ControlSystem.VoltageChanged += ControlSystem_VoltageChanged;
            //    MainWindow.ControlSystem.CurrentChanged += ControlSystem_CurrentChanged;
             //   MainWindow.ControlSystem.FilamentMonitorChanged += ControlSystem_FilamentMonitorChanged;
              //  MainWindow.ControlSystem.StateReported += ControlSystem_StateReported;
            }
            catch (Exception)
            {
            }
        }

        private void RemoveInitCallback() {
            try
            {
              //  MainWindow.ControlSystem.XRayOnChanged -= ControlSystem_XRayOnChanged;
              //  MainWindow.ControlSystem.VoltageChanged -= ControlSystem_VoltageChanged;
              //  MainWindow.ControlSystem.CurrentChanged -= ControlSystem_CurrentChanged;
              //  MainWindow.ControlSystem.FilamentMonitorChanged -= ControlSystem_FilamentMonitorChanged;
              //  MainWindow.ControlSystem.StateReported -= ControlSystem_StateReported;

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
        /// 倒计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MC110WarmUpTimerTick(object sender, EventArgs e)
        {
            Console.WriteLine("SerialPortControler_RS232PROTOCOL_MC110.Instance.IsWarmingUpStep = {0}", SerialPortControler_RS232PROTOCOL_MC110.Instance.IsWarming);

            if (SerialPortControler_RS232PROTOCOL_MC110.Instance.IsWarming == true)
            {
                Console.WriteLine("正在预热！");
                //  RemoveInitCallback();
                //  btnOK.Content = runState.Text = "完成";
                //   System.Media.SystemSounds.Exclamation.Play();
                if (this.Visibility == Visibility.Hidden) {
                    this.Visibility = Visibility.Visible;
                }

                runState.Text = SerialPortControler_RS232PROTOCOL_MC110.Instance.TS_Step + "/5";
                tbTimeSpan.Text = SerialPortControler_RS232PROTOCOL_MC110.Instance.TS_Elapsed_Time;
                tbPower.Text = SerialPortControler_RS232PROTOCOL_MC110.Instance.TS_Pwr_Step;

                int kv = 0;
                int.TryParse(SerialPortControler_RS232PROTOCOL_MC110.Instance.TS_Volt_Step, out kv);
                tbKV.Text = (kv / 1000).ToString();
            }
            else {
                if (this.Visibility == Visibility.Visible)
                {
                    this.Visibility = Visibility.Hidden;
                }

            }
           // _span = _span.Add(new TimeSpan(0, 0, -1));


        }
        /// <summary>
        /// 取消预热
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AbortPreheat(object sender, RoutedEventArgs e)
        {
           // MainWindow.ControlSystem.XRayOff();
           // RemoveInitCallback();
            //_span = TimeSpan.Zero;
            this.Close();
        }

        private void WndPreheat_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //未预热完成，开始后台预热
                if (SerialPortControler_RS232PROTOCOL_MC110.Instance.IsWarming == true)
                {
                    e.Cancel = true;
                    if (CMessageBox.Show("尚未完成预热，关闭后将无法正常使用光源,下次启动时将会继续预热", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        _timer.Stop();
                        this.Close();
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

           // _span = TimeSpan.FromMinutes(_preheatMinutes);
            //runPreheatTime.Text = _preheatMinutes.ToString();
            tbTimeSpan.Text = "00:00";

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += MC110WarmUpTimerTick;
            _timer.Start();
        }



    }
}
