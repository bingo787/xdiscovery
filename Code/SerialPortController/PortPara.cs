using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SerialPortController.Setting
{
    /// <summary>
    /// 端口数据
    /// </summary>
    public class PortPara : INotifyPropertyChanged
    {

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


        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
