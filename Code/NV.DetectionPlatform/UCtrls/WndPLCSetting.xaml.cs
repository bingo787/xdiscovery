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
                    if (selected == null)
                        CurrentParam = PLCParams[0];
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

        public void InitPLC()
        {

            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var ps = db.PLCParam.ToList();
                if (ps.Count > 0)
                {
                    if (DicomViewer.Current != null)
                    {
                        //  设置USM的参数，做锐化
                        //  DicomViewer.Current.SetAOIParam((int)ps[0].UpperlimitofBubble, (int)ps[0].LowerlimitofBubble, (int)ps[0].PercentofBubblePass);
                        CurrentParam = ps[0];



                    }
                }
            }
        }


        private void SetPLC(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded)
                return;

            //System.Console.WriteLine(String.Format("SetUSM  ---------"));
            //if ( UsmParamChangedEvent != null)
            //{
            //    int radius = (int)sldrRadius.Value;
            //    int amount = (int)sldrAmount.Value;
            //    int threshold = (int)sldrThreshold.Value;
            //     System.Console.WriteLine(String.Format("SetUSM  {0},{1},{2}", amount,radius,threshold));
            //    UsmParamChangedEvent(amount,radius,threshold);

            //}

        }


        /// <summary>
        /// 添加方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            int radius, amount, threshold;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\nPlease input solution name");
                return;
            }
            if (!int.TryParse(txtRadius.Text, out radius))
            {
                CMessageBox.Show("半径值不合法。\nInvalid radius");
                return;
            }
            if (!int.TryParse(txtAmount.Text, out amount))
            {
                CMessageBox.Show("数量值不合法。\nInvolid amount");
                return;
            }
            if (!int.TryParse(txtThreshold.Text, out threshold))
            {
                CMessageBox.Show("阀值不合法。\nInvalid threshold");
                return;
            }
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
                    param.X = radius;
                    param.Y = amount;
                    param.Z = threshold;

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
            int raduis, amount, threshold;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\n\nPlease input solution name");
                return;
            }
            if (!int.TryParse(txtRadius.Text, out raduis))
            {
                CMessageBox.Show("半径值不合法。\nInvalid radius");
                return;
            }
            if (!int.TryParse(txtAmount.Text, out amount))
            {
                CMessageBox.Show("数量值不合法。\nInvolid amount");
                return;
            }
            if (!int.TryParse(txtThreshold.Text, out threshold))
            {
                CMessageBox.Show("阀值不合法。\nInvalid threshold");
                return;
            }
            PLCParam p = lstParams.SelectedItem as PLCParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.PLCParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;

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
        public delegate void PLCParamChanged(int amount, int radius, int threshold);
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
                //int radius = (int)sldrRadius.Value;
                //int amount = (int)sldrAmount.Value;
                //int threshold = (int)sldrThreshold.Value;
                //System.Console.WriteLine(String.Format("SetUSM  {0},{1},{2}", amount, radius, threshold));
               // PLCParamChangedEvent(amount, radius, threshold);

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
