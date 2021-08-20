using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NV.Config
{
    public enum NV_StatusCodes
    {
        NV_SC_SUCCESS = 0,      ///< OK      
        NV_SC_ERROR = -1001,  ///< Generic errorcode
        NV_SC_ERR_NOT_INITIALIZED = -1002,
        NV_SC_ERR_NOT_IMPLEMENTED = -1003,
        NV_SC_ERR_RESOURCE_IN_USE = -1004,
        NV_SC_ACCESS_DENIED = -1005,  ///< Access denied
        NV_SC_INVALID_HANDLE = -1006,  ///< Invalid handle
        NV_SC_INVALID_ID = -1007,  ///< Invalid ID
        NV_SC_NO_DATA = -1008,  ///< No data
        NV_SC_INVALID_PARAMETER = -1009,  ///< Invalid parameter
        NV_SC_FILE_IO = -1010,  ///< File IO error
        NV_SC_TIMEOUT = -1011,  ///< Timeout
        NV_SC_ERR_ABORT = -1012,  /* GenTL v1.1 */
        NV_SC_INVALID_BUFFER_SIZE = -1013,  ///< Invalid buffer size
        NV_SC_ERR_NOT_AVAILABLE = -1014,  /* GenTL v1.2 */
        NV_SC_INVALID_ADDRESS = -1015,  /* GenTL v1.3 */

        NV_SC_ERR_CUSTOM_ID = -10000,
        NV_SC_INVALID_FILENAME = -10001, ///< Invalid file name
        NV_SC_GC_ERROR = -10002, ///< GenICam error. 
        NV_SC_VALIDATION_ERROR = -10003, ///< Settings File Validation Error. 
        NV_SC_VALIDATION_WARNING = -10004, ///< Settings File Validation Warning. 
    }
    public enum NV_AcquisitionMode
    {
        NV_CONTINUE,				///< 连续工作模式(默认值)
        NV_FALLING_EDGE_TRIGGER,	///< 下降沿触发模式
        NV_RISING_EDGE_TRIGGER,		///< 上升沿触发模式
        NV_LOW_LEVEL_TRIGGER,		///< 高电平触发模式
        NV_HIGH_LEVEL_TRIGGER		///< 低电平触发模式
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
    public enum NV_Gain
    {
        NV_GAIN_01,					///< Sensor的增益档位：0.1PF
        NV_GAIN_04,					///< Sensor的增益档位：0.4PF
        NV_GAIN_07,					///< Sensor的增益档位：0.7PF(默认值)
        NV_GAIN_10					///< Sensor的增益档位：1.0PF
    }
    public enum NV_CorrType
    {
        NV_CORR_NO,				///< 不应用校正使能
        NV_CORR_HARD,			///< 应用硬件校正使能
        NV_CORR_SOFT			///< 应用软件校正使能
    }
    public struct NV_Version
    {
        public int FirmwareVersion;	///< 探测器的应用软件版本
        public int KernelVersion;		///< 探测器的系统内核版本
        public int HardwareVersion;	///< 探测器的处理板版本
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

        private NV_Gain _gain = NV_Gain.NV_GAIN_07;
        /// <summary>
        /// 增益档
        /// </summary>
        [XmlElement(ElementName = "增益档")]
        public NV_Gain Gain
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

        private ObservableCollection<NV_Gain> _gains = new ObservableCollection<NV_Gain>()
        {
            NV_Gain.NV_GAIN_01,
            NV_Gain.NV_GAIN_04,
            NV_Gain.NV_GAIN_07,
            NV_Gain.NV_GAIN_10,
        };
        /// <summary>
        /// 增益档列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<NV_Gain> Gains
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

        private NV_BinningMode _binningMode = NV_BinningMode.NV_BINNING_1X1;
        /// <summary>
        /// binning模式
        /// </summary>
        [XmlElement(ElementName = "binning模式")]
        public NV_BinningMode BinningMode
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
        private ObservableCollection<NV_BinningMode> _binningModes = new ObservableCollection<NV_BinningMode>()
        {
            NV_BinningMode.NV_BINNING_1X1,
            NV_BinningMode.NV_BINNING_2X2,
        };
        /// <summary>
        /// Binning列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<NV_BinningMode> BinningModes
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

        private NV_AcquisitionMode _acquisitionMode = NV_AcquisitionMode.NV_CONTINUE;
        /// <summary>
        /// 触发模式
        /// </summary>
        [XmlElement(ElementName = "触发模式")]
        public NV_AcquisitionMode AcquisitionMode
        {
            get
            {
                return _acquisitionMode;
            }
            set
            {
                Set(() => AcquisitionMode, ref _acquisitionMode, value);
            }
        }

        private ObservableCollection<NV_AcquisitionMode> _acquisitionModes = new ObservableCollection<NV_AcquisitionMode>()
        {
            NV_AcquisitionMode.NV_CONTINUE,
            NV_AcquisitionMode.NV_FALLING_EDGE_TRIGGER,
            NV_AcquisitionMode.NV_HIGH_LEVEL_TRIGGER,
            NV_AcquisitionMode.NV_LOW_LEVEL_TRIGGER,
            NV_AcquisitionMode.NV_RISING_EDGE_TRIGGER,
        };
        /// <summary>
        /// 触发模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<NV_AcquisitionMode> AcquisitionModes
        {
            get
            {
                return _acquisitionModes;
            }
            set
            {
                _acquisitionModes = value;
            }
        }

        private ObservableCollection<NV_CorrType> _correctionModes = new ObservableCollection<NV_CorrType>()
        {
            NV_CorrType.NV_CORR_HARD,
            NV_CorrType.NV_CORR_NO,
            NV_CorrType.NV_CORR_SOFT
        };
        /// <summary>
        /// 校正模式列表
        /// </summary>
        [XmlIgnoreAttribute]
        public ObservableCollection<NV_CorrType> CorrectionModes
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

        private NV_CorrType _offsetCorMode = NV_CorrType.NV_CORR_SOFT;
        /// <summary>
        /// 本底校正模式
        /// </summary>
        [XmlElement(ElementName = "本底校正模式")]
        public NV_CorrType OffsetCorMode
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
        private NV_CorrType _gainCorMode = NV_CorrType.NV_CORR_SOFT;
        /// <summary>
        /// 增益校正模式
        /// </summary>
        [XmlElement(ElementName = "增益校正模式")]
        public NV_CorrType GainCorMode
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
        private NV_CorrType _defectCorMode = NV_CorrType.NV_CORR_SOFT;
        /// <summary>
        /// 坏点校正模式
        /// </summary>
        [XmlElement(ElementName = "坏点校正模式")]
        public NV_CorrType DefectCorMode
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
