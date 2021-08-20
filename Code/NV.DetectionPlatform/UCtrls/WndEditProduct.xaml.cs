using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NV.Infrastructure.UICommon;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// WndPortSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WndEditProduct : Window
    {
        public WndEditProduct()
        {
            this.Initialized += UCProductRegister_Initialized;
            this.Loaded += UCProductRegister_Loaded;

            InitializeComponent();
        }

        void UCProductRegister_Initialized(object sender, EventArgs e)
        {
            NewProduct = new Product();
            Specifications = new ObservableCollection<string>();
            Types = new ObservableCollection<string>();
            this.DataContext = this;
        }
        /// <summary>
        /// 当前产品
        /// </summary>
        public Product NewProduct { get; set; }
        /// <summary>
        /// 规格列表
        /// </summary>
        public ObservableCollection<string> Specifications { set; get; }
        /// <summary>
        /// 类型列表
        /// </summary>
        public ObservableCollection<string> Types { set; get; }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UCProductRegister_Loaded(object sender, RoutedEventArgs e)
        {
            //同步当前产品重置产品登记信息
            using (Entities db = new Entities(Global.ConnectionString))
            {
                Types.Clear();
                Specifications.Clear();
                foreach (var item in db.Product.OrderByDescending(p => p.StartTime))
                {
                    if (!Types.Contains(item.ProductTypeID))
                        Types.Add(item.ProductTypeID);
                    if (Types.Count >= 10)
                        break;
                }
                foreach (var item in db.Product.OrderByDescending(p => p.StartTime))
                {
                    if (!Specifications.Contains(item.ProductSpecification))
                        Specifications.Add(item.ProductSpecification);
                    if (Specifications.Count >= 10)
                        break;
                }
            }
        }
        /// <summary>
        /// 产品登记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterProduct(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewProduct.ProductName))
            {
                CMessageBox.Show("请输入产品名称\nPlease input product name first");
                return;
            }
            using (Entities db = new Entities(Global.ConnectionString))
            {
                Product pRepeat = db.Product.FirstOrDefault(p => p.GUID != NewProduct.GUID && p.ProductName == NewProduct.ProductName);
                if (pRepeat != null)
                {
                    CMessageBox.Show("已存在该名称的产品记录\nA product record with that name already exists");
                    return;
                }
                Product tp = db.Product.FirstOrDefault(p => p.GUID == NewProduct.GUID);
                if (tp != null)
                {
                    //登记产品
                    NativeMethods.CopyTo(NewProduct, tp);
                    db.SaveChanges();

                    CMessageBox.Show("修改成功。\nOperation completed");
                    DialogResult = true;
                }
                else
                {
                    CMessageBox.Show("修改失败。\nOperation failed");
                    return;
                }
            }
        }
    }
}
