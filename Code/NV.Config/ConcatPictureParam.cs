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
    public class ConcatPictureParam : NotifyModel
    {
        private static ConcatPictureParam _catParam;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string _fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ConcatPictureParam.xml");

        //私有变量
        /// <summary>
        /// 实例
        /// </summary>
        public static ConcatPictureParam Instance
        {
            get
            {
                if (_catParam == null)
                {
                    Load();
                    return _catParam;
                }

                return _catParam;
            }
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void Load()
        {
            _catParam = SerializeHelper.LoadFromFile<ConcatPictureParam>(_fileName);
        }
        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public void Save()
        {
            SerializeHelper.SaveToFile(_catParam, _fileName);
        }
        /// <summary>
        /// LEFT_1.X
        /// </summary>
        private int _leftPicAPointX = 720;
        [XmlElement(ElementName = "左图A点坐标X")]
        public int LeftPicAX
        {
            get
            {
                return _leftPicAPointX;
            }
            set
            {
                if (_leftPicAPointX == value)
                {
                    return;
                }
                _leftPicAPointX = value;
            }
        }


        private int _leftPicAPointY = 0;
        [XmlElement(ElementName = "左图A点坐标Y")]
        public int LeftPicAY
        {
            get
            {
                return _leftPicAPointY;
            }
            set
            {
                _leftPicAPointY = value;
            }
        }

        private int _leftPicBPointX = 1470;
        [XmlElement(ElementName = "左图B点坐标X")]
        public int LeftPicBX
        {
            get
            {
                return _leftPicBPointX;
            }
            set
            {
                if (_leftPicBPointX == value)
                {
                    return;
                }
                _leftPicBPointX = value;
            }
        }


        private int _leftPicBPointY = 0;
        [XmlElement(ElementName = "左图B点坐标Y")]
        public int LeftPicBY
        {
            get
            {
                return _leftPicBPointY;
            }
            set
            {
                _leftPicBPointY = value;
            }
        }


        ////////////////////////////////// 右图 //////////////////////
        private int _rightPicAPointX = 1680;
        [XmlElement(ElementName = "右图A点坐标X")]
        public int RightPicAX
        {
            get
            {
                return _rightPicAPointX;
            }
            set
            {
                if (_rightPicAPointX == value)
                {
                    return;
                }
                _rightPicAPointX = value;
            }
        }


        private int _rightPicAPointY = 0;
        [XmlElement(ElementName = "右图A点坐标Y")]
        public int RightPicAY
        {
            get
            {
                return _rightPicAPointY;
            }
            set
            {
                _rightPicAPointY = value;
            }
        }

        private int _rightPicBPointX = 2380;
        [XmlElement(ElementName = "右图B点坐标X")]
        public int RightPicBX
        {
            get
            {
                return _rightPicBPointX;
            }
            set
            {
                if (_rightPicBPointX == value)
                {
                    return;
                }
                _rightPicBPointX = value;
            }
        }


        private int _rightPicBPointY = 0;
        [XmlElement(ElementName = "右图B点坐标Y")]
        public int RightPicBY
        {
            get
            {
                return _rightPicBPointY;
            }
            set
            {
                _rightPicBPointY = value;
            }
        }
    }
}
