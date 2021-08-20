using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NV.Config
{
    /// <summary>
    /// 安全设定
    /// </summary>
    public class HVGeneratorParam : NotifyModel
    {
        private static HVGeneratorParam _detector;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string _fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "HVGeneratorParam.xml");

        //私有变量
        /// <summary>
        /// 实例
        /// </summary>
        public static HVGeneratorParam Instance
        {
            get
            {
                if (_detector == null)
                {
                    Load();
                    return _detector;
                }

                return _detector;
            }
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void Load()
        {
            _detector = SerializeHelper.LoadFromFile<HVGeneratorParam>(_fileName);
        }
        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public void Save()
        {
            SerializeHelper.SaveToFile(_detector, _fileName);
        }


        private bool _isAutoPreheat = false;
        /// <summary>
        /// 积分时间
        /// </summary>
        [XmlElement(ElementName = "自动预热")]
        public bool IsAutoPreheat
        {
            get
            {
                return _isAutoPreheat;
            }
            set
            {
                Set(() => IsAutoPreheat, ref _isAutoPreheat, value);
            }
        }

        private double _preheatKV = 15;
        /// <summary>
        /// 预热电压
        /// </summary>
        [XmlElement(ElementName = "预热电压")]
        public double PreheatKV
        {
            get
            {
                return _preheatKV;
            }
            set
            {
                Set(() => PreheatKV, ref _preheatKV, value);
            }
        }
        private int _preheatCurrent = 200;
        /// <summary>
        /// 预热电流
        /// </summary>
        [XmlElement(ElementName = "预热电流")]
        public int PreheatCurrent
        {
            get
            {
                return _preheatCurrent;
            }
            set
            {
                Set(() => PreheatCurrent, ref _preheatCurrent, value);
            }
        }

        private int _preheatMinutes = 10;
        /// <summary>
        /// 预热时间/分钟
        /// </summary>
        [XmlElement(ElementName = "预热时间(分钟)")]
        public int PreheatMinutes
        {
            get
            {
                return _preheatMinutes;
            }
            set
            {
                Set(() => PreheatMinutes, ref _preheatMinutes, value);
            }
        }

        private double _maxKV = 160;
        /// <summary>
        /// 安全电压
        /// </summary>
        [XmlElement(ElementName = "电压上限")]
        public double MaxKV
        {
            get
            {
                return _maxKV;
            }
            set
            {
                Set(() => MaxKV, ref _maxKV, value);
            }
        }
        private int _maxCurrent = 1500;
        /// <summary>
        /// 安全电流
        /// </summary>
        [XmlElement(ElementName = "电流上限")]
        public int MaxCurrent
        {
            get
            {
                return _maxCurrent;
            }
            set
            {
                Set(() => MaxCurrent, ref _maxCurrent, value);
            }
        }
        private int _maxPower = 200;
        /// <summary>
        /// 安全功率
        /// </summary>
        [XmlElement(ElementName = "功率上限")]
        public int MaxPower
        {
            get
            {
                return _maxPower;
            }
            set
            {
                Set(() => MaxPower, ref _maxPower, value);
            }
        }
        private string _pwd;
        /// <summary>
        /// 密码
        /// </summary>
        [XmlElement(ElementName = "厂商验证码")]
        public string Pwd
        {
            get
            {
                return _pwd;
            }
            set
            {
                Set(() => Pwd, ref _pwd, value);
            }
        }

        private string _externSoft;
        /// <summary>
        /// 额外随启动软件
        /// </summary>
        [XmlElement(ElementName = "额外随启动软件")]
        public string ExternSoft
        {
            get
            {
                return _externSoft;
            }
            set
            {
                Set(() => ExternSoft, ref _externSoft, value);
            }
        }
    }
}
