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
            detector.IsMultiFramesOverlay = Data.IsMultiFramesOverlay;
            detector.IsMultiFramesOverlayByAvg = Data.IsMultiFramesOverlayByAvg;
            detector.MultiFramesOverlayNumber = Data.MultiFramesOverlayNumber;
            detector.Delay = Data.Delay;
            

            string autoOffset = Data.IsAutoPreOffset ? "1" : "0";
            NV.Infrastructure.UICommon.IniFile.WriteString("System", "AutoOffsetCalOnOpen", autoOffset, System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "nvDentalDet.ini"));

            detector.HB_SetBinningMode((byte)Data.BinningMode);
            detector.HB_SetTriggerMode((int)Data.TriggerMode);
            detector.HB_SetGain((int)Data.Gain);
            detector.NV_SetOffsetCal((HB_OffsetCorrType)Data.OffsetCorMode);
            detector.NV_SetGainCal((HB_CorrType)Data.GainCorMode);
            detector.NV_SetDefectCal((HB_CorrType)Data.DefectCorMode);
            detector.HB_UpdateTriggerAndCorrectEnable((int)Data.TriggerMode);

            //detector.NV_SetShutterMode((NV_ShutterMode)Data.ShutterMode);
            //detector.HB_SetAcquisitionMode((int)Data.AcquisitionMode);
            //detector.HB_SetAqcSpanTime(Data.ExpTime);
            //detector.HB_SetMaxFrames(Data.MaxFrames);

            CMessageBox.Show("操作成功。\nOperation completed");
        }
    }
}
