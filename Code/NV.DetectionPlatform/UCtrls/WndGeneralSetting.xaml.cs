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
using NV.DRF.Core.Model;
using SerialPortController;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndGeneralSetting : Window
    {
        public WndGeneralSetting()
        {
            InitializeComponent();

            this.Loaded += WndGeneralSetting_Loaded;
        }

        void WndGeneralSetting_Loaded(object sender, RoutedEventArgs e)
        {
            txtImageFolder.Text = GeneralSettingHelper.Instance.FileDirectory;
            txtLutFile.Text = GeneralSettingHelper.Instance.LutFile;
            txtCompany.Text = GeneralSettingHelper.Instance.CompanyName;
            txtDepartment.Text = GeneralSettingHelper.Instance.Department;
            txtAddress.Text = GeneralSettingHelper.Instance.Address;
            txtHVName.Text = GeneralSettingHelper.Instance.HVName;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettingHelper.Instance.FileDirectory = txtImageFolder.Text;
            GeneralSettingHelper.Save();
            CMessageBox.Show("保存成功");
        }
        /// <summary>
        /// 选择存档目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectDirClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择图像存储目录";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtImageFolder.Text = dialog.SelectedPath;
            }
        }
        /// <summary>
        /// 校验应用伪彩文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLut_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckLut(txtLutFile.Text))
            {
                CMessageBox.Show("该Lut文件不符合规范，请重新选择");
                return;
            }

            GeneralSettingHelper.Instance.LutFile = txtLutFile.Text;
            GeneralSettingHelper.Save();
            CMessageBox.Show("保存成功");
        }
        /// <summary>
        /// 检查Lut文件规范
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool CheckLut(string p)
        {
            try
            {
                using (var fr = System.IO.File.Open(p, System.IO.FileMode.Open))
                {
                    if (fr.Length == 768)
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
        /// <summary>
        /// 配置伪彩文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFileClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "请选择Lut文件";
            dialog.InitialDirectory = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath,"Luts");
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtLutFile.Text = dialog.FileName;
            }
        }
        /// <summary>
        /// 保存公司信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCompanyInfo(object sender, RoutedEventArgs e)
        {
            GeneralSettingHelper.Instance.CompanyName = txtCompany.Text;
            GeneralSettingHelper.Instance.Department = txtDepartment.Text;
            GeneralSettingHelper.Instance.Address = txtAddress.Text;
            GeneralSettingHelper.Instance.HVName = txtHVName.Text;
            GeneralSettingHelper.Save();
            CMessageBox.Show("保存成功");
        }
    }
}
