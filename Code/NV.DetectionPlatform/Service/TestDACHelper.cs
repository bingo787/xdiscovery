using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/* nvDentalDet.ini配置文件实例
[AcquisitionControl]
AcquisitionMode=0
ShutterMode=0
BinningMode=0
[WorkControl]
OffsetX=0
OffsetY=0
Width=1280
Height=1280
MaxFrames=0
ExpTime=168 	；默认168，wifi模式下改为2610
Gain=2
[ImageProcessing]
OffsetCalType=0
GainCalType=0
DefectCalType=0
MirrorXType=0
MirrorYType=0
InversionType=0
DigitalGammaType=0
DigitalLUTType=0
ImageOffset=100

[System]
AutoOffsetCalOnOpen=0    ;开机自动offset配置，1自动进行，0无

[ApplicationMode1]
Binning=1
Gain=0.1pF
TestDAC=1590

[ApplicationMode2]
Binning=1
Gain=0.4pF
TestDAC=1100

[ApplicationMode3]
Binning=1
Gain=0.7pF
TestDAC=1024

[ApplicationMode4]
Binning=1
Gain=1.0pF
TestDAC=1000

[ApplicationMode5]
Binning=2
Gain=0.1pF
TestDAC=2080

[ApplicationMode6]
Binning=2
Gain=0.4pF
TestDAC=1005

[ApplicationMode7]
Binning=2
Gain=0.7pF
TestDAC=850

[ApplicationMode8]
Binning=2
Gain=1.0pF
TestDAC=780

[DisplayMode]
DebugMode=1

[ChannelCal]
Enabled=0
ChannelCalOffset=200
ChannelWidth=80
TestDACCode1=1024
TestDACCode2=3000
TestDACCode3=6000
TestDACCode4=9000
TestDACCode5=12000

[Language]
LanguageMode =1
 * */

namespace ExamModule.Service
{

    /// <summary>
    /// ID:TestDAC设置辅助类
    /// Describe:用于获取、设置配置文件中的DAC值来完成DAC配置
    /// Author:ybx
    /// Date:2018-12-27 15:11:37
    /// </summary>
    public static class TestDACHelper
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string configFilePath = System.Environment.CurrentDirectory + "\\nvDentalDet.ini";
        /// <summary>
        /// 获取所有DAC配置
        /// </summary>
        /// <returns></returns>
        public static List<DACInfo> GetDACInfoList()
        {
            List<DACInfo> list = new List<DACInfo>();
            for (int i = 0; i < 16; i++)
            {
                string section = "ApplicationMode" + i.ToString();

                DACInfo info = new DACInfo();
                info.ApplicationMode = section;
                info.TestDAC = NV.Infrastructure.UICommon.IniFile.ReadString(section, "TestDAC", configFilePath);
                info.Binning = NV.Infrastructure.UICommon.IniFile.ReadString(section, "Binning", configFilePath);
                info.Gain = NV.Infrastructure.UICommon.IniFile.ReadString(section, "Gain", configFilePath);

                if (string.IsNullOrEmpty(info.Gain) || string.IsNullOrEmpty(info.Binning))
                {
                    continue;
                }
                list.Add(info);
            }

            return list;
        }
        /// <summary>
        /// 设置DAC
        /// </summary>
        /// <param name="dac"></param>
        public static void SaveDACInfo(DACInfo dac)
        {
            NV.Infrastructure.UICommon.IniFile.WriteString(dac.ApplicationMode, "TestDAC", dac.TestDAC, configFilePath);
            NV.Infrastructure.UICommon.IniFile.WriteString(dac.ApplicationMode, "Binning", dac.Binning, configFilePath);
            NV.Infrastructure.UICommon.IniFile.WriteString(dac.ApplicationMode, "Gain", dac.Gain, configFilePath);
        }
    }

    /// <summary>
    /// DAC配置类
    /// </summary>
    public class DACInfo : NV.Infrastructure.UICommon.ObservableModel
    {
        private string _applicationMode;
        /// <summary>
        /// 程序模式
        /// </summary>
        public string ApplicationMode
        {
            get
            {
                return _applicationMode;
            }
            set
            {
                Set(() => ApplicationMode, ref _applicationMode, value);
            }
        }
        private string _binning;
        /// <summary>
        /// Binning
        /// </summary>
        public string Binning
        {
            get
            {
                return _binning;
            }
            set
            {
                Set(() => Binning, ref _binning, value);
            }
        }
        private string _gain;
        /// <summary>
        /// 增益
        /// </summary>
        public string Gain
        {
            get
            {
                return _gain;
            }
            set
            {
                Set(() => Gain, ref _gain, value);
            }
        }
        private string _testDAC;
        /// <summary>
        /// DAC值
        /// </summary>
        public string TestDAC
        {
            get
            {
                return _testDAC;
            }
            set
            {
                Set(() => TestDAC, ref _testDAC, value);
            }
        }
      
    }
}
