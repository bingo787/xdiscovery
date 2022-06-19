using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InstallLibrary
{
    [RunInstaller(true)]
    public partial class RayonInstaller : System.Configuration.Install.Installer
    {
        public RayonInstaller()
        {
            InitializeComponent();
            this.BeforeInstall += new InstallEventHandler(RayonInstaller_BeforeInstall);
            this.AfterInstall += new InstallEventHandler(RayonInstaller_AfterInstall);
        }
        private void RayonInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {

            string path = this.Context.Parameters["targetdir"];//获取用户设定的安装目标路径, 注意，需要在Setup项目里面自定义操作的属性栏里面的CustomActionData添加上/targetdir="[TARGETDIR]\"

            // 1. install SQL Compat 
            // 2. install VC_redist
            List<string> cmds = new List<string>{
                   path + "\\Database\\VC_redist.x64.exe",
                   path + "\\Database\\SSCERuntime_x64-CHS.exe",

          };
            foreach (var command in cmds)
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        FileName = command,
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                p.StandardInput.WriteLine("exit");
                p.Close();
            }

          


        }
        private void RayonInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

            /*
             regsvr32 "%~dp0\imageview.dicom.dll"
             regsvr32 "%~dp0\imageview.dll"
             regsvr32 "%~dp0\report\reportdicom.dll"
             regsvr32 "%~dp0\report\reportview.dll"
             */

            string path = this.Context.Parameters["targetdir"];//获取用户设定的安装目标路径, 注意，需要在Setup项目里面自定义操作的属性栏里面的CustomActionData添加上/targetdir="[TARGETDIR]\"
            List<string> cmds =
            new List<string>{
                    path + "\\imageview.dicom.dll",
                    path + "\\imageview.dll",
                    path + "\\report\\reportdicom.dll",
                    path + "\\report\\reportview.dll"

            };
            foreach (var command in cmds)
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        FileName = "regsvr32.exe",
                        Arguments = "/s " + command,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                p.StandardInput.WriteLine("exit");
                p.Close();
            }
        }
    }
}
