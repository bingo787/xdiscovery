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

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndPortSetting : Window
    {
        public WndPortSetting()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// 本机端口列表
        /// </summary>
        public List<string> PortNames
        {
            get
            {
                return SerialPort.GetPortNames().ToList();
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        public NV.Config.PortPara Data
        {
            get
            {
                return NV.Config.PortPara.Instance;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            NV.Config.PortPara.Instance.Save();
            CMessageBox.Show("保存成功");
            try
            {
                var ControlSystem = MainWindow.ControlSystem;
                try
                {
                    ControlSystem.ClosePort();
                }
                catch { }
                ControlSystem.OpenSerialPort();
            }
            catch (Exception)
            {
                CMessageBox.Show("高压通讯串口打开失败，请检查串口设置");
            }
            DialogResult = true;
        }
    }
}
