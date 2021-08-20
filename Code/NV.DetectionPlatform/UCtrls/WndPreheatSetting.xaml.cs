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

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndPreheatSetting : Window
    {
        public WndPreheatSetting()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public NV.Config.HVGeneratorParam Data
        {
            get
            {
                return NV.Config.HVGeneratorParam.Instance;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Data.Save();
            CMessageBox.Show("保存成功");
            DialogResult = true;
        }
    }
}
