using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NV.DetectionPlatform.Entity;
using NV.DRF.Core;
using NV.DRF.Core.Global;
using NV.DRF.Controls;
using NV.Infrastructure.UICommon;
using System.Collections.ObjectModel;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// UCProductRegister.xaml 的交互逻辑
    /// </summary>
    public partial class UCProductRegister : UserControl
    {
        public UCProductRegister()
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
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
            //同步当前产品重置产品登记信息
            NewProduct.GUID = System.Guid.NewGuid().ToString();
            NewProduct.ProductName = string.Empty;

            //using (Entities db = new Entities(Global.ConnectionString))
            //{
            //    Types.Clear();
            //    Specifications.Clear();
            //    foreach (var item in db.Product.OrderByDescending(p => p.StartTime))
            //    {
            //        if (!Types.Contains(item.ProductTypeID))
            //            Types.Add(item.ProductTypeID);
            //        if (Types.Count >= 10)
            //            break;
            //    }
            //    foreach (var item in db.Product.OrderByDescending(p => p.StartTime))
            //    {
            //        if (!Specifications.Contains(item.ProductSpecification))
            //            Specifications.Add(item.ProductSpecification);
            //        if (Specifications.Count >= 10)
            //            break;
            //    }

            //    if (Specifications.Count > 0)
            //    {
            //        NewProduct.ProductSpecification = Specifications[0];
            //    }
            //    if (Types.Count > 0)
            //    {
            //        NewProduct.ProductTypeID = Types[0];
            //    }
            //}
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
                CMessageBox.Show("请输入产品名称");
                return;
            }
            using (Entities db = new Entities(Global.ConnectionString))
            {
                var pro = db.Product.FirstOrDefault(p => p.ProductName == NewProduct.ProductName && p.ProductTypeID == NewProduct.ProductTypeID);
                if (pro == null)
                {
                    //登记产品
                    Product newP = new Product();
                    NativeMethods.CopyTo(NewProduct, newP);
                    newP.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    newP.ImageFolder = Global.CreateSavingImageDirctory(newP.GUID);
                    db.Product.AddObject(newP);

                    db.SaveChanges();
                    Global.CurrentProduct = newP;
                    Global.MainWindow.UpdateProduct(newP,true);
                    CMessageBox.Show("产品登记完毕。");
                }
                else
                {
                    if (CMessageBox.Show("产品名称已存在记录,是否合并该产品记录继续检查？", "询问", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Global.CurrentProduct = pro;
                        Global.MainWindow.UpdateProduct(pro,false);
                    }
                    else
                        return;
                }
            }

            UCProductRegister_Loaded(null, null);
        }

        private void txtProductName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterProduct(null, null);
            }
        }
    }
}
