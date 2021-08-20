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
using Detector;
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
            detector.IsMultiFramesOverlay = Data.IsMultiFramesOverlay;
            detector.IsMultiFramesOverlayByAvg = Data.IsMultiFramesOverlayByAvg;
            detector.MultiFramesOverlayNumber = Data.MultiFramesOverlayNumber;
            detector.NV_SetGain((NV_Gain)Data.Gain);
            detector.NV_SetExpTime(Data.ExpTime);
            detector.NV_SetMaxFrames(Data.MaxFrames);
            string autoOffset = Data.IsAutoPreOffset ? "1" : "0";
            NV.Infrastructure.UICommon.IniFile.WriteString("System", "AutoOffsetCalOnOpen", autoOffset, System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "nvDentalDet.ini"));
            detector.NV_SetBinningMode((NV_BinningMode)Data.BinningMode);
            detector.NV_SetShutterMode((NV_ShutterMode)Data.ShutterMode);
            detector.NV_SetAcquisitionMode((NV_AcquisitionMode)Data.AcquisitionMode);
            detector.NV_SetOffsetCal((NV_CorrType)Data.OffsetCorMode);
            detector.NV_SetGainCal((NV_CorrType)Data.GainCorMode);
            detector.NV_SetDefectCal((NV_CorrType)Data.DefectCorMode);
            detector.Delay = Data.Delay;
            detector.NV_SaveParamFile();

            CMessageBox.Show("操作成功。\nOperation completed");
        }
    }
}
