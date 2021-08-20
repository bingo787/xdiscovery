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

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndAOISetting : Window, INotifyPropertyChanged
    {
        public WndAOISetting()
        {
            InitializeComponent();

            this.DataContext = this;
            AOIParams = new System.Collections.ObjectModel.ObservableCollection<AOIParam>();
            CurrentParam = new AOIParam();
            this.Loaded += WndExamSetting_Loaded;
        }

        /// <summary>
        /// 属性
        /// </summary>
        private AOIParam _currentParam;
        /// <summary>
        /// 获取或设置属性
        /// </summary>
        public AOIParam CurrentParam
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
        public System.Collections.ObjectModel.ObservableCollection<AOIParam> AOIParams { set; get; }
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
                AOIParams.Clear();
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    db.AOIParam.ToList().ForEach(t => AOIParams.Add(t));
                }
                if (AOIParams.Count > 0)
                {
                    var selected = AOIParams.FirstOrDefault(t => t.GUID == oldGUID);
                    if (selected == null)
                        CurrentParam = AOIParams[0];
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

        public static void InitAOI()
        {
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var ps = db.AOIParam.ToList();
                if (ps.Count > 0)
                {
                    if (DicomViewer.Current != null)
                    {
                        DicomViewer.Current.SetAOIParam((int)ps[0].UpperlimitofBubble, (int)ps[0].LowerlimitofBubble, (int)ps[0].PercentofBubblePass);
                        DicomViewer.Current.Invalidate();
                    }
                }
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
            int upper, lower, pass;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\nPlease input solution name");
                return;
            }
            if (!int.TryParse(txtUpper.Text, out upper))
            {
                CMessageBox.Show("上限值不合法。\nInvalid BubbleUpperlimit");
                return;
            }
            if (!int.TryParse(txtLower.Text, out lower))
            {
                CMessageBox.Show("下限值不合法。\nInvolid BubbleLowerlimit");
                return;
            }
            if (!int.TryParse(txtPass.Text, out pass))
            {
                CMessageBox.Show("合格率值不合法。\nInvalid BubbleQualification");
                return;
            }
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.AOIParam.FirstOrDefault(para => para.Name == name);
                if (repeat != null)
                {
                    CMessageBox.Show("已存在该名称方案。\nThe name already exists");
                    return;
                }
                else
                {
                    AOIParam param = new AOIParam();
                    param.GUID = System.Guid.NewGuid().ToString();
                    param.Name = name;
                    param.UpperlimitofBubble = upper;
                    param.LowerlimitofBubble = lower;
                    param.PercentofBubblePass = pass;

                    db.AOIParam.AddObject(param);
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
            AOIParam p = lstParams.SelectedItem as AOIParam;
            if (CMessageBox.Show("确定要删除该方案吗？\nAre you sure you want to delete it?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            else
            {
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    var repeat = db.AOIParam.FirstOrDefault(para => para.GUID == p.GUID);
                    if (repeat != null)
                    {
                        db.AOIParam.DeleteObject(repeat);
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
            int upper, lower, pass;
            if (string.IsNullOrEmpty(name))
            {
                CMessageBox.Show("请输入新方案名称\n\nPlease input solution name");
                return;
            }
            if (!int.TryParse(txtUpper.Text, out upper))
            {
                CMessageBox.Show("上限值不合法。\nInvalid BubbleUpperlimit");
                return;
            }
            if (!int.TryParse(txtLower.Text, out lower))
            {
                CMessageBox.Show("下限值不合法。\nInvolid BubbleLowerlimit");
                return;
            }
            if (!int.TryParse(txtPass.Text, out pass))
            {
                CMessageBox.Show("合格率值不合法。\nInvalid BubbleQualification");
                return;
            }
            AOIParam p = lstParams.SelectedItem as AOIParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.AOIParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;
                    repeat.UpperlimitofBubble = upper;
                    repeat.LowerlimitofBubble = lower;
                    repeat.PercentofBubblePass = pass;
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

        public delegate void CloseEventHandler();
        public event CloseEventHandler CloseSettingEvent;
        ///// <summary>
        ///// 关闭设置
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Close(object sender, RoutedEventArgs e)
        //{
        //    if (CloseSettingEvent != null)
        //    {
        //        CloseSettingEvent.Invoke((int)CurrentParam.WindowWidth, (int)CurrentParam.WindowLevel);
        //    }
        //}
        ///// <summary>
        ///// 设置参数
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void SetWWWL(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    try
        //    {
        //        int ww = (int)sldrWW.Value;
        //        int wl = (int)sldrWL.Value;
        //        if (WindowWLChangedEvent != null)
        //        {
        //            WindowWLChangedEvent(ww, wl);
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        private void SetAOI(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded)
                return;
            int upper = (int)sldrAOIUpper.Value;
            int lower = (int)sldrAOILower.Value;
            int pass = (int)sldrAOIPass.Value;

            if (DicomViewer.Current != null)
            {
                DicomViewer.Current.SetAOIParam(upper, lower, pass);
                DicomViewer.Current.Invalidate();
            }

        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
