using NV.Infrastructure.UICommon;
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
using NV.DRF.Core.Global;
using NV.DRF.Controls;
using NV.DetectionPlatform.Entity;
using System.Collections.ObjectModel;
using NV.DRF.Core.Model;
using System.Reflection;
using System.IO;

namespace NV.DetectionPlatform.UCtrls
{
    /// <summary>
    /// ProductBrowser.xaml 的交互逻辑
    /// </summary>
    public partial class WndProductBrowser : Window
    {
        public WndProductBrowser()
        {
            InitializeComponent();
            this.DataContext = new BrowerViewModel(this);

        }
    }

    public class BrowerViewModel : ViewModelBase
    {

        #region 字段
        public DataGrid dgHistData;
        /// <summary>
        /// 双击元素是否为Datagrid中的RowDetail的ListBox
        /// </summary>
        private bool _isClickSourceListBox = false;
        private FrameworkElement _view;
        #endregion

        #region 属性
        private QueryInfo _condition = new QueryInfo();
        /// <summary>
        /// 查询条件
        /// </summary>
        public QueryInfo Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                Set(() => Condition, ref _condition, value);
            }
        }
        private Product _curProduct = new Product();
        /// <summary>
        /// 当前产品
        /// </summary>
        public Product CurrentProduct
        {
            get
            {
                return _curProduct;
            }
            set
            {
                Set(() => CurrentProduct, ref _curProduct, value);
                UpdateProductFolder();
                //Global.CurrentProduct = _curProduct;
            }
        }

        public ObservableCollection<PlatformFilesModel> PlatformModels
        { set; get; }

        private ObservableCollection<Product> _testRecords = new ObservableCollection<Product>();
        /// <summary>
        /// 产品列表
        /// </summary>
        public ObservableCollection<Product> TestRecords
        {
            get
            {
                return _testRecords;
            }
            set
            {
                Set(() => TestRecords, ref _testRecords, value);
            }
        }
        #endregion

        #region 命令

        /// <summary>
        /// load命令
        /// </summary>
        public ICommand CmdLoaded { set; get; }
        /// <summary>
        /// unload命令
        /// </summary>
        public ICommand CmdUnLoaded { set; get; }
        /// <summary>
        /// datequery命令
        /// </summary>
        public ICommand CmdDateQuery { set; get; }
        /// <summary>
        /// query命令
        /// </summary>
        public ICommand CmdQuery { set; get; }
        /// <summary>
        /// 双击行快速检查命令
        /// </summary>
        public ICommand CmdFastExam { set; get; }
        /// <summary>
        /// 检查命令
        /// </summary>
        public ICommand CmdExam { set; get; }
        /// <summary>
        /// 回放命令
        /// </summary>
        public ICommand CmdReview { set; get; }
        /// <summary>
        /// 删除命令
        /// </summary>
        public ICommand CmdDel { set; get; }
        /// <summary>
        /// 编辑病例命令
        /// </summary>
        public ICommand CmdEdit { set; get; }
        /// <summary>
        /// 保存命令
        /// </summary>
        public ICommand CmdSave { set; get; }
        /// <summary>
        /// 双击序列命令
        /// </summary>
        public ICommand CmdDoubleClickSeries { set; get; }
        /// <summary>
        /// 打印命令
        /// </summary>
        public ICommand CmdPrint { set; get; }
        /// <summary>
        /// 发送服务器命令
        /// </summary>
        public ICommand CmdSend { set; get; }
        /// <summary>
        /// 锁定\解除锁定命令
        /// </summary>
        public ICommand CmdLock { set; get; }
        /// <summary>
        /// 导入命令
        /// </summary>
        public ICommand CmdImport { set; get; }
        /// <summary>
        /// 导出命令
        /// </summary>
        public ICommand CmdExport { set; get; }
        /// <summary>
        /// 打开序列命令
        /// </summary>
        public ICommand CmdOpenSeries { set; get; }
        /// <summary>
        /// 更新序列命令
        /// </summary>
        public ICommand CmdUpdateSeries { set; get; }

        #endregion

        #region 构造函数
        public BrowerViewModel(FrameworkElement fe)
        {
            InitData(fe);
            InitCommand();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData(FrameworkElement fe)
        {
            _view = fe;
            dgHistData = (DataGrid)fe.FindName("dgHistData");
            Condition.DateFrom = DateTime.Now.Date;
            Condition.DateTo = DateTime.Now.Date;
            PlatformModels = new ObservableCollection<PlatformFilesModel>();
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitCommand()
        {
            CmdLoaded = new RelayCommand(ExecuteLoaded);
            CmdUnLoaded = new RelayCommand(ExecuteUnLoaded);
            CmdDateQuery = new RelayCommand<string>(ExecuteDateQuery);
            CmdQuery = new RelayCommand(ExecuteQuery);
            CmdFastExam = new RelayCommand(ExecuteFastExam);
            CmdExam = new RelayCommand(ExecuteExam);
            CmdDel = new RelayCommand<object>(ExecuteDel);
            CmdEdit = new RelayCommand<object>(ExecuteEdit);
            CmdSave = new RelayCommand<object>(ExecuteSave);
            CmdDoubleClickSeries = new RelayCommand(ExecuteDoubleClickSeries);
            CmdReview = new RelayCommand(ExecuteReview);
            CmdPrint = new RelayCommand<object>(ExecutePrint);
            CmdExport = new RelayCommand<object>(ExecuteExport);
            CmdOpenSeries = new RelayCommand<object>(ExecuteOpenSeries);
            CmdUpdateSeries = new RelayCommand<object>(ExecuteUpdateSeries);
        }



        #endregion

        #region 业务逻辑
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteExport(object obj)
        {
            if (CurrentProduct != null && !string.IsNullOrEmpty(CurrentProduct.ImageFolder))
            {
                if (!Directory.Exists(CurrentProduct.ImageFolder))
                {
                    CMessageBox.Show("该产品尚未进行检验.");
                    return;
                }
                System.Diagnostics.Process.Start(CurrentProduct.ImageFolder);
            }
        }
        /// <summary>
        /// load的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteLoaded()
        {
            LoadGridColumnWidth(dgHistData);

            ExecuteQuery();
            if (dgHistData.Items.Count > 0)
            {
                dgHistData.SelectedIndex = 0;
            }
            else
            {
            }
            dgHistData.IsEnabled = true;
        }

        /// <summary>
        /// unload的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteUnLoaded()
        {
            SaveGridColumnWidth(dgHistData);
        }
        /// <summary>
        /// datequery的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteDateQuery(string parameter)
        {
            switch (parameter)
            {
                case "today":
                    TodayWorklist();
                    break;
                case "week":
                    ThisWeekWorklist();
                    break;
                case "month":
                    ThisMonthWorklist();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置当天日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodayWorklist()
        {
            Condition.DateFrom = DateTime.Now;
            Condition.DateTo = DateTime.Now;

            ExecuteQuery();
        }

        /// <summary>
        /// 设置本周
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThisWeekWorklist()
        {
            int d = (int)DateTime.Now.DayOfWeek;
            d = d == (int)DayOfWeek.Sunday ? 7 : d;
            Condition.DateFrom = DateTime.Now.AddDays(1 - d);
            Condition.DateTo = Condition.DateFrom.AddDays(6);

            ExecuteQuery();
        }

        /// <summary>
        /// 设置本月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThisMonthWorklist()
        {
            Condition.DateFrom = DateTime.Now.AddDays(1 - DateTime.Now.Day);
            Condition.DateTo = DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.Day);

            ExecuteQuery();
        }
        /// <summary>
        /// query的执行方法
        /// </summary>
        private void ExecuteQuery()
        {
            try
            {
                using (Entities db = new Entities(Global.ConnectionString))
                {
                    string name, type, keywords, startDate, endDate;
                    name = Condition.Name;
                    type = Condition.Type;
                    keywords = Condition.Keywords;
                    startDate = Condition.DateFrom.ToString("yyyy-MM-dd");
                    endDate = Condition.DateTo.ToString("yyyy-MM-dd") + " 23:59:59";

                    IQueryable<Product> records = db.Product;
                    if (!string.IsNullOrEmpty(name))
                        records = records.Where(r => r.ProductName.Contains(name.Trim()));
                    if (!string.IsNullOrEmpty(type))
                        records = records.Where(r => r.ProductTypeID.Contains(type.Trim()));
                    if (!string.IsNullOrEmpty(keywords))
                        records = records.Where(r => r.ProductKeywords.Contains(keywords.Trim()));
                    if (!string.IsNullOrEmpty(startDate))
                        records = records.Where(r => (string.Compare(r.StartTime, startDate) == 1 || string.Compare(r.EndTime, startDate) == 1));
                    if (!string.IsNullOrEmpty(endDate))
                        records = records.Where(r => string.Compare(r.StartTime, endDate) == -1 || string.Compare(r.EndTime, endDate) == -1);

                    TestRecords = new ObservableCollection<Product>(records);
                }

                if (dgHistData.Items.Count > 0)
                {
                    dgHistData.SelectedIndex = 0;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                this.Error(ex, "历史数据-查询失败。");
                CMessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 双击行快速检查的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteFastExam()
        {
            DataGridRow curRow = typeof(DataGrid).InvokeMember("MouseOverRow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty, null, dgHistData, null) as DataGridRow;
            if (curRow == null)
                return;

            Product study = dgHistData.SelectedItem as Product;

            if (study == null) return;

            ExecuteExam();
        }
        /// <summary>
        /// 双击序列的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteDoubleClickSeries()
        {
            _isClickSourceListBox = true;
        }
        /// <summary>
        /// 检查的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteExam()
        {
            if (dgHistData.SelectedItem == null) return;

            Global.CurrentProduct = dgHistData.SelectedItem as Product;
            Global.MainWindow.UpdateProduct(dgHistData.SelectedItem as Product, false);
            this.Log("测试记录-开始测试，产品Name:" + Global.CurrentProduct.ProductName);
            if (_view != null && (_view as Window) != null)
            {
                try
                {
                    (_view as Window).DialogResult = true;
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 回放的执行方法
        /// </summary>
        private void ExecuteReview()
        {
            //if (dgHistData.SelectedItem == null) return;

            //Global.CurrentProduct = dgHistData.SelectedItem as Product;
            //this.Log("测试记录-开始测试，产品名称:" + Global.CurrentProduct.ProductName);
            //if (_view != null && (_view as Window) != null)
            //{
            //    (_view as Window).DialogResult = true;
            //}
        }

        /// <summary>
        /// 打印的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePrint(object parameter)
        {
            try
            {

            }
            catch (Exception ex)
            {
                this.Error(ex, "图像处理-进入打印界面失败。");
                CMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 根据配置设置DataGrid的列宽（xml反序列化）
        /// </summary>
        /// <param name="dg"></param>
        private void LoadGridColumnWidth(DataGrid dg)
        {
            string fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "cfg", dg.Name + ".xml");
            List<DataGridColumnInfo> dgciList = SerializeHelper.LoadFromFile<List<DataGridColumnInfo>>(fileName);

            for (int i = 0; i < dgciList.Count && i < dg.Columns.Count; i++)
            {
                dg.Columns[i].Width = new DataGridLength(dgciList[i].Width, DataGridLengthUnitType.Pixel);
                dg.Columns[i].Visibility = dgciList[i].Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// 保存DataGrid列宽到配置文件（xml序列化）
        /// </summary>
        /// <param name="dg"></param>
        private void SaveGridColumnWidth(DataGrid dg)
        {
            string fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "cfg", dg.Name + ".xml");
            List<DataGridColumnInfo> dgciList = new List<DataGridColumnInfo>();

            for (int i = 0; i < dg.Columns.Count; i++)
            {
                dgciList.Add(new DataGridColumnInfo()
                {
                    Width = (int)dg.Columns[i].ActualWidth,
                    Visible = dg.Columns[i].Visibility == System.Windows.Visibility.Visible
                });
            }

            SerializeHelper.SaveToFile(dgciList, fileName);
        }

        /// <summary>
        /// 删除的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteDel(object parameter)
        {
            try
            {
                if (dgHistData.SelectedItems == null || dgHistData.SelectedItems.Count == 0) return;

                List<Product> studyList = new List<Product>();
                foreach (Product stu in dgHistData.SelectedItems)
                {
                    studyList.Add(stu);
                }

                if (studyList != null)
                {
                    if (CMessageBox.Show("您确实要删除选定测试记录吗？\nAre you sure you want to delete the selected test records", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        using (Entities db = new Entities(Global.ConnectionString))
                        {
                            foreach (var study in studyList)
                            {
                                var item = db.Product.FirstOrDefault(ser => ser.GUID == study.GUID);
                                if (item != null)
                                {
                                    //文件服务器无法删除文件
                                    if (System.IO.Directory.Exists(item.ImageFolder))
                                    {
                                        System.IO.Directory.Delete(item.ImageFolder, true);
                                    }
                                    db.DeleteObject(item);
                                    this.Log("历史数据-删除测试记录成功，ID:" + study.ProductName);
                                    (dgHistData.ItemsSource as ObservableCollection<Product>).Remove(study);
                                }
                            }
                            db.SaveChanges();
                        }
                        if (dgHistData.Items.Count > 0)
                        {
                            dgHistData.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CMessageBox.Show("保存失败," + ex.Message);
                this.Error(ex);
            }
        }

        /// <summary>
        /// 编辑病例的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteEdit(object parameter)
        {
            try
            {
                Product newP = new Product();
                NativeMethods.CopyTo(CurrentProduct, newP);

                WndEditProduct edit = new WndEditProduct() { NewProduct = newP };
                if (edit.ShowDialogEx() == true)
                {
                    ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                this.Error(ex, "历史数据-编辑信息失败。");
                CMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 打开序列的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteOpenSeries(object parameter)
        {
            //Global.CurrentSeries = parameter as NV.DRF.DAL.Series;
            //Study study = dgHistData.SelectedItem as NV.DRF.DAL.Study;
            //Global.CurrentStudy = study;

            //if (study == null) return;

            //if (study.StudyStatus == "COMPLETED")
            //{
            //    ExecuteReview();
            //}
            //else
            //{
            //    ExecuteExam();
            //}
        }
        /// <summary>
        /// 打开序列的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteUpdateSeries(object parameter)
        {
            //Global.CurrentSeries = parameter as NV.DRF.DAL.Series;
        }

        /// <summary>
        /// 保存的执行方法
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteSave(object parameter)
        {
        }

        /// <summary>
        /// DataGrid列宽和是否可视类，用于保存和加载设置
        /// </summary>
        public class DataGridColumnInfo
        {
            public int Width { get; set; }
            public bool Visible { get; set; }
        }


        public void UpdateProductFolder(bool isClear = false)
        {
            try
            {
                PlatformModels.Clear();

                if (CurrentProduct == null || !Directory.Exists(CurrentProduct.ImageFolder))
                    return;

                string[] subDir = Directory.GetDirectories(CurrentProduct.ImageFolder);
                if (subDir.Length == 0)
                    return;

                foreach (var dir in subDir)
                {
                    string[] fils = Directory.GetFiles(dir, "*.dcm");
                    if (fils.Length == 0)
                        continue;

                    string png = string.Empty;
                    if (Directory.GetFiles(dir, "*.png").Length > 0)
                        png = Directory.GetFiles(dir, "*.png")[0];
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    PlatformFilesModel model = new PlatformFilesModel
                    {
                        Thumbnail = png,
                        DcmFiles = new ObservableCollection<string>(fils),
                        DateTime = dirInfo.Name,
                    };

                    PlatformModels.Add(model);
                }
            }
            catch { }

        }
        #endregion

    }
}
