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
    public partial class WndWLSetting : UserControl, INotifyPropertyChanged
    {
        public WndWLSetting()
        {
            InitializeComponent();

            this.DataContext = this;
            ImageParams = new System.Collections.ObjectModel.ObservableCollection<ImageParam>();
            CurrentParam = new ImageParam();
            this.Loaded += WndExamSetting_Loaded;
        }
        /// <summary>
        /// 属性
        /// </summary>
        private ImageParam _currentParam;
        /// <summary>
        /// 获取或设置属性
        /// </summary>
        public ImageParam CurrentParam
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
        public System.Collections.ObjectModel.ObservableCollection<ImageParam> ImageParams { set; get; }
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
                ImageParams.Clear();
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    db.ImageParam.ToList().ForEach(t => ImageParams.Add(t));
                }
                if (ImageParams.Count > 0)
                {
                    var selected = ImageParams.FirstOrDefault(t => t.GUID == oldGUID);
                    if (selected == null)
                        CurrentParam = ImageParams[0];
                    else
                    {
                        CurrentParam = selected;
                    }
                    NV.Config.SerializeHelper.SaveToFile(CurrentParam, System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config\\wlsetting.xaml"));
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
            int ww, wl;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\n Please input new solution name");
                return;
            }
            if (!int.TryParse(txtWW.Text, out ww))
            {
                CMessageBox.Show("WindowWidth值不合法。\n Invalid window width value");
                return;
            }
            if (!int.TryParse(txtWL.Text, out wl))
            {
                CMessageBox.Show("WindowLevel值不合法。\n Invalid window level value");
                return;
            }

            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.ImageParam.FirstOrDefault(para => para.Name == name);
                if (repeat != null)
                {
                    CMessageBox.Show("已存在该名称方案。\n The name already exists");
                    return;
                }
                else
                {
                    ImageParam param = new ImageParam();
                    param.GUID = System.Guid.NewGuid().ToString();
                    param.Name = name;
                    param.WindowLevel = wl;
                    param.WindowWidth = ww;
                    if (Global.CurrentProduct != null)
                    {
                        param.ProductType = Global.CurrentProduct.ProductTypeID;
                    }
                    db.AddToImageParam(param);
                    db.SaveChanges();
                    CMessageBox.Show("添加成功\n Operation completed");
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
            ImageParam p = lstParams.SelectedItem as ImageParam;
            if (CMessageBox.Show("确定要删除该方案吗？\n Are you sure you want to delete it?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            else
            {
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    var repeat = db.ImageParam.FirstOrDefault(para => para.GUID == p.GUID);
                    if (repeat != null)
                    {
                        db.ImageParam.DeleteObject(repeat);
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
            int ww, wl;

            if (!int.TryParse(txtWW.Text, out ww))
            {
                CMessageBox.Show("WindowWidth值不合法。\n Invalid window width value");
                return;
            }
            if (!int.TryParse(txtWL.Text, out wl))
            {
                CMessageBox.Show("WindowLevel值不合法。\nInvalid window level value");
                return;
            }
            ImageParam p = lstParams.SelectedItem as ImageParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.ImageParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;
                    repeat.WindowWidth = ww;
                    repeat.WindowLevel = wl;
                    if (Global.CurrentProduct != null)
                    {
                        repeat.ProductType = Global.CurrentProduct.ProductTypeID;
                    }
                    db.SaveChanges();
                    CMessageBox.Show("保存成功\n Operation completed");
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

        public delegate void WindowWLChanged(int ww, int wl);
        public event WindowWLChanged WindowWLChangedEvent;
        public delegate void WLSettedEventHandler(int ww, int wl);
        public event WLSettedEventHandler CloseSettingEvent;
        /// <summary>
        /// 关闭设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, RoutedEventArgs e)
        {
            if (CloseSettingEvent != null)
            {
                CloseSettingEvent.Invoke((int)CurrentParam.WindowWidth, (int)CurrentParam.WindowLevel);
            }
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetWWWL(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int ww = (int)sldrWW.Value;
                int wl = (int)sldrWL.Value;
                if (WindowWLChangedEvent != null)
                {
                    WindowWLChangedEvent(ww, wl);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
