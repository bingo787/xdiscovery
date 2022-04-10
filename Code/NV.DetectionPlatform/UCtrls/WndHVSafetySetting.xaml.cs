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
    public partial class WndHVSafetySetting : Window
    {
        public WndHVSafetySetting()
        {
            InitializeComponent();
            this.Loaded += WndHVSafetySetting_Loaded;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WndHVSafetySetting_Loaded(object sender, RoutedEventArgs e)
        {
            NV.Config.HVGeneratorParam para = NV.Config.HVGeneratorParam.Instance;

            txtKV.Text = para.MaxKV.ToString();
            //txtUA.Text = para.MaxCurrent.ToString();
            //txtPower.Text = para.MaxPower.ToString();
            txtExternSoft.Text = para.ExternSoft;

        }
        /// <summary>
        /// 应用设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                CMessageBox.Show("请输入安全密码。");
                return;
            }
            NV.Config.HVGeneratorParam para = NV.Config.HVGeneratorParam.Instance;
            if (string.IsNullOrEmpty(para.Pwd) || para.Pwd == txtPassword.Password)
            {
                int kv, ua, power;
                if (!int.TryParse(txtKV.Text, out kv))
                {
                    CMessageBox.Show("电压上限不合法");
                    return; ;
                }
                //if (!int.TryParse(txtUA.Text, out ua))
                //{
                //    CMessageBox.Show("电流上限不合法");
                //    return; ;
                //}
                //if (!int.TryParse(txtPower.Text, out power))
                //{
                //    CMessageBox.Show("功率上限不合法");
                //    return;
                //}
                para.MaxKV = kv;
                //para.MaxCurrent = ua;
                //para.MaxPower = power;
                para.Pwd = txtPassword.Password;
                para.ExternSoft = txtExternSoft.Text;

                para.Save();
                CMessageBox.Show("设置成功。");
                DialogResult = true;
            }
            else
            {
                CMessageBox.Show("密码错误。");
            }
        }
        /// <summary>
        /// 选择随机启动软件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetExternSoftPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog od = new System.Windows.Forms.OpenFileDialog();
            if (od.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                txtExternSoft.Text = od.FileName;
            }
        }
    }
}
