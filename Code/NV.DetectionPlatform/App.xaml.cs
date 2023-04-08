using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using NV.DRF.Controls;
using NV.DRF.Core.Global;

namespace NV.DetectionPlatform
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Global.Error(e.Exception, "未经处理的异常");
                    MessageBox.Show("我们很抱歉，当前程序遇到一些问题，请联系管理员" + e.Exception.Message + e.Exception.ToString());
                    if (this.MainWindow == null || this.MainWindow.IsVisible != true)
                    {
                        //如果是启动过程中失败，自杀进程。
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                    e.Handled = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message+ex.ToString());
                }
            }));
        }
         protected override void OnStartup(StartupEventArgs e)
         {
             FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
             base.OnStartup(e);
          }
}
}
