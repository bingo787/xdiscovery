using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NV.Config
{


    public enum HB_TriggerMode
    {
        SORTWARE =1 ,
        CLEAR = 2,
        HVG = 3,
        FREE_AED = 4,
        CONTINUE = 7
    }

    public enum HB_FPD_COMM_TYPE {

        UDP = 0,
        UDP_JUMBO,
        PCIE,
        WLAN
    }

    public enum NV_ShutterMode
    {
        NV_GLOBAL_SHUTTER,			///< 全局快门(默认值)
        NV_ROLLING_SHUTTER			///< 滚动快门
    }
    public enum NV_BinningMode
    {
        NV_BINNING_1X1 = 0,				///< 1x1binning(默认值)
        NV_BINNING_2X2 = 1,				///< 2x2binning
    }

    public enum HB_BinningMode
    {
        BINNING_1X1 = 1,				///< 1x1binning(默认值)
        BINNING_2X2 = 2,             ///< 2x2binning
        BINNING_4X4 = 4,				///< 2x2binning
    }

    public enum NV_Gain
    {
        NV_GAIN_01,					///< Sensor的增益档位：0.1PF
        NV_GAIN_04,					///< Sensor的增益档位：0.4PF
        NV_GAIN_07,					///< Sensor的增益档位：0.7PF(默认值)
        NV_GAIN_10					///< Sensor的增益档位：1.0PF
    }

    public enum HB_Gain {
        HB_GAIN_06 = 1,       // [1]-0.6pC
        HB_GAIN_12,         // [2]-1.2pC
        HB_GAIN_24,         // [3]-2.4pC
        HB_GAIN_36,         // [4]-3.6pC
        HB_GAIN_48,         // [5]-4.8pC
        HB_GAIN_72,         // [6]-7.2pC, 默认7.2pC
        HB_GAIN_96,         // [7]-9.6pC

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

        private HB_Gain _gain = HB_Gain.HB_GAIN_72;
        /// <summary>
        /// 增益档
        /// </summary>
        [XmlElement(ElementName = "增益档")]
        public HB_Gain Gain
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

        private ObservableCollection<HB_Gain> _gains = new ObservableCollection<HB_Gain>()
        {
            HB_Gain.HB_GAIN_06,       // [1]-0.6pC
            HB_Gain.HB_GAIN_12,         // [2]-1.2pC
            HB_Gain.HB_GAIN_24,         // [3]-2.4pC
            HB_Gain.HB_GAIN_36,         // [4]-3.6pC
            HB_Gain.HB_GAIN_48,         // [5]-4.8pC
            HB_Gain.HB_GAIN_72,         // [6]-7.2pC, 默认7.2pC
            HB_Gain.HB_GAIN_96        // [7]-9.6pC
        };
        /// <summary>
        /// 增益档列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_Gain> Gains
        {
            get
            {
                return _gains;
            }
            set
            {
                _gains = value;
            }
        }

        private int _expTime = 168;
        /// <summary>
        /// 积分时间
        /// </summary>
        [XmlElement(ElementName = "积分时间ms")]
        public int ExpTime
        {
            get
            {
                return _expTime;
            }
            set
            {
                Set(() => ExpTime, ref _expTime, value);
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

        private bool _isMultiFramesOverlay = false;
        /// <summary>
        /// 多帧叠加降噪
        /// </summary>
        [XmlElement(ElementName = "静态多帧叠加降噪")]
        public bool IsMultiFramesOverlay
        {
            get
            {
                return _isMultiFramesOverlay;
            }
            set
            {
                Set(() => IsMultiFramesOverlay, ref _isMultiFramesOverlay, value);
            }
        }
        private bool _isMultiFramesOverlayByAvg = true;
        /// <summary>
        /// 多帧叠加降噪后均值
        /// </summary>
        [XmlElement(ElementName = "静态多帧均值叠加降噪")]
        public bool IsMultiFramesOverlayByAvg
        {
            get
            {
                return _isMultiFramesOverlayByAvg;
            }
            set
            {
                Set(() => IsMultiFramesOverlayByAvg, ref _isMultiFramesOverlayByAvg, value);
            }
        }
        private int _multiFramesOverlayNumber = 2;
        /// <summary>
        /// 静态叠加降噪帧数
        /// </summary>
        [XmlElement(ElementName = "静态叠加降噪帧数")]
        public int MultiFramesOverlayNumber
        {
            get
            {
                return _multiFramesOverlayNumber;
            }
            set
            {
                Set(() => MultiFramesOverlayNumber, ref _multiFramesOverlayNumber, value);
            }
        }

        private HB_BinningMode _binningMode = HB_BinningMode.BINNING_1X1;
        /// <summary>
        /// binning模式
        /// </summary>
        [XmlElement(ElementName = "binning模式")]
        public HB_BinningMode BinningMode
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
        private ObservableCollection<HB_BinningMode> _binningModes = new ObservableCollection<HB_BinningMode>()
        {
            HB_BinningMode.BINNING_1X1,
            HB_BinningMode.BINNING_2X2,
            HB_BinningMode.BINNING_4X4,
        };
        /// <summary>
        /// Binning列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_BinningMode> BinningModes
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
        private NV_ShutterMode _shutterMode = NV_ShutterMode.NV_GLOBAL_SHUTTER;
        /// <summary>
        /// shutter模式
        /// </summary>
        [XmlElement(ElementName = "shutter模式")]
        public NV_ShutterMode ShutterMode
        {
            get
            {
                return _shutterMode;
            }
            set
            {
                Set(() => ShutterMode, ref _shutterMode, value);
            }
        }

        private ObservableCollection<NV_ShutterMode> _shutterModes = new ObservableCollection<NV_ShutterMode>()
        {
            NV_ShutterMode.NV_GLOBAL_SHUTTER,
            NV_ShutterMode.NV_ROLLING_SHUTTER,
        };
        /// <summary>
        /// 列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<NV_ShutterMode> ShutterModes
        {
            get
            {
                return _shutterModes;
            }
            set
            {
                _shutterModes = value;
            }
        }

        private HB_TriggerMode _triggerMode = HB_TriggerMode.SORTWARE;
        /// <summary>
        /// 触发模式
        /// </summary>
        [XmlElement(ElementName = "触发模式")]
        public HB_TriggerMode TriggerMode
        {
            get
            {
                return _triggerMode;
            }
            set
            {
                Set(() => TriggerMode, ref _triggerMode, value);
            }
        }

        private ObservableCollection<HB_TriggerMode> _triggerModes = new ObservableCollection<HB_TriggerMode>()
        {
            HB_TriggerMode.SORTWARE,
            HB_TriggerMode.CLEAR,
            HB_TriggerMode.HVG,
            HB_TriggerMode.FREE_AED,
            HB_TriggerMode.CONTINUE
        };
        /// <summary>
        /// 触发模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_TriggerMode> TriggerModes
        {
            get
            {
                return _triggerModes;
            }
            set
            {
                _triggerModes = value;
            }
        }

        ///==================================
        private HB_FPD_COMM_TYPE _communicationType = HB_FPD_COMM_TYPE.UDP;
        /// <summary>
        /// 通讯类型
        /// </summary>
        [XmlElement(ElementName = "通讯方式")]
        public HB_FPD_COMM_TYPE CommunicationType
        {
            get
            {
                return _communicationType;
            }
            set
            {
                Set(() => CommunicationType, ref _communicationType, value);
            }
        }

        private ObservableCollection<HB_FPD_COMM_TYPE> _communicationTypes = new ObservableCollection<HB_FPD_COMM_TYPE>()
        {
            HB_FPD_COMM_TYPE.UDP,
            HB_FPD_COMM_TYPE.UDP_JUMBO,
            HB_FPD_COMM_TYPE.WLAN,
            HB_FPD_COMM_TYPE.PCIE

        };
        /// <summary>
        /// 通讯类型列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_FPD_COMM_TYPE> CommunicationTypes
        {
            get
            {
                return _communicationTypes;
            }
            set
            {
                _communicationTypes = value;
            }
        }

        private ObservableCollection<HB_CorrType> _correctionModes = new ObservableCollection<HB_CorrType>()
        {

            HB_CorrType.NO,
            HB_CorrType.SOFTWARE,
            HB_CorrType.FRIMWARE
        };
        /// <summary>
        /// 校正模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_CorrType> CorrectionModes
        {
            get
            {
                return _correctionModes;
            }
            set
            {
                _correctionModes = value;
            }
        }

        //

        private ObservableCollection<HB_OffsetCorrType> _offsetCorrectionModes = new ObservableCollection<HB_OffsetCorrType>()
        {
            HB_OffsetCorrType.NO ,              
            HB_OffsetCorrType.SOFTWARE_PRE_OFFSET ,        
            HB_OffsetCorrType.FIRMWARE_PRE_OFFSET,       
            HB_OffsetCorrType.FIRMWARE_POST_OFFSET
        };
        /// <summary>
        /// Offset 校正模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<HB_OffsetCorrType> OffsetCorrectionModes
        {
            get
            {
                return _offsetCorrectionModes;
            }
            set
            {
                _offsetCorrectionModes = value;
            }
        }

        //

        private HB_OffsetCorrType _offsetCorMode = HB_OffsetCorrType.FIRMWARE_POST_OFFSET;
        /// <summary>
        /// 本底校正模式
        /// </summary>
        [XmlElement(ElementName = "本底校正模式")]
        public HB_OffsetCorrType OffsetCorMode
        {
            get
            {
                return _offsetCorMode;
            }
            set
            {
                Set(() => OffsetCorMode, ref _offsetCorMode, value);
            }
        }
        private HB_CorrType _gainCorMode = HB_CorrType.SOFTWARE;
        /// <summary>
        /// 增益校正模式
        /// </summary>
        [XmlElement(ElementName = "增益校正模式")]
        public HB_CorrType GainCorMode
        {
            get
            {
                return _gainCorMode;
            }
            set
            {
                Set(() => GainCorMode, ref _gainCorMode, value);
            }
        }
        private HB_CorrType _defectCorMode = HB_CorrType.SOFTWARE;
        /// <summary>
        /// 坏点校正模式
        /// </summary>
        [XmlElement(ElementName = "坏点校正模式")]
        public HB_CorrType DefectCorMode
        {
            get
            {
                return _defectCorMode;
            }
            set
            {
                Set(() => DefectCorMode, ref _defectCorMode, value);
            }
        }

    }
}
