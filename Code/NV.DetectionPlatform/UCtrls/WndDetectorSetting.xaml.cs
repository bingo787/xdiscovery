﻿using System;
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
using Detector;
using NV.Config;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndDetectorSetting : Window
    {
        public WndDetectorSetting()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public NV.Config.NV1313FPDSetting Data
        {
            get
            {
                return NV.Config.NV1313FPDSetting.Instance;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Data.Save();
            var detector = Detector.DetectorController.Instance;
            detector.Delay = Data.Delay;
            detector.ScaleRatio = Data.ScaleRatio;

            detector.SetUVCDeviceParameters((int)Data.ImageMode, 0, 0, 0);
            detector.GetUVCDeviceParameters();

            string autoOffset = Data.IsAutoPreOffset ? "1" : "0";
            NV.Infrastructure.UICommon.IniFile.WriteString("System", "AutoOffsetCalOnOpen", autoOffset, System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "nvDentalDet.ini"));

            CMessageBox.Show("操作成功。\nOperation completed");
        }
    }
}
