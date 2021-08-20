using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NV.Config
{
    /// <summary>
    /// 端口数据
    /// </summary>
    public class PortPara : NotifyModel
    {
        private static PortPara _portParam;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string _fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ControllerPortParam.xml");

        //私有变量
        /// <summary>
        /// 实例
        /// </summary>
        public static PortPara Instance
        {
            get
            {
                if (_portParam == null)
                {
                    Load();
                    return _portParam;
                }

                return _portParam;
            }
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void Load()
        {
            _portParam = SerializeHelper.LoadFromFile<PortPara>(_fileName);
        }
        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public void Save()
        {
            SerializeHelper.SaveToFile(_portParam, _fileName);
        }
        /// <summary>
        /// 串口名称
        /// </summary>
        private string _portName = "COM1";
        [XmlElement(ElementName = "串口名称")]
        public string PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                if (_portName == value)
                {
                    return;
                }
                _portName = value;
                RaisePropertyChanged("PortIndex");
            }
        }


        private int _baudRate = 9600;
        [XmlElement(ElementName = "波特率")]
        public int BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                _baudRate = value;
                RaisePropertyChanged("BaudRate");
            }
        }
        private int _dataBits = 8;
        [XmlElement(ElementName = "数据位")]
        public int DataBits
        {
            get
            {
                return _dataBits;
            }
            set
            {
                _dataBits = value;
                RaisePropertyChanged("DataBits");
            }
        }
        private System.IO.Ports.Parity _parity = Parity.None;
        [XmlElement(ElementName = "奇偶校验")]
        public Parity Parity
        {
            get
            {
                return _parity;
            }
            set
            {
                _parity = value;
                RaisePropertyChanged("Parity");
            }
        }
        private StopBits _stopBits = StopBits.One;
        [XmlElement(ElementName = "停止位")]
        public StopBits StopBits
        {
            get
            {
                return _stopBits;
            }
            set
            {
                _stopBits = value;
                RaisePropertyChanged("StopBits ");
            }
        }

        private List<Parity> _parities = new List<Parity>()
        {   
                    Parity.Even,
                    Parity.Mark,
                    Parity.None,
                    Parity.Odd,
                    Parity.Space,
        };
        [XmlIgnoreAttribute]
        public List<Parity> Parities
        {
            get
            {
                return _parities;
            }
            set
            {
                _parities = value;
            }
        }
        //private Parity[] _parities = new Parity[]
        //{   
        //            Parity.Even,
        //            Parity.Mark,
        //            Parity.None,
        //            Parity.Odd,
        //            Parity.Space,
        //};
        //[XmlElement(ElementName = "奇偶校验位列表")]
        //public Parity[] Parities
        //{
        //    get
        //    {
        //        return _parities;
        //    }
        //    set
        //    {
        //        _parities = value;
        //    }
        //}
        private List<StopBits> _stopBitses = new List<StopBits>()
        {
                    StopBits.None,
                    StopBits.One,
                    StopBits.OnePointFive,
                    StopBits.Two,
        };
        [XmlIgnoreAttribute]
        public List<StopBits> StopBitses
        {
            get
            {
                return _stopBitses;
            }
            set
            {
                _stopBitses = value;
            }
        }
        //private StopBits[] _stopBitses = new StopBits[]
        //{
        //            StopBits.None,
        //            StopBits.One,
        //            StopBits.OnePointFive,
        //            StopBits.Two,
        //};
        //[XmlElement(ElementName = "停止位列表")]
        //public StopBits[] StopBitses
        //{
        //    get
        //    {
        //        return _stopBitses;
        //    }
        //    set
        //    {
        //        _stopBitses = value;
        //    }
        //}
    }
}
