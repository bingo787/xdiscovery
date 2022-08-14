using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using NV.DetectionPlatform.Entity;
using NV.DRF.Controls;
using NV.DRF.Core.Global;
using SerialPortController;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndExamSetting : Window, INotifyPropertyChanged
    {
        ///// <summary>
        ///// 高压控制器
        ///// </summary>
        //public  ControlSystem = MainWindow.ControlSystem;
        public WndExamSetting()
        {
            InitializeComponent();
            InitilizeControlSystem();

            this.DataContext = this;
            ExamParams = new System.Collections.ObjectModel.ObservableCollection<ExamParam>();
            CurrentParam = new ExamParam();
            this.Loaded += WndExamSetting_Loaded;
        }

        /// <summary>
        /// 属性
        /// </summary>
        private ExamParam _currentParam;
        /// <summary>
        /// 获取或设置属性
        /// </summary>
        public ExamParam CurrentParam
        {
            get
            {
                return _currentParam;
            }
            set
            {
                if (_currentParam == value)
                {
                    return;
                }
                _currentParam = value;
                RaisePropertyChanged("CurrentParam");
            }
        }

        /// <summary>
        /// 初始化串口控制器
        /// </summary>
        private void InitilizeControlSystem()
        {
            try
            {
                var controlSystem = MainWindow.ControlSystem;
                controlSystem.XRayOnChanged += ControlSystem_XRayOnChanged;
                controlSystem.VoltageSettingChanged += ControlSystem_VoltageChanged;
                controlSystem.CurrentSettingChanged += ControlSystem_CurrentChanged;
                controlSystem.FilamentMonitorChanged += ControlSystem_FilamentMonitorChanged;
                controlSystem.StateReported += ControlSystem_StateReported;
                controlSystem.FaultCleared += ControlSystem_FaultCleared;
            }
            catch (Exception)
            {
            }
        }

        void ControlSystem_FaultCleared()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbError.Text = "--";
            }));
        }

        private void ControlSystem_XRayOnChanged(bool arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbError.Text = arg ? "XRAY ON" : "--";
            }));
        }

        private void ControlSystem_StateReported(string report)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbError.Text = report;
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
              //  tbUA.Text = arg.ToString();
            }));
        }

        private void ControlSystem_VoltageChanged(double arg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbKV.Text = arg.ToString("f1");
            }));
        }

        public System.Collections.ObjectModel.ObservableCollection<ExamParam> ExamParams { set; get; }

        void WndExamSetting_Loaded(object sender, RoutedEventArgs e)
        {
            ExamParams.Clear();
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                db.ExamParam.ToList().ForEach(t => ExamParams.Add(t));
            }
            if (ExamParams.Count > 0)
                CurrentParam = ExamParams[0];

            var setting = NV.Config.HVGeneratorParam.Instance;
            if (setting.MaxKV > 0)
            {
                sldrkv.Maximum = setting.MaxKV;
            }
            if (setting.MaxPower > 0)
            {
                sldrpower.Maximum = setting.MaxPower;
                _powerMax = setting.MaxPower;
            }
        }
        /// <summary>
        /// 额定功率
        /// </summary>
        private int _powerMax = int.MaxValue;
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 添加方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            double kv,power, time;
            int ua, fps;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\n Please input new solution name");
                return;
            }
            //if (!int.TryParse(txtua.Text, out ua))
            //{
            //    CMessageBox.Show("电流值不合法。\n Invalid current value");
            //    return;
            //}
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\n Invalid current value");
                return;
            }
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtTime.Text, out time))
            {
                CMessageBox.Show("曝光时间值不合法。\nInvalid spot time");
                return;
            }
            if (!int.TryParse(cboFps.Text, out fps))
            {
                CMessageBox.Show("帧速率值不合法。\n Invalid exposeFPS");
                return;
            }

            if (!CheckMaxPower())
                return;

            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.ExamParam.FirstOrDefault(para => para.Name == name);
                if (repeat != null)
                {
                    CMessageBox.Show("已存在该名称方案。The name already exists");
                    return;
                }
                else
                {
                    ExamParam param = new ExamParam();
                    param.GUID = System.Guid.NewGuid().ToString();
                    param.Name = name;
                    param.KV = kv;
                    param.Power = power;
                    param.UA = 0;
                    param.Time = time;
                    param.Fps = fps;
                    if (Global.CurrentProduct != null)
                    {
                        param.ProductType = Global.CurrentProduct.ProductTypeID;
                    }
                    db.AddToExamParam(param);
                    db.SaveChanges();
                    CMessageBox.Show("添加成功\n Operation completed");
                    WndExamSetting_Loaded(null, null);
                }
            }

        }
        /// <summary>
        /// 删除方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Del(object sender, RoutedEventArgs e)
        {
            if (lstParams.SelectedItem == null)
            {
                return;
            }
            ExamParam p = lstParams.SelectedItem as ExamParam;
            if (CMessageBox.Show("确定要删除该方案吗？\n Are you sure you want to delete it?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            else
            {
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    var repeat = db.ExamParam.FirstOrDefault(para => para.GUID == p.GUID);
                    if (repeat != null)
                    {
                        db.ExamParam.DeleteObject(repeat);
                        db.SaveChanges();
                        WndExamSetting_Loaded(null, null);
                    }
                }
            }
        }
        /// <summary>
        /// 保存修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save(object sender, RoutedEventArgs e)
        {
            if (lstParams.SelectedItem == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(txtName.Text))
            {
                CMessageBox.Show("请输入新方案名称\n Please input new solution name");
                return;
            }
            double kv,power, time;
            int ua, fps;

            //if (!int.TryParse(txtua.Text, out ua))
            //{
            //    CMessageBox.Show("电流值不合法。\n Invalid current value");
            //    return;
            //}
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtTime.Text, out time))
            {
                CMessageBox.Show("曝光时间值不合法。\nInvalid spot time");
                return;
            }
            if (!int.TryParse(cboFps.Text, out fps))
            {
                CMessageBox.Show("帧速率值不合法。\n Invalid exposeFPS");
                return;
            }
            if (!CheckMaxPower())
                return;

            ExamParam p = lstParams.SelectedItem as ExamParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.ExamParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;
                    repeat.KV = kv;
                    repeat.Power = power;
                    repeat.UA = 0;
                    repeat.Time = time;
                    repeat.Fps = fps;
                    if (Global.CurrentProduct != null)
                    {
                        repeat.ProductType = Global.CurrentProduct.ProductTypeID;
                    }
                    db.SaveChanges();
                    CMessageBox.Show("保存成功\nOperation completed");
                    WndExamSetting_Loaded(null, null);
                }
            }
        }
        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double kv, power,time;
            int ua = 0, fps;

            //if (!int.TryParse(txtua.Text, out ua))
            //{
            //    CMessageBox.Show("电流值不合法。\n Invalid current value");
            //    return;
            //}
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtTime.Text, out time))
            {
                CMessageBox.Show("曝光时间值不合法。\nInvalid spot time");
                return;
            }
            if (!int.TryParse(cboFps.Text, out fps))
            {
                CMessageBox.Show("帧速率值不合法。\n Invalid exposeFPS");
                return;
            }

            if (!CheckMaxPower())
                return;

            ExamParam param = new ExamParam();
            param.KV = kv;
            param.UA = ua;
            param.Power = power;
            param.Time = time;
            param.Fps = fps;

            Global.CurrentParam = param;
            var controlSystem = MainWindow.ControlSystem;

            controlSystem.SetKV(kv);
            System.Threading.Thread.Sleep(150);
            controlSystem.SetPower(power);
            this.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 设置电压
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetKV(object sender, RoutedEventArgs e)
        {
            double kv;
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid current value");
                return;
            }
            if (!CheckMaxPower())
                return;
            MainWindow.ControlSystem.SetKV(kv);

        }
        /// <summary>
        /// 检查当前kv ma是否超过额定功率，给出警告
        /// </summary>
        private bool CheckMaxPower()
        {
            if (sldrpower.Value > _powerMax * 1000)
            {
                CMessageBox.Show("功率上限为：" + _powerMax + "W,当前设置将超出安全额定功率！\n Currnet Power is too large!");
                return false;
            }
            return true;
        }
        ///// <summary>
        ///// 设置电流
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void SetUA(object sender, RoutedEventArgs e)
        //{
        //    int ua;
        //    if (!int.TryParse(txtua.Text, out ua))
        //    {
        //        CMessageBox.Show("电流值不合法。\nInvalid voltage value");
        //        return;
        //    }
        //    if (!CheckMaxPower())
        //        return;

        //    MainWindow.ControlSystem.SetCurrent(ua);
        //}
        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetP(object sender, RoutedEventArgs e)
        {
            double power;
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\nInvalid voltage value");
                return;
            }
            if (!CheckMaxPower())
                return;

            MainWindow.ControlSystem.SetPower(power);
        }
        /// <summary>
        /// 选择方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select(object sender, RoutedEventArgs e)
        {
            double kv,power, time;
            int ua = 0, fps;

            //if (!int.TryParse(txtua.Text, out ua))
            //{
            //    CMessageBox.Show("电流值不合法。\n Invalid current value");
            //    return;
            //}
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtTime.Text, out time))
            {
                CMessageBox.Show("曝光时间值不合法。\nInvalid spot time");
                return;
            }
            if (!int.TryParse(cboFps.Text, out fps))
            {
                CMessageBox.Show("帧速率值不合法。\n Invalid exposeFPS");
                return;
            }

            if (!CheckMaxPower())
                return;

            ExamParam param = new ExamParam();
            param.KV = kv;
            param.UA = ua;
            param.Power = power;
            param.Time = time;
            param.Fps = fps;

            Global.CurrentParam = param;

            MainWindow.ControlSystem.SetKV(kv);
            System.Threading.Thread.Sleep(150);
            MainWindow.ControlSystem.SetPower(power);
        }


        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string p)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
        #endregion

        private void StartAcq_Click(object sender, RoutedEventArgs e)
        {
            double kv, power,time;
            int ua = 0, fps;

            //if (!int.TryParse(txtua.Text, out ua))
            //{
            //    CMessageBox.Show("电流值不合法。\n Invalid current value");
            //    return;
            //}
            if (!double.TryParse(txtkv.Text, out kv))
            {
                CMessageBox.Show("电压值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtPower.Text, out power))
            {
                CMessageBox.Show("功率值不合法。\nInvalid voltage value");
                return;
            }
            if (!double.TryParse(txtTime.Text, out time))
            {
                CMessageBox.Show("曝光时间值不合法。\nInvalid spot time");
                return;
            }
            if (!int.TryParse(cboFps.Text, out fps))
            {
                CMessageBox.Show("帧速率值不合法。\n Invalid exposeFPS");
                return;
            }

            if (!CheckMaxPower())
                return;

            ExamParam param = new ExamParam();
            param.KV = kv;
            param.UA = ua;
            param.Power = power;
            param.Time = time;
            param.Fps = fps;

            Global.CurrentParam = param;
            var controlSystem = MainWindow.ControlSystem;

            controlSystem.SetKV(kv);
            System.Threading.Thread.Sleep(150);
            controlSystem.SetPower(power);
            if (StartAcqEvent != null)
            {
                StartAcqEvent.Invoke(this, null);
            }
        }
        private void AutoWL(object sender, RoutedEventArgs e)
        {
            if (AutoWLEvent != null)
                AutoWLEvent.Invoke(this, null);
        }
        public event RoutedEventHandler StartAcqEvent;
        public event RoutedEventHandler AutoWLEvent;


    }
}
