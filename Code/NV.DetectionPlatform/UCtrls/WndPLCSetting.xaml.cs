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
using NV.DRF.Core.Ctrl;
using OpenCvSharp;
using NV.Infrastructure.UICommon;
using SerialPortController;
using System.Threading;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndPLCSetting : System.Windows.Window, INotifyPropertyChanged
    {
        public WndPLCSetting()
        {
            InitializeComponent();

            this.DataContext = this;
            PLCParams = new System.Collections.ObjectModel.ObservableCollection<PLCParam>();
            CurrentParam = new PLCParam();
            this.Loaded += WndExamSetting_Loaded;
            this.PLCParamChangedEvent += WndPLCSetting_PLCParamChangedEvent;
        }

        private void WndPLCSetting_PLCParamChangedEvent(double x, double y, double z)
        {
            try
            {

                //  Y轴不动，Y的参数给到Z，让Z动

                PlcController.Instance.MoveX(x);
                Thread.Sleep(500);
                PlcController.Instance.MoveZ(y);
            }
            catch (Exception e) {
                CMessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// 属性
        /// </summary>
        private PLCParam _currentParam;
        /// <summary>
        /// 获取或设置属性
        /// </summary>


        public PLCParam CurrentParam
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
        /// 图像参数列表
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<PLCParam> PLCParams { set; get; }
        /// <summary>
        /// 初始化方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WndExamSetting_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldGUID = CurrentParam == null ? "null" : CurrentParam.GUID;

                PLCParams.Clear();
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    db.PLCParam.ToList().ForEach(t => PLCParams.Add(t));
                }

                if (PLCParams.Count > 0)
                {
                    var selected = PLCParams.FirstOrDefault(t => t.GUID == oldGUID);

                    if (selected == null) {
                        CurrentParam = PLCParams[0];
                    }
                    else
                    {
                        CurrentParam = selected;
                    }
                }

            }
            catch (Exception)
            {
            }
        }


  

        /// <summary>
        /// 添加方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            double x, y, z =0;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\nPlease input solution name");
                return;
            }
            if (!double.TryParse(txtRadius.Text, out x))
            {
                CMessageBox.Show("数据不合法。\nInvalid value");
                return;
            }
            if (!double.TryParse(txtAmount.Text, out y))
            {
                CMessageBox.Show("数据不合法。\nInvalid value");
                return;
            }
            //if (!double.TryParse(txtThreshold.Text, out z))
            //{
            //    CMessageBox.Show("数据不合法。\nInvalid value");
            //    return;
            //}
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.PLCParam.FirstOrDefault(para => para.Name == name);
                if (repeat != null)
                {
                    CMessageBox.Show("已存在该名称方案。\nThe name already exists");
                    return;
                }
                else
                {
                    PLCParam param = new PLCParam();
                    param.GUID = System.Guid.NewGuid().ToString();
                    param.Name = name;
                    param.X = x;
                    param.Y = y;
                    param.Z = z;

                    db.PLCParam.AddObject(param);
                    db.SaveChanges();
                    CMessageBox.Show("添加成功\nOperation completed");
                    CurrentParam = param;
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
            PLCParam p = lstParams.SelectedItem as PLCParam;
            if (CMessageBox.Show("确定要删除该方案吗？\nAre you sure you want to delete it?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            else
            {
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    var repeat = db.PLCParam.FirstOrDefault(para => para.GUID == p.GUID);
                    if (repeat != null)
                    {
                        db.PLCParam.DeleteObject(repeat);
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
            string name = txtName.Text;
            double x, y, z =0;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\n\nPlease input solution name");
                return;
            }
            if (!double.TryParse(txtRadius.Text, out x))
            {
                CMessageBox.Show("数据不合法。\nInvalid value");
                return;
            }
            if (!double.TryParse(txtAmount.Text, out y))
            {
                CMessageBox.Show("数据不合法。\nInvalid value");
                return;
            }
            //if (!double.TryParse(txtThreshold.Text, out z))
            //{
            //    CMessageBox.Show("数据不合法。\nInvalid value");
            //    return;
            //}
            PLCParam p = lstParams.SelectedItem as PLCParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.PLCParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;
                    repeat.X = x;
                    repeat.Y = y;
                    repeat.Z = z;
                    db.SaveChanges();
                    CMessageBox.Show("保存成功\nOperation completed");
                    WndExamSetting_Loaded(null, null);
                }
            }
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
        public delegate void PLCParamChanged(double x, double y, double z);
        public event PLCParamChanged PLCParamChangedEvent;

        public delegate void CloseEventHandler();
        public event CloseEventHandler CloseSettingEvent;

        public delegate void BackEventHandler();
        public event BackEventHandler BackSettingEvent;


 

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            if (PLCParamChangedEvent != null)
            {
                double x, y, z = 0;
                if (!double.TryParse(txtRadius.Text, out x))
                {
                    CMessageBox.Show("数据不合法。\nInvalid value");
                    return;
                }
                if (!double.TryParse(txtAmount.Text, out y))
                {
                    CMessageBox.Show("数据不合法。\nInvalid value");
                    return;
                }
                //if (!double.TryParse(txtThreshold.Text, out z))
                //{
                //    CMessageBox.Show("数据不合法。\nInvalid value");
                //    return;
                //}
                PLCParamChangedEvent(x, y, z);

            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            if (BackSettingEvent != null)
            {
                BackSettingEvent();

            }
        }
    }
}
