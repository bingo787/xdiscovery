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

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndUSMSetting : System.Windows.Window, INotifyPropertyChanged
    {
        public WndUSMSetting()
        {
            InitializeComponent();

            this.DataContext = this;
            USMParams = new System.Collections.ObjectModel.ObservableCollection<USMParam>();
            CurrentParam = new USMParam();
            this.Loaded += WndExamSetting_Loaded;
        }

        /// <summary>
        /// 属性
        /// </summary>
        private USMParam _currentParam;
        /// <summary>
        /// 获取或设置属性
        /// </summary>
        public USMParam CurrentParam
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
        public System.Collections.ObjectModel.ObservableCollection<USMParam> USMParams { set; get; }
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
                USMParams.Clear();
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    db.USMParam.ToList().ForEach(t => USMParams.Add(t));
                }
                if (USMParams.Count > 0)
                {
                    var selected = USMParams.FirstOrDefault(t => t.GUID == oldGUID);
                    if (selected == null)
                        CurrentParam = USMParams[0];
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

        public static void InitUSM()
        {
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var ps = db.USMParam.ToList();
                if (ps.Count > 0)
                {
                    //if (DicomViewer.Current != null)
                    //{
                    //    //  设置USM的参数，做锐化
                    //    //  DicomViewer.Current.SetAOIParam((int)ps[0].UpperlimitofBubble, (int)ps[0].LowerlimitofBubble, (int)ps[0].PercentofBubblePass);
                    //    int radius = (int)ps[0].Radius;
                    //    int amount = (int)ps[0].Amount;
                    //    int threshold = (int)ps[0].Threshold;
                    //    if (DicomViewer.Current != null)
                    //    {
                    //        UnsharpenMaskByCurrentParam();
                    //        // DicomViewer.Current.SetAOIParam(();
                    //        // DicomViewer.Current.Invalidate();
                    //        DicomViewer.Current.Invalidate();
                    //    }
                    //}
                }
            }
        }


        public static void UnsharpenMaskByCurrentParam(ref NV.DRF.Core.Ctrl.Film film, USMParam param) {

            
            //1 读取照片数据
            film.CurrentDv.GetImageSize(out ushort width, out ushort height, out ushort bits, ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL);
            ushort[] data = new ushort[width * height];
            film.CurrentDv.GetImageData(out width, out height, out bits, out data[0], ImageViewLib.tagGET_IMAGE_FLAG.GIF_ALL, false);
            Mat src = new Mat(width, height, MatType.CV_16U, data);

          //  CMessageBox.Show("src size " + src.Size().ToString());

            int amount = (int)param.Amount;
            int radius = (int)param.Radius;
            int threshold = (int)param.Threshold;


            if (radius % 2 == 0) {
                radius += 1;
            }
           string p =  (string.Format("amount:{0} radius:{1} threshold:{2}", amount, radius, threshold));
           CMessageBox.Show(p);



            //2 高斯模糊

            Mat usm = new Mat();// (width, height, MatType.CV_16U);
            Mat blured = new Mat();// (width, height, MatType.CV_16U);
            Cv2.GaussianBlur(src, blured, new OpenCvSharp.Size(radius, radius), 0, 0);

            //CMessageBox.Show("blured size " + blured.Size().ToString());

            double fac = (double)threshold / 100;
            Cv2.AddWeighted(src, fac, blured, 1-fac, 0, usm);

           // CMessageBox.Show("usm size " + usm.Size().ToString() + "  height " + usm.Height);
            int w = usm.Size().Width;
            int h = usm.Size().Height;

            ushort[] result = new ushort[w * h];

            for (int i = 0; i < usm.Rows; i++) {
                for (int j = 0; j < usm.Cols; j++) {
                       result[i * usm.Cols + j] = usm.At<ushort>(i, j);
                }
            }

            film.PutData((ushort)w,(ushort)h,16, result, true);

           // Cv2.ImShow("USM", imgdst);
        }

        public static void UnsharpenMaskByDatabaseParam(ref NV.DRF.Core.Ctrl.Film film) {


            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {

                var ps = db.USMParam.ToList();
                if (ps.Count > 0)
                {
                    if (DicomViewer.Current != null)
                    {
                        UnsharpenMaskByCurrentParam(ref film ,ps[0]);
                        // DicomViewer.Current.SetAOIParam(();
                       // DicomViewer.Current.Invalidate();
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
                var repeat = db.USMParam.FirstOrDefault(para => para.Name == name);
                if (repeat != null)
                {
                    CMessageBox.Show("已存在该名称方案。\nThe name already exists");
                    return;
                }
                else
                {
                    USMParam param = new USMParam();
                    param.GUID = System.Guid.NewGuid().ToString();
                    param.Name = name;
                    param.Radius = radius;
                    param.Amount = amount;
                    param.Threshold = threshold;

                    db.USMParam.AddObject(param);
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
            USMParam p = lstParams.SelectedItem as USMParam;
            if (CMessageBox.Show("确定要删除该方案吗？\nAre you sure you want to delete it?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            else
            {
                using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
                {
                    var repeat = db.USMParam.FirstOrDefault(para => para.GUID == p.GUID);
                    if (repeat != null)
                    {
                        db.USMParam.DeleteObject(repeat);
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
            USMParam p = lstParams.SelectedItem as USMParam;
            using (NV.DetectionPlatform.Entity.Entities db = new Entity.Entities(NV.DRF.Core.Global.Global.ConnectionString))
            {
                var repeat = db.USMParam.FirstOrDefault(para => para.GUID == p.GUID);
                if (repeat != null)
                {
                    repeat.Name = txtName.Text;
                    repeat.Radius = raduis;
                    repeat.Amount = amount;
                    repeat.Threshold = threshold;
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


        private void SetUSM(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded)
                return;
            int radius = (int)sldrRadius.Value;
            int amount = (int)sldrAmount.Value;
            int threshold = (int)sldrThreshold.Value;

            if (DicomViewer.Current != null)
            {

             //  UnsharpenMaskByCurrentParam(radius,amount,threshold);
             //   DicomViewer.Current
               // DicomViewer.Current.SetAOIParam(radius, amount, threshold);
              //  DicomViewer.Current.Invalidate();
            }

        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
