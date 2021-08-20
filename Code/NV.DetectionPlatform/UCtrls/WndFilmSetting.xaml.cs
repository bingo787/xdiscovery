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
    public partial class WndFilmSetting : Window
    {
        public WndFilmSetting()
        {
            InitializeComponent();
            this.Loaded += WndFilmSetting_Loaded;
        }

        void WndFilmSetting_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                {
                    return;
                }

                using (NV.DetectionPlatform.Entity.Entities dbConfig = new Entities(Global.ConnectionString))
                {
                    dgMain.ItemsSource = dbConfig.Overlay.Where(o => o.IsUseful == true).ToList();
                }

                if (System.IO.File.Exists(_configFileName))
                {
                    ImageOverlay = SerializeHelper.LoadFromFile<ImageOverlay>(_configFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (ImageOverlay == null)
            {
                ImageOverlay = new ImageOverlay();
            }

            using (System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection())
            {
                cboFontName.Items.Clear();
                foreach (System.Drawing.FontFamily ff in ifc.Families)
                {
                    cboFontName.Items.Add(ff.Name);
                }
            }

            cboFontSize.Items.Clear();
            for (int i = 4; i < 64; i += 2)
            {
                cboFontSize.Items.Add(i.ToString());
            }

            DataContext = ImageOverlay;
        }

        private ImageOverlay ImageOverlay = null;
        private static string _configFileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "ImageView.xml");

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Overlay ov = new Overlay();

            if (!string.IsNullOrEmpty(txtText.Text))
            {
                ov.Type = "STATIC";
                ov.Description = txtText.Text.Trim();
                ov.DisplayFormat = txtText.Text.Trim();
                ov.TagElement = "";
                ov.TagGroup = "";
            }
            else
            {
                NV.DetectionPlatform.Entity.Overlay oovv = dgMain.SelectedItem as NV.DetectionPlatform.Entity.Overlay;

                ov.Type = "DYNAMIC";
                ov.Description = oovv.OverlayDesc;
                ov.DisplayFormat = txtFormat.Text.Trim();
                ov.TagGroup = oovv.TagGroup;
                ov.TagElement = oovv.TagElement;
            }

            string pos = (sender as Button).Tag as string;

            if (pos.CompareTo("TopLeft") == 0)
            {
                ImageOverlay.TopLeft.Add(ov);
            }
            else if (pos.CompareTo("TopRight") == 0)
            {
                ImageOverlay.TopRight.Add(ov);
            }
            else if (pos.CompareTo("BottomLeft") == 0)
            {
                ImageOverlay.BottomLeft.Add(ov);
            }
            else if (pos.CompareTo("BottomRight") == 0)
            {
                ImageOverlay.BottomRight.Add(ov);
            }
        }

        private void dgMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NV.DetectionPlatform.Entity.Overlay ov = dgMain.SelectedItem as NV.DetectionPlatform.Entity.Overlay;

            if (ov != null)
            {
                txtFormat.Text = ov.DisplayFormat;
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            string pos = (sender as Button).Tag as string;

            if (pos.CompareTo("TopLeft") == 0)
            {
                if (dgTopLeft.SelectedItem != null)
                {
                    ImageOverlay.TopLeft.Remove(dgTopLeft.SelectedItem as Overlay);
                }
            }
            else if (pos.CompareTo("TopRight") == 0)
            {
                if (dgTopRight.SelectedItem != null)
                {
                    ImageOverlay.TopRight.Remove(dgTopRight.SelectedItem as Overlay);
                }
            }
            else if (pos.CompareTo("BottomLeft") == 0)
            {
                if (dgBottomLeft.SelectedItem != null)
                {
                    ImageOverlay.BottomLeft.Remove(dgBottomLeft.SelectedItem as Overlay);
                }
            }
            else if (pos.CompareTo("BottomRight") == 0)
            {
                if (dgBottomRight.SelectedItem != null)
                {
                    ImageOverlay.BottomRight.Remove(dgBottomRight.SelectedItem as Overlay);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SerializeHelper.SaveToFile(ImageOverlay, _configFileName);
            this.Log("文本设置-修改成功.");

            CMessageBox.Show("保存成功.");
            Global.MainWindow.NotifyTip("Film", "Update");
            DialogResult = true;
        }
    }

    [System.Serializable]
    public class ImageOverlay
    {
        public ImageOverlay()
        {
            TopLeft = new ObservableCollection<Overlay>();
            TopRight = new ObservableCollection<Overlay>();
            BottomLeft = new ObservableCollection<Overlay>();
            BottomRight = new ObservableCollection<Overlay>();
        }

        public string FontName { get; set; }
        public int FontSize { get; set; }
        public string Resolution { get; set; }

        public ObservableCollection<Overlay> TopLeft { get; set; }
        public ObservableCollection<Overlay> TopRight { get; set; }
        public ObservableCollection<Overlay> BottomLeft { get; set; }
        public ObservableCollection<Overlay> BottomRight { get; set; }
    }

    [System.Serializable]
    public class Overlay
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string DisplayFormat { get; set; }
        public string TagGroup { get; set; }
        public string TagElement { get; set; }
    }
}
