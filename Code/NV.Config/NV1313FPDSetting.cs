using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NV.Config
{


    public enum LIONUVC_XRAY_TYPE
    {
        VTC_D =0 ,
        VTC_A = 1,
    }

    public enum LIONUVC_MODEL_FILTER {

        FILTER = 0,
        NO_FILTER 
    }



    public enum LIONUVC_BinningMode
    {
        NO_BINNING = 0,             ///< 2x2binning
        BINNING = 1,				///< 1x1binning(默认值)

    }



    public enum LIONUVC_IMAGE_MODE {
        AC = 0,       // AC 模式出图
        VTC = 1         // VTC出图

    }

    public enum HB_OffsetCorrType
    {
        NO = 0,				///< 不应用校正使能
        SOFTWARE_PRE_OFFSET = 1,			///< 应用硬件校正使能
        FIRMWARE_PRE_OFFSET = 2,			///< 应用软件校正使能
        FIRMWARE_POST_OFFSET = 3
    }

    public enum HB_CorrType
    {

        NO = 0,				///< 不应用校正使能
        SOFTWARE = 1,			///< 应用硬件校正使能
        FRIMWARE = 2			///< 应用软件校正使能
    }

 

    /// <summary>
    /// ID:NV1313探测器设置
    /// Describe:保存用户对NV1313探测器设置信息
    /// Author:ybx
    /// Date:2019-1-23 16:47:42
    /// </summary>
    public class NV1313FPDSetting : NotifyModel
    {
        private static NV1313FPDSetting _detector;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string _fileName = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "NV1313FPDSettingParam.xml");

        //私有变量
        /// <summary>
        /// 实例
        /// </summary>
        public static NV1313FPDSetting Instance
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
            _detector = SerializeHelper.LoadFromFile<NV1313FPDSetting>(_fileName);
        }
        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public void Save()
        {
            SerializeHelper.SaveToFile(_detector, _fileName);
        }



        private bool _isAutoPreOffset = true;
        /// <summary>
        /// 开机自动校正
        /// </summary>
        [XmlElement(ElementName = "自动校正")]
        public bool IsAutoPreOffset
        {
            get
            {
                return _isAutoPreOffset;
            }
            set
            {
                Set(() => IsAutoPreOffset, ref _isAutoPreOffset, value);
            }
        }

        private LIONUVC_IMAGE_MODE _image_mode = LIONUVC_IMAGE_MODE.AC;
        /// <summary>
        /// 出图模式
        /// </summary>
        [XmlElement(ElementName = "出图模式")]
        public LIONUVC_IMAGE_MODE ImageMode
        {
            get
            {
                return _image_mode;
            }
            set
            {
                Set(() => ImageMode, ref _image_mode, value);
            }
        }

        private ObservableCollection<LIONUVC_IMAGE_MODE> _image_modes = new ObservableCollection<LIONUVC_IMAGE_MODE>()
        {
            LIONUVC_IMAGE_MODE.AC,       // [1]-0.6pC
            LIONUVC_IMAGE_MODE.VTC         // [2]-1.2pC
        };
        /// <summary>
        /// 出图模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<LIONUVC_IMAGE_MODE> ImageModes
        {
            get
            {
                return _image_modes;
            }
            set
            {
                _image_modes = value;
            }
        }

        private double _scaleRatio = 0.1;
        /// <summary>
        /// 尺度比例
        /// </summary>
        [XmlElement(ElementName = "尺度比例")]
        public double ScaleRatio
        {
            get
            {
                return _scaleRatio;
            }
            set
            {
                Set(() => ScaleRatio, ref _scaleRatio, value);
            }
        }
        private int _delay = 1000;
        /// <summary>
        /// 延迟采集时间
        /// </summary>
        [XmlElement(ElementName = "延时采集时间ms")]
        public int Delay
        {
            get
            {
                return _delay;
            }
            set
            {
                Set(() => Delay, ref _delay, value);
            }
        }
        private int _maxFrames = 10;
        /// <summary>
        /// 最大帧
        /// </summary>
        [XmlElement(ElementName = "最大帧")]
        public int MaxFrames
        {
            get
            {
                return _maxFrames;
            }
            set
            {
                Set(() => MaxFrames, ref _maxFrames, value);
            }
        }


        private LIONUVC_BinningMode _binningMode = LIONUVC_BinningMode.BINNING;
        /// <summary>
        /// binning模式
        /// </summary>
        [XmlElement(ElementName = "Binning模式")]
        public LIONUVC_BinningMode BinningMode
        {
            get
            {
                return _binningMode;
            }
            set
            {
                Set(() => BinningMode, ref _binningMode, value);
            }
        }
        private ObservableCollection<LIONUVC_BinningMode> _binningModes = new ObservableCollection<LIONUVC_BinningMode>()
        {
            LIONUVC_BinningMode.BINNING,
            LIONUVC_BinningMode.NO_BINNING,
        };
        /// <summary>
        /// Binning列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<LIONUVC_BinningMode> BinningModes
        {
            get
            {
                return _binningModes;
            }
            set
            {
                _binningModes = value;
            }
        }


        private LIONUVC_XRAY_TYPE _xrayType = LIONUVC_XRAY_TYPE.VTC_D;
        /// <summary>
        /// 触发模式
        /// </summary>
        [XmlElement(ElementName = "XRay类型")]
        public LIONUVC_XRAY_TYPE XrayType
        {
            get
            {
                return _xrayType;
            }
            set
            {
                Set(() => XrayType, ref _xrayType, value);
            }
        }

        private ObservableCollection<LIONUVC_XRAY_TYPE> _xrayTypes = new ObservableCollection<LIONUVC_XRAY_TYPE>()
        {
            LIONUVC_XRAY_TYPE.VTC_D,
            LIONUVC_XRAY_TYPE.VTC_A
        };
        /// <summary>
        /// Xray类型列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<LIONUVC_XRAY_TYPE> XrayTypes
        {
            get
            {
                return _xrayTypes;
            }
            set
            {
                _xrayTypes = value;
            }
        }

        ///==================================
        private LIONUVC_MODEL_FILTER _modelFilter = LIONUVC_MODEL_FILTER.FILTER;
        /// <summary>
        /// 图像处理
        /// </summary>
        [XmlElement(ElementName = "图像处理")]
        public LIONUVC_MODEL_FILTER ModelFilter
        {
            get
            {
                return _modelFilter;
            }
            set
            {
                Set(() => ModelFilter, ref _modelFilter, value);
            }
        }

        private ObservableCollection<LIONUVC_MODEL_FILTER> _modeFilters = new ObservableCollection<LIONUVC_MODEL_FILTER>()
        {
            LIONUVC_MODEL_FILTER.FILTER,
            LIONUVC_MODEL_FILTER.NO_FILTER,

        };
        /// <summary>
        /// 图像处理列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<LIONUVC_MODEL_FILTER> ModelFilters
        {
            get
            {
                return _modeFilters;
            }
            set
            {
                _modeFilters = value;
            }
        }

       
    }
}
