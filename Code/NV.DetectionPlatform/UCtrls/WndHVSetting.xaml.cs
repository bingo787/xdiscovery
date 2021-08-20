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
using NV.DRF.Core.Global;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndHVSetting : Window
    {
        public WndHVSetting()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int ua,count;
            double kv;
            if (!double.TryParse(txtKV.Text, out kv))
            {
                CMessageBox.Show("KV值不合法。");
            }
            if (!int.TryParse(txtUA.Text, out ua))
            {
                CMessageBox.Show("UA值不合法。");
            }
            if (!int.TryParse(txtCount.Text, out count))
            {
                CMessageBox.Show("级数值不合法。");
            }
            try
            {
                var win = Global.MainWindow as MainWindow;
                if (win!=null)
                {
                    win._examView.StartAcq(Service.ExamType.MultiEnergyAvg, true, count, kv, ua);
                }
            }
            catch (Exception ex)
            {
                CMessageBox.Show(ex.Message);
            }
        }
    }
}
