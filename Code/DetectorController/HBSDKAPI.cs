using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace Detector
{


    // Notice: call back function
    // @USER_CALLBACK_HANDLE_ENVENT
    // @cmd:enum eEventCallbackCommType
    // @buff
    // @len
    // @nID
    //typedef int (* USER_CALLBACK_HANDLE_ENVENT) (unsigned char cmd, void* buff, int len, int nID);
    public unsafe delegate int USER_CALLBACK_HANDLE_ENVENT(Byte cmd, void* buff, int len, int nID);

    // Notice: call back function
    // @USER_CALLBACK_HANDLE_PROCESS
    // @cmd:enum eAnswerCallbackCommType
    // @buff
    // @len
    //typedef int (* USER_CALLBACK_HANDLE_PROCESS) (unsigned char cmd, int retcode, void* inContext);
    public unsafe delegate int USER_CALLBACK_HANDLE_PROCESS(Byte cmd, int retcode, ushort* buff);



    public enum HBIRETCODE
    {
        HBI_SUCCSS = 0,
        HBI_ERR_OPEN_DETECTOR_FAILED = 8001,
        HBI_ERR_INVALID_PARAMS = 8002,
        HBI_ERR_CONNECT_FAILED = 8003,
        HBI_ERR_MALLOC_FAILED = 8004,
        HBI_ERR_RELIMGMEM_FAILED = 8005,
        HBI_ERR_RETIMGMEM_FAILED = 8006,
        HBI_ERR_NODEVICE = 8007,
        HBI_ERR_NODEVICE_TRY_CONNECT = 8008,
        HBI_ERR_DEVICE_BUSY = 8009,
        HBI_ERR_SENDDATA_FAILED = 8010,
        HBI_ERR_RECEIVE_DATA_FAILED = 8011,
        HBI_ERR_COMMAND_DISMATCH = 8012,
        HBI_ERR_NO_IMAGE_RAW = 8013,
        HBI_ERR_PTHREAD_ACTIVE_FAILED = 8014,
        HBI_ERR_STOP_ACQUISITION = 8015,
        HBI_ERR_INSERT_FAILED = 8016,
        HBI_ERR_GET_CFG_FAILED = 8017,
        HBI_NOT_SUPPORT = 8018,
        HBI_REGISTER_CALLBACK_FAILED = 8019,
        HBI_SEND_MESSAGE_FAILD = 8020,
        HBI_ERR_WORKMODE = 8021,
        HBI_FAILED = 8022,
        HBI_FILE_NOT_EXISTS = 8023,
        HBI_COMM_TYPE_ERR = 8024,
        HBI_TYPE_IS_NOT_EXISTS = 8025,
        HBI_SAVE_FILE_FAILED = 8026,
        HBI_INIT_PARAM_FAILED = 8027,
        HBI_END = 8028
    };

    public struct CodeStringTable
    {
        int num;
        int HBIRETCODE;
        string errString;
    };
   

    //#define JUMBO_PACKET//add by lss

    public struct HBConstant
    {
        public const int FPD_DATA_BITS = 16;
        public const int PACKET_MAX_SIZE = 8218;
        public const int IMG_PACKET_DATA_MAX_LEN = 8192;
        public const int CFG_PACKET_DATA_MAX_LEN = 1024;
        public const int CUSTOMEFFECTRECVDATASIZE = 8202;
        public const int USHORT_PACKET_DATA_MAX_LEN = 4096;

        public const int LIVE_ACQ_MAX_FRAME_NUM = 20;

        public const int MAX_PATH = 260;
    }



    //#else// JUMBO_PACKET
    //public const int PACKET_MAX_SIZE = 1050;
    //public const int IMG_PACKET_DATA_MAX_LEN = 1024;
    //public const int CUSTOMEFFECTRECVDATASIZE = 1034;







    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    // Notice:Fpd Size struct
    public struct FpdPixelMatrixTable
    {
        public uint fpd_size;
        public uint fpd_width;
        public uint fpd_height;
        public uint fpd_packet_sum;

        public FpdPixelMatrixTable(uint fpd_size, uint fpd_width, uint fpd_height, uint fpd_packet_sum)
        {
            this.fpd_size = fpd_size;
            this.fpd_width = fpd_width;
            this.fpd_height = fpd_height;
            this.fpd_packet_sum = fpd_packet_sum;
        }
    };

    // Notice:Fpd Pixel Matrix Table,size is 14.
    // static const FpdPixelMatrixTable[] HB_FPD_SIZE = {

    //  FpdPixelMatrixTable( 1, 3072, 3072, 2304 ),/*,"4343-140um" */
    // FpdPixelMatrixTable( 2, 2560, 3072, 1920 ),/*,"3543-140um" */
    // FpdPixelMatrixTable( 3, 1264, 1024, 316  ),/*,"1613-125um" */
    // FpdPixelMatrixTable( 4, 2048, 2048, 1024 ),/*,"3030-140um" */
    // FpdPixelMatrixTable( 5, 2816, 3584, 2464 ),/*,"2530-85um" */
    // FpdPixelMatrixTable( 6, 2048, 1792, 896  ),/*,"3025-140um" */
    // FpdPixelMatrixTable( 7, 4288, 4288, 4489 ),/*,"4343-100um" - 5240 2952*/
    // FpdPixelMatrixTable( 8, 3072, 3840, 2880 ),/*,"2530-75um"*/
    // FpdPixelMatrixTable( 9, 1024, 1024, 256  ), /*,"2121-200um"*/
    //  }; 

    // Notice: Image Property
    public struct IMAGE_PROPERTY
    {
        public uint nSize;                //fpd_size
        public uint nwidth;               //image width
        public uint nheight;              //image height
        public uint datatype;             //data type：0-unsigned char,1-char,2-unsigned short,3-float,4-double
        public uint ndatabit;             //data bit:0-16bit,1-14bit,2-12bit,3-8bit
        public uint nendian;              //endian:0-little endian,1-biger endian
        public uint packet_sum;           //fpd send packet sum per frame
        public uint tail_size;            //Tail packet size
        public int frame_size;                    //data size per frame
    };

    // Notice:Fpd Trigger Work Mode
    public enum TRIGGER_MODE
    {
        INVALID_TRIGGER_MODE = 0x00,
        STATIC_SOFTWARE_TRIGGER_MODE = 0x01,         // 
        STATIC_PREP_TRIGGER_MODE = 0x02,         //
        STATIC_HVG_TRIGGER_MODE = 0x03,         //
        STATIC_AED_TRIGGER_MODE = 0x04,
        DYNAMIC_HVG_TRIGGER_MODE = 0x05,
        DYNAMIC_FPD_TRIGGER_MODE = 0x06,
        DYNAMIC_FPD_CONTINUE_MODE = 0x07
    };

    // Notice:fpd software calibrate enable info.
    public class SOFTWARE_CALIBRATE_ENABLE
    {


        public bool enableOffsetCalib;
        public bool enableGainCalib;
        public bool enableDefectCalib;
        public bool enableDummyCalib;

        public SOFTWARE_CALIBRATE_ENABLE()
        {
            this.enableOffsetCalib = false;
            this.enableGainCalib = false;
            this.enableDefectCalib = false;
            this.enableDummyCalib = false;
        }
    };

    // Notice:fpd software calibrate enable info.
    public class IMAGE_CORRECT_ENABLE
    {

        public bool bFeedbackCfg;                  //true-ECALLBACK_TYPE_ROM_UPLOAD Event,false-ECALLBACK_TYPE_SET_CFG_OK Event
        public byte ucOffsetCorrection;   //00-"Do nothing";01-"prepare Offset Correction";  02-"post Offset Correction";
        public byte ucGainCorrection;     //00-"Do nothing";01-"Software Gain Correction";   02-"Hardware Gain Correction"
        public byte ucDefectCorrection;   //00-"Do nothing";01-"Software Defect Correction"; 02-"Software Defect Correction"
        public byte ucDummyCorrection;    //00-"Do nothing";01-"Software Dummy Correction";  02-"Software Dummy Correction"

        public IMAGE_CORRECT_ENABLE()
        {
            this.bFeedbackCfg = false;
            this.ucOffsetCorrection = 0;
            this.ucGainCorrection = 0;
            this.ucDefectCorrection = 0;
            this.ucDummyCorrection = 0;
        }
    };

    // Notice: acq mode:static and dynamic
    public enum IMAGE_ACQ_MODE
    {
        STATIC_ACQ_DEFAULT_MODE = 0x00,   // 默认单帧采集
        DYNAMIC_ACQ_DEFAULT_MODE,         // 默认连续采集，固件做offset
        DYNAMIC_ACQ_BARK_MODE,            // 创建Offset模板-连续采集暗场图,软件做offset
        DYNAMIC_ACQ_BRIGHT_MODE,          // 创建Gain模板-连续采集亮场图
        STATIC_ACQ_BRIGHT_MODE,           // 创建Gain模板-单帧采集亮场图
        STATIC_DEFECT_ACQ_MODE,           // 创建Defect模板-缺陷校正采集亮场图
        DYNAMIC_DEFECT_ACQ_MODE
    };

    // Notice: generate template file type
    public enum TFILE_MODE
    {
        GAIN_T = 0x00,
        DEFECT_T = 0x01,
        FIREWARE_BIN = 0x02
    };

    // download template file type
    public enum DOWNLOAD_FILE_TYPE
    {
        OFFSET_TMP = 0x00,
        GAIN_TMP = 0x01,
        DEFECT_TMP = 0x02
    }


    // upload process status
    public enum UPLOAD_FILE_STATUS
    {
        UPLOAD_FILE_START = 0x00,
        UPLOAD_FILE_DURING = 0x01,
        UPLOAD_FILE_STOP = 0x02
    };

    // template file attributes
    public class HBIDOWNLOAD_FILE
    {

        public HBIDOWNLOAD_FILE()
        {
            emfiletype = DOWNLOAD_FILE_TYPE.OFFSET_TMP;
            filepath = "";
        }


        public DOWNLOAD_FILE_TYPE emfiletype;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HBConstant.MAX_PATH)]
        public string filepath;


    };

    public enum BadPixel_T //坏点坏线添加
    {
        BAD_PIXEL = 0x00,
        BAD_ROW = 0x01,
        BAD_COL = 0x02,
        BAD_LINE_BEGIN = 0x03,
        BAD_LINE_END = 0x04,
        DEL_BAD_PIXEL = 0x05,
        DEL_BAD_ROW = 0x06,
        DEL_BAD_COL = 0x07,
        DEL_BAD_LINE_BEGIN = 0x08,
        DEL_BAD_LINE_END = 0x09
    };

    public enum GRAY_MODE
    {
        GRAY_8 = 0x00,
        GRAY_16 = 0x01,
        GRAY_32 = 0x02
    };

    public enum LIVE_MODE
    {
        ACQ_OFFSET_TI = 0x01,//acquistion offset template and image
        ACQ_IMAGE,// Only acq image
        ACQ_OFFSET_T
    };

    // Notice:fpd aqc mode
    public class FPD_AQC_MODE
    {
        public FPD_AQC_MODE()
        {
            aqc_mode = IMAGE_ACQ_MODE.DYNAMIC_ACQ_DEFAULT_MODE;
            ngroupno = 0;
            nframesum = 0;
            ndiscard = 0;
            nframeid = 0;
            nLiveMode = LIVE_MODE.ACQ_IMAGE;//add by lss
            isOverLap = false;//add by lss 2020/8/12
            nGrayBit = GRAY_MODE.GRAY_16;//add by lss 2020/8/12
            bSimpleGT = false;//简单快速做模板
        }
        public IMAGE_ACQ_MODE aqc_mode;
        public int ngroupno;
        public int nframesum;
        public int ndiscard;
        public int nframeid;
        public LIVE_MODE nLiveMode;
        public bool isOverLap;
        public bool bSimpleGT;
        public GRAY_MODE nGrayBit;//16,32
                                  /*
                                  01：Image+OffsetTemplate 带有该参数点击Live Acquisition命令时，探测器会先做OFFSET模板再进行采集图像
                                  02：Only Image	         带有该参数点击Live Acquisition命令时,探测器会直接进行上图。不会有做模板的时间
                                  03：Only OffsetTemplate   带有该参数点击Live Acquisition命令时,探测器会只做模板。做完模板就会结束状态。
                                  */
    };

    public class DOWNLOAD_T_FILE_MODE_ST
    {
        DOWNLOAD_T_FILE_MODE_ST()
        {
            tf_mode = TFILE_MODE.GAIN_T;
            filepath = "";
        }

        public TFILE_MODE tf_mode;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HBConstant.MAX_PATH)]
        public string filepath;
    };
    // Notice: After Each Member Variables, show Variables enum , 
    // before '-' is variables' value, after '-' is the meaning of the value;
    // each value departed by ';' symbol
    public enum eRequestCommType
    {
        EDL_COMMON_TYPE_INVALVE = 0x00,
        EDL_COMMON_TYPE_GLOBAL_RESET = 0x01,
        EDL_COMMON_TYPE_PREPARE = 0x02,
        EDL_COMMON_TYPE_SINGLE_SHORT = 0x03,
        EDL_COMMON_TYPE_LIVE_ACQ = 0x04,
        EDL_COMMON_TYPE_STOP_ACQ = 0x05,
        EDL_COMMON_TYPE_PACKET_MISS = 0x06,
        EDL_COMMON_TYPE_FRAME_MISS = 0x07,
        EDL_COMMON_TYPE_DUMMPING = 0x08,
        EDL_COMMON_TYPE_END_RESPONSE = 0x0A, // End response packet
        EDL_COMMON_TYPE_CONNECT_FPD = 0x0B, // connect add by mhyang 20210301
        EDL_COMMON_TYPE_DISCONNECT_FPD = 0x0C, // disconnect add by mhyang 20210301
        EDL_COMMON_TYPE_SET_RAM_PARAM_CFG = 0x10,
        EDL_COMMON_TYPE_GET_RAM_PARAM_CFG = 0x11,
        EDL_COMMON_TYPE_SET_ROM_PARAM_CFG = 0x12,
        EDL_COMMON_TYPE_GET_ROM_PARAM_CFG = 0x13,
        EDL_COMMON_TYPE_SET_FACTORY_PARAM_CFG = 0x14,
        EDL_COMMON_TYPE_GET_FACTORY_PARAM_CFG = 0x15,
        EDL_COMMON_TYPE_RESET_FIRM_PARAM_CFG = 0x16, // add by mhyang 20200821
        EDL_COMMON_DOWNLOAD_OFFSET_TEMPLATE = 0x2E, //add by MH.YANG 30/12
        EDL_COMMON_DOWNLOAD_GAIN_TEMPLATE = 0x2F, //add by lss 04/10
        EDL_COMMON_DOWNLOAD_DEFECT_TEMPLATE = 0x30,
        EDL_COMMON_TYPE_UPDATE_BIN_FILE = 0x50,
        EDL_COMMON_TYPE_ERASE_BIN_FILE = 0x4F,
        EDL_COMMON_TYPE_SET_AQC_MODE = 0xFF
    };

    // Notice: After Each Member Variables, show Variables enum , 
    // before '-' is variables' value, after '-' is the meaning of the value;
    // each value departed by ';' symbol
    public enum CALLBACK_EVENT_COMM_TYPE
    {
        ECALLBACK_TYPE_INVALVE = 0X00,
        ECALLBACK_TYPE_COMM_RIGHT = 0X01,
        ECALLBACK_TYPE_COMM_WRONG = 0X02,
        ECALLBACK_TYPE_DUMMPLING = 0X03,
        ECALLBACK_TYPE_ACQ_END = 0X04,
        ECALLBACK_TYPE_FW_UPDATE_PROGRESS = 0x06,
        ECALLBACK_TYPE_FW_EASE_FINISH = 0x07,
        ECALLBACK_TYPE_FPD_STATUS = 0X09, // 状态包
        ECALLBACK_TYPE_ROM_UPLOAD = 0X10,
        ECALLBACK_TYPE_RAM_UPLOAD = 0X11,
        ECALLBACK_TYPE_FACTORY_UPLOAD = 0X12,
        ECALLBACK_TYPE_SINGLE_IMAGE = 0X51,
        ECALLBACK_TYPE_MULTIPLE_IMAGE = 0X52,
        ECALLBACK_TYPE_PACKET_MISS = 0X5B,
        ECALLBACK_TYPE_LIVE_ACQ_OK = 0XA0,
        ECALLBACK_TYPE_ADMIN_CFG_UPDATA = 0XA1,
        ECALLBACK_TYPE_USER_CFG_UPDATA = 0XA2,
        ECALLBACK_TYPE_PACKET_MISS_MSG = 0XA4,
        ECALLBACK_TYPE_THREAD_EVENT = 0XA5,
        ECALLBACK_TYPE_CALIBRATE_ACQ_MSG = 0XA6,
        ECALLBACK_TYPE_OFFSET_ERR_MSG = 0XA7,
        ECALLBACK_TYPE_GAIN_ERR_MSG = 0XA8,
        ECALLBACK_TYPE_DEFECT_ERR_MSG = 0XA9,
        ECALLBACK_TYPE_NET_ERR_MSG = 0XAA
    };

    public enum eCallbackUpdateFirmwareStatus
    {
        ECALLBACK_UPDATE_STATUS_START = 0XB0,
        ECALLBACK_UPDATE_STATUS_PROGRESS = 0XB1,
        ECALLBACK_UPDATE_STATUS_RESULT = 0XB2
    };

    public enum eCallbackDownloadTemplateFileStatus
    {
        ECALLBACK_DTFile_STATUS_CONNECT = 0XC0,
        ECALLBACK_DTFile_STATUS_ERASE = 0XC1,
        ECALLBACK_DTFile_STATUS_PROGRESS = 0XC2,
        ECALLBACK_DTFile_STATUS_RESULT = 0XC3,
        ECALLBACK_DTFile_STATUS_NET = 0XC4,
        ECALLBACK_DTFile_STATUS_CMD_ERR = 0XC5,
        ECALLBACK_DTFile_STATUS_SEND = 0XC6,
        ECALLBACK_DTFile_STATUS_NET_ERR = 0XC7
    };


    // Notice: calibrate template raw file call back info
    public class CALLBACK_RAW_INFO
    {
        public CALLBACK_RAW_INFO()
        {
            szRawName = "";
            dMean = 0.0;
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HBConstant.MAX_PATH)]
        public string szRawName;   // 返回数据文件名称
        public double dMean;               // 灰度均值
    };

    // Notice: calibrate
    public enum EnumTempType
    {
        OFFSETFILE = 0x00,
        GAINFILE,
        BADPIXFILE,
        GAINA,
        GAINB
    };

    public struct calibrateModeItem
    {
        int id;                    //ID标识
        int dosvalue;              //剂量值
        int groupnum;              //获取测试组总数
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HBConstant.MAX_PATH)]
        string offsetpath; //亮场offset文件路径
        int averagegray;           //该剂量下所有帧的平均灰度
    };

    // Notice:generate calibrate template input param
    public class CALIBRATE_INPUT_PARAM
    {
        public CALIBRATE_INPUT_PARAM()
        {
            image_w = 0;
            image_h = 0;
            binning = 1;
            roi_x = 0;
            roi_y = 0;
            roi_w = 0;
            roi_h = 0;
            group_sum = 0;
            per_group_num = 0;
        }
        public int image_w;       // image width
        public int image_h;       // image height
        public int binning;       // binning
        public int roi_x;         // ROI left
        public int roi_y;         // ROI top
        public int roi_w;         // ROI width
        public int roi_h;         // ROI height
        public int group_sum;     // group sum
        public int per_group_num; // num per group
    }
    ;


    // System Manufacture Information：100
    public struct SysBaseInfo
    {
        // Register_1 // Register_1~Register_100
        Byte m_byProductType;                      //01	//01-Flat Panel Detector;  02-Linear Array Detector
                                                   // Register_2
        Byte m_byXRaySensorType;                   //01	//01-Amorphous Silicon; 02-CMOS; 03-IGZO; 04-LTPS; 05-Amorphous Selenium
                                                   // Register_3
        Byte m_byPanelSize;                        //01	//01-1717 inch Panel Size; 02-1417; 03-1414; 04-1212; 
                                                   //05-1012; 06-0912; 07-0910; 08-0909; 09-0506; 10-0505; 11-0503
                                                   // Register_4
        Byte m_byPixelPitch;                       //01	//01-27 um; 02-50; 03-75; 04-85; 05-100; 06-127; 07-140; 08-150; 09-180; 10-200 11-205; 12-400; 13-1000
                                                   // Register_5
        Byte m_byPixelMatrix;                  //01	//01-"3072 x 3072"; 02-"2536 x 3528"; 03-"2800 x 2304"; 04-"2304 x 2304"; 05-"2048 x 2048"; 06-"1536 x 1536"; 
                                               //07-"1024 x 1536"; 08-"1024 x 1024"; 09-"1024 x 0768"; 10-"1024 x 0512"; 11-"0768 x 0768"; 
                                               //12-"0512 x 0512"; 13-"0512 x 0256"; 14-"0256 x 0256"
                                               // Register_6
        Byte m_byScintillatorType;             //01	//01-DRZ Standard; 02-DRZ High; 03-DRZ Plus; 04-PI-200; 05-Structured GOS; 06-CSI Evaporation; 07-CSI Attach;
                                               // Register_7
        Byte m_byScanLineFanoutMode;               //01	//01-Single Side Single Gate; 02-Dual Side Single Gate; 03-Single Side Dual Gate; 04-Dual Side Dual Gate;
                                                   // Register_8
        Byte m_byReadoutLineFanoutMode;        //01	//01-Single Side Single Read; 02-Dual Side Single Read; 03-Single Side Dual Read; 04-Dual Side Dual Read;
                                               // Register_9
        Byte m_byFPDMode;                      //01	//01-Static; 02-Dynamic;
                                               // Register_10
        Byte m_byFPDStyle;                     //01	//01-Fixed; 02-Wireless; 03-Portable;
                                               // Register_11
        Byte m_byApplicationMode;              //01	//01-Medical; 02-Industry;
                                               // Register_12
        Byte m_byGateChannel;                  //01	//01-"600 Channel"; 02-"512 Channel"; 03-"384 Channel"; 04-"256 Channel"; 05-"128 Channel"; 06-"064 Channel"
                                               // Register_13
        Byte m_byGateType;                     //01	//01-"HX8677"; 02-"HX8698D"; 03-"NT39565D"
                                               // Register_14
        Byte m_byAFEChannel;                       //01	//01-"64 Channel"; 02-"128 Channel"; 03-"256 Channel"; 04-"512 Channel";
                                                   // Register_15
        Byte m_byAFEType;                      //01	//01-"AFE0064"; 02-"TI COF 2256"; 03-"AD8488"; 04-"ADI COF 2256";
                                               // Register_16~Register_17
        ushort m_sGateNumber;                       //02	//Gate Number
                                                    // Register_18~Register_19
        ushort m_sAFENumber;                        //02	//AFE Number		
                                                    // Register_20~Register_21
        ushort m_sImageWidth;                       //02	//Image Width // modified by mhyang 20191220
                                                    // Register_22~Register_23
        ushort m_sImageHeight;                      //02	//Image Height
                                                    // Register_24~Register_37
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        string m_cSnNumber;                  //14	//sn number   // modified by mhyang 20200401
                                             // Register_38~Register_100.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 63)]
        string m_abySysBaseInfoReserved;     //63//////Registers 38To100(include) Are Reserved;
    };

    // System Manufacture Information：50
    public struct SysManuInfo
    {
        // Register_1~Register_4 // Register_100~Register_150
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_16
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_abyFPDSerialNumber;         //12	//FPD SN public Byte 01-12;
                                                                                                      // Register_17~Register_18
        public ushort m_sYear;                         //02	//Year
                                                       // Register_19~Register_20
        public ushort m_sMouth;                            //02	//Month
                                                           // Register_21~Register_22
        public ushort m_sDay;                              //02	//Day
                                                           // Register_23~Register_24
        public ushort m_sEOSFirmwareVersion;               //02	//EOS Firmware Version
                                                           // Register_25~Register_26
        public ushort m_sEOSFirmwareBuildingTime;          //02	//EOS Firmware Building Time
                                                           // Register_27~Register_28
        public ushort m_sMasterFPGAFirmwareVersion;        //02	//Master FPGA Firmware Version
                                                           // Register_29~Register_31
        public Byte m_byMasterFPGAFirmwareBuildingTime1;//01	//Master FPGA Firmware Building Time1
        public Byte m_byMasterFPGAFirmwareBuildingTime2;//01	//Master FPGA Firmware Building Time2
        public Byte m_byMasterFPGAFirmwareBuildingTime3;//01	//Master FPGA Firmware Building Time3
                                                        // Register_32~Register_34
        public Byte m_byMasterFPGAFirmwareStatus1;        //01	//Master FPGA Firmware status1
        public Byte m_byMasterFPGAFirmwareStatus2;        //01	//Master FPGA Firmware status2
        public Byte m_byMasterFPGAFirmwareStatus3;      //01	//Master FPGA Firmware status3
                                                        // Register_35~Register_36
        public ushort m_sMCUFirmwareVersion;               //02	//MCU Firmware Version
                                                           // Register_37~Register_38
        public ushort m_sMCUFirmwareBuildingTime;          //02	//MCU Firmware Building Time
                                                           // Register_39~Register_50
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public Byte m_abySysManuInfoReserved;
    };

    // System Status Information：28
    public struct SysStatusInfo
    {
        // Register_1~Register_4 // Register_151~Register_178
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_8
        public uint m_unTemperature;                   //04	//Temperature
                                                       // Register_9~Register_12
        public uint m_unHumidity;                      //04	//Humidity
                                                       // Register_13~Register_16
        public uint m_unDose;                          //04	//Dose
                                                       // Register_17~Register_28
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public Byte m_abySysStatusInfoReserved;
    }
        ;

    // Gigabit Ethernet Information：40
    public struct EtherInfo
    {
        // Register_1~Register_4 // Register_179~Register_218
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public Byte m_byHead;
        // Register_9~Register_14
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)] public Byte m_bySourceMAC;                   //06	//Source MAC
                                                                                                         // Register_15~Register_18
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_bySourceIP;                    //04	//Source IP
                                                                                                         // Register_5~Register_6
        public ushort m_sSourceUDPPort;                    //02	//Source UDP Port
                                                           // Register_19~Register_24
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)] public Byte m_byDestMAC;                     //06	//Dest MAC
                                                                                                         // Register_25~Register_28
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byDestIP;                      //04	//Dest IP
                                                                                                         // Register_7~Register_8
        public ushort m_sDestUDPPort;                      //02	//Dest UDP Port	
                                                           // Register_29~Register_40
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public Byte m_abyEtherInfoReserved;
    }
        ;

    // High Voltage Generator Interface Trigger Mode Information：21
    public struct HiVolTriggerModeInfo
    {
        // Register_1~Register_4  //Register_219~Register_239
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5
        public Byte m_byPrepSignalValidTrigger;           //01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                          //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
                                                          // Register_6
        public Byte m_byExposureEnableSignal;         //01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                      //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
                                                      // Register_7
        public Byte m_byXRayExposureSignalValid;      //01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                      //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
                                                      // Register_8
        public Byte m_bySyncInSignalValidTrigger;     //01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                      //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid";
                                                      // Register_9
        public Byte m_bySyncOutSignalValidTrigger;        //01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid";
                                                          // Register_10~Register_21
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public Byte m_abyEtherInfoReserved;
    }
        ;

    // System Configuration Information：128
    public struct SysCfgInfo
    {
        // Register_1~Register_4   //Register_240~Register_367	
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5
        public Byte m_byWorkMode;                     //01	// 01-"Static: Software Mode" 02-"Static: Prep Mode"; 03-"Static Hvg Trigger Mode"; 	                                                            // 04-Static AED Trigger Mode, 05-Dynamic Hvg In Mode,06-Dynamic Fpd Out Mode.
                                                      // Register_6
        public Byte m_byPreviewModeTransmission;      //01	//00-"Just transmmit full resolution image matrix"; 01-"Just transmmit the preview image"; 
                                                      //02-"Transmmit the preview image and full resolution image"
                                                      // Register_7
        public Byte m_byPositiveVsNegativeIntegrate;  //01	//01-"Electron"; 02-"Hole" //04-"Static: Inner Trigger Mode"; 05-"Dynamic: Sync In Mode"; 06-"Dynamic: Sync Out Mode"
                                                      // Register_8
        public Byte m_byBinning;                      //01	//01-"1x1"; 02-"2x2 Binning"; 03-"3x3 Binning"; 04-"4x4 Binning";
                                                      // Register_9
        public Byte m_byIntegratorCapacitorSelection; //01	//01-"Integrator Capacitor PGA0"; 02-"Integrator Capacitor PGA1"; 03-"PGA2";......;08-"Integrator Capacitor PGA7"
                                                      // Register_10
        public Byte m_byAmplifierGainSelection;           //01	//01-"Amplifier Gain AMP0"; 02-"Amplifier Gain AMP1"; 03-"Amplifier Gain AMP2"; 04-"Amplifier Gain AMP3";
                                                          //05-"Amplifier Gain AMP4"; 06-"Amplifier Gain AMP5"; 07-"Amplifier Gain AMP6"; 08-"Amplifier Gain AMP7";
                                                          // Register_11
        public Byte m_bySelfDumpingEnable;                //01	//01-"Enable Dumping Process"; 02-"Disable Dumping Process";
                                                          // Register12
        public Byte m_bySelfDumpingProcessing;            //01	//01-"Clear Process for Dumping"; 02-"Acquisition Process for Dumping";	
                                                          // add 20190705
        public Byte m_byClearFrameNumber;               //01	//01-"Clear Frame"; 02-"Clear Frame";03-"Clear Frame";04-"Clear Frame";
                                                        // Register_13
        public Byte m_byPDZ;                          //01	//01-"Disable PDZ"; 02-"Enable PDZ"
                                                      // Register_14
        public Byte m_byNAPZ;                         //01	//01-"Disable NAPZ"; 02-"Enable NAPZ"
                                                      // Register15
        public Byte m_byInnerTriggerSensorSelection;  //01	//00-"No Selection"; 01-"I Sensor Selection"; 02-"II Sensor Selection"; 
                                                      //03-"III Sensor Selection"; 04-"IV Sensor Selection"; 05-"V Sensor Selection"                                                                
                                                      // Register_16~Register_17
        public ushort m_sZoomBeginRowNumber;               //02	//Zoom Begin Row Number
                                                           // Register_18~Register_19
        public ushort m_sZoomEndRowNumber;             //02	//Zoom End Row Number
                                                       // Register_20~Register_21
        public ushort m_sZoomBeginColumnNumber;            //02	//Zoom Begin Column Number
                                                           // Register_22~Register_23
        public ushort m_sZoomEndColumnNumber;              //02	//Zoom End Column Number
                                                           // Register24~Register27
        public uint m_unSelfDumpingSpanTime;           //04	//unit: ms
                                                       // Register28~Register31
        public uint m_unContinuousAcquisitionSpanTime; //04	//unit: ms
                                                       // Register32~Register35
        public uint m_unPreDumpingDelayTime;           //04	//unit: ms
                                                       // Register36~Register39
        public uint m_unPreAcquisitionDelayTime;       //04	//unit: ms
                                                       // Register40~Register43
        public uint m_unPostDumpingDelayTime;          //04	//unit: ms
                                                       // Register44~Register47
        public uint m_unPostAcquisitionDelayTime;      //04	//unit: ms
                                                       // Register48~Register51
        public uint m_unSyncInExposureWindowTime;      //04	//unit: ms
                                                       // Register52~Register55
        public uint m_unSyncOutExposureWindowTime;     //04	//unit: ms
                                                       // Register56~Register59
        public uint m_unTFTOffToIntegrateOffTimeSpan;  //04	//unit: ms
                                                       // Register_60~Register_63
        public uint m_unIntegrateTime;                 //04	//unit: ms
                                                       // Register_64~Register_67
        public uint m_unPreFrameDelay;                 //04	//unit: ms
                                                       // Register_68~Register_71
        public uint m_unPostFrameDelay;                    //04	//unit: ms
                                                           // Register_72~Register_75
        public uint m_unSaturation;
        // Register_76~Register_79
        public uint m_unClipping;                      //04	//unit: ms
                                                       // Register_80~Register_81
        public ushort m_byHvgRdyMode;                     //02	//0-"Edge Trigger"; 非0-"[1~255]:Config Delay,unit:n*128 ms"
                                                          // Register_82~Register_83
        public ushort m_sSaecPreRdyTm;                 //02	//SAEC pre ready time
                                                       // Register_84~Register_85
        public ushort m_sSaecPostRdyTm;                    //02	//SAEC post ready time
                                                           // Register_86
        public Byte m_byDefectThreshold;                //01	//Defect correction:Calculating the threshold of bad points
                                                        // Register_87~Register_127
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)] public Byte m_abySysCfgInfoReserved;
    };

    // Image Calibration Configuration：20
    public struct ImgCaliCfg
    {
        // Register_1~Register_4 //Register_368~Register_387
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;                        //04    //head 
                                                                                                         // Register_5
        public Byte m_byOffsetCorrection;             //01	//01-"Software Offset Correction"; 02-"Hardware Offset Correction";
                                                      // Register_6
        public Byte m_byGainCorrection;                   //01	//01-"Software Gain Correction"; 02-"Hardware Gain Correction"
                                                          // Register_7
        public Byte m_byDefectCorrection;             //01	//01-"Software Defect Correction"; 02-"Hardware Defect Correction"
                                                      // Register_8
        public Byte m_byDummyCorrection;              //01	//01-"Software Dummy Correction"; 02-"Software Dummy Correction"
                                                      // Register_9~Register_20
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public Byte m_abyImgCaliCfgReserved;      //12//////Registers 114(include) Are Reserved;
    }
        ;

    // Voltage Adjust Configuration：48
    public struct VolAdjustCfg
    {
        // Register_1~Register_4  //Register_388~Register_435
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_8
        public uint m_unVCOM;                          //04	//VCOM
                                                       // Register_9~Register_12
        public uint m_unVGG;                           //04	//VGG
                                                       // Register_13~Register_16
        public uint m_unVEE;                           //04	//VEE
                                                       // Register_17~Register_20
        public uint m_unVbias;                         //04	//Vbias	
                                                       // Register_21~Register_36
        [MarshalAs(UnmanagedType.SysUInt, SizeConst = 4)] public uint m_unComp;                       //04	//Comp1	// Register114
                                                                                                      // Register_37~Register_48
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_abyVolAdjustCfgReserved;        //12//////Registers 114(include) Are Reserved;
    };

    // TI COF Parameter Configuration：84
    public struct TICOFCfg
    {
        // Register_1~Register_4  //Register_436~Register_519
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_64
        [MarshalAs(UnmanagedType.U2, SizeConst = 30)] public ushort m_sTICOFRegister;
        // Register_65~Register_84
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)] public Byte m_abyTICOFCfgReserved;
    }
        ;

    //CMOS Parameter Configuration：116
    public struct CMOSCfg
    {
        // Register_1~Register_4 //Register_520~Register_635
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_68
        [MarshalAs(UnmanagedType.U2, SizeConst = 32)] public ushort m_sCMOSRegister;
        // Register_69~Register_116
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)] public Byte m_abyCMOSCfgReserved;
    };

    // ADI COF Parameter Configuration：389
    public struct ADICOFCfg
    {
        // Register_1~Register_4 // Register_636~Register_1024
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public Byte m_byHead;
        // Register_5~Register_19
        [MarshalAs(UnmanagedType.U2, SizeConst = 15)] public ushort m_sADICOFRegister;
        // Register_20~Register_375
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 355)] public Byte m_abyADICOFCfgReserved;
    };

    // 1024 byte regedit
    public struct RegCfgInfo
    {
        // System base Information：100,Register_1~Register_100
        public SysBaseInfo m_SysBaseInfo;
        // System Manufacture Information：50,Register_101~Register_150
        public SysManuInfo m_SysManuInfo;
        // System Status Information：28,Register_151~Register_178
        public SysStatusInfo m_SysStatusInfo;
        // Gigabit Ethernet Information：40,Register_179~Register_218
        public EtherInfo m_EtherInfo;
        // High Voltage Generator Interface Trigger Mode Information：21,Register_219~Register_239
        public HiVolTriggerModeInfo m_HiVolTriggerModeInfo;
        // System Configuration Information：127,Register_240~Register_366
        public SysCfgInfo m_SysCfgInfo;
        // Image Calibration Configuration：20,Register_367~Register_386
        public ImgCaliCfg m_ImgCaliCfg;
        // Voltage Adjust Configuration：48,Register_387~Register_434
        public VolAdjustCfg m_VolAdjustCfg;
        // TI COF Parameter Configuration：84,Register_435~Register_518
        public TICOFCfg m_TICOFCfg;
        // CMOS Parameter Configuration：116,Register_519~Register_634
        public CMOSCfg m_CMOSCfg;
        // ADI COF Parameter Configuration：390,Register_635~Register_1024
        public ADICOFCfg m_ADICOFCfg;
        ///////////////////////
    };


    public static class HBSDK
    {

        public static string GetErrorMsgByCode(int code) {

            switch ((HBIRETCODE)code)
            {
                case HBIRETCODE.HBI_SUCCSS: return "Success";
                case HBIRETCODE.HBI_ERR_OPEN_DETECTOR_FAILED: return "Open device driver failed";
                case HBIRETCODE.HBI_ERR_INVALID_PARAMS: return "Parameter error";
                case HBIRETCODE.HBI_ERR_CONNECT_FAILED: return "Connect failed";
                case HBIRETCODE.HBI_ERR_MALLOC_FAILED: return "Malloc memory failed";
                case HBIRETCODE.HBI_ERR_RELIMGMEM_FAILED: return "Releaseimagemem fail";
                case HBIRETCODE.HBI_ERR_RETIMGMEM_FAILED: return "ReturnImageMem fail";
                case HBIRETCODE.HBI_ERR_NODEVICE: return "No Init DLL Instance";
                case HBIRETCODE.HBI_ERR_NODEVICE_TRY_CONNECT: return "Disconnect status";
                case HBIRETCODE.HBI_ERR_DEVICE_BUSY: return "Fpd is busy";
                case HBIRETCODE.HBI_ERR_SENDDATA_FAILED: return "SendData failed";
                case HBIRETCODE.HBI_ERR_RECEIVE_DATA_FAILED: return "Receive Data failed";
                case HBIRETCODE.HBI_ERR_COMMAND_DISMATCH: return "Command dismatch";
                case HBIRETCODE.HBI_ERR_NO_IMAGE_RAW: return "No Image raw";
                case HBIRETCODE.HBI_ERR_PTHREAD_ACTIVE_FAILED: return "Pthread active failed";
                case HBIRETCODE.HBI_ERR_STOP_ACQUISITION: return "Pthread stop data acquisition failed";
                case HBIRETCODE.HBI_ERR_INSERT_FAILED: return "insert calibrate mode failed";
                case HBIRETCODE.HBI_ERR_GET_CFG_FAILED: return "get device config failed";
                case HBIRETCODE.HBI_NOT_SUPPORT: return "not surport yet";
                case HBIRETCODE.HBI_REGISTER_CALLBACK_FAILED: return "failed to register callback function";
                case HBIRETCODE.HBI_SEND_MESSAGE_FAILD: return "send message failed";
                case HBIRETCODE.HBI_ERR_WORKMODE: return "switch work mode failed";
                case HBIRETCODE.HBI_FAILED: return "operation failed";
                case HBIRETCODE.HBI_FILE_NOT_EXISTS: return "file does not exist";
                case HBIRETCODE.HBI_COMM_TYPE_ERR: return "communication is not exist";
                case HBIRETCODE.HBI_TYPE_IS_NOT_EXISTS: return "this type is not exists";
                case HBIRETCODE.HBI_SAVE_FILE_FAILED: return "save file failed";
                case HBIRETCODE.HBI_INIT_PARAM_FAILED: return "Init dll param failed";
                case HBIRETCODE.HBI_END: return "Exit monitoring";
                default:
                    return "Unknown error";
 }
        }


        /*********************************************************
        * 编    号: No001
        * 函 数 名: HBI_Init
        * 功能描述: 初始化动态库
        * 参数说明:
        * 返 回 值：void *
                    失败：NULL,成功：非空
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern UIntPtr HBI_Init();

        /*********************************************************
        * 编    号: No002
        * 函 数 名: HBI_Destroy
        * 功能描述: 释放动态库资源
        * 参数说明:
                In: UIntPtr - 句柄
                Out: 无
        * 返 回 值：void
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern void HBI_Destroy(UIntPtr handle);

        /*********************************************************
        * 编    号: No003
        * 函 数 名: HBI_RegEventCallBackFun
        * 功能描述: 注册回调函数
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                USER_CALLBACK_HANDLE_ENVENT handleEventfun - 注册回调函数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_RegEventCallBackFun(UIntPtr handle, USER_CALLBACK_HANDLE_ENVENT handleEventfun);

        /*********************************************************
        * 编    号: No004
        * 函 数 名: HBI_ConnectDetector
        * 功能描述: 建立连接
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    char *szRemoteIp - 平板IP地址,如192.168.10.40
                    unsigned short remotePort - 平板端口,如32897(0x8081)
                    char *szlocalIp - 上位机地址,如192.168.10.20
                    unsigned short localPort -上位机端口,如192.168.10.40
                    int offsetTemp - 0:连接成功后固件不重新做offset模板
                                     1:连接成功后固件重新做offset模板
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ConnectDetector(UIntPtr handle, string szRemoteIp, ushort remotePort, string szlocalIp, ushort localPort, int offsetTemp);

        /*********************************************************
        * 编    号: No005
        * 函 数 名: HBI_DisConnectDetector
        * 功能描述: 断开连接
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                SOFTWARE_CALIBRATE_ENABLE inEnable - 校正使能状态,见HbDllType.h中FPD_SOFTWARE_CALIBRATE_ENABLE
                Out: 无
        * 返 回 值：int
                    0   - 成功
                    非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_DisConnectDetector(UIntPtr handle);

        /*********************************************************
        * 编    号: No006
        * 函 数 名: HBI_LiveAcquisition
        * 功能描述: 连续采集
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    FPD_AQC_MODE _mode - 采集模式以及参数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_LiveAcquisition(UIntPtr handle, FPD_AQC_MODE _mode);

        /*********************************************************
        * 编    号: No007
        * 函 数 名: HBI_StopAcquisition
        * 功能描述: 停止采集
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备     注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_StopAcquisition(UIntPtr handle);

        /*********************************************************
        * 编    号: No008
        * 函 数 名: HBI_ResetFirmware
        * 功能描述: 重置固件出厂设置
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ResetFirmware(UIntPtr handle);// add by mhyang 20200821

        /*********************************************************
        * 编    号: No009
        * 函 数 名: HBI_ResetDetector
        * 功能描述: 重置探测器
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ResetDetector(UIntPtr handle);

        /*********************************************************
        * 编    号: No010
        * 函 数 名: HBI_GetSDKVerion
        * 功能描述: 获取SDK版本号
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: char *szVer
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSDKVerion(UIntPtr handle, StringBuilder szVer);

        /*********************************************************
        * 编    号: No011
        * 函 数 名: HBI_GetFirmareVerion
        * 功能描述: 获取固件版本号
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: char *szVer
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetFirmareVerion(UIntPtr handle, StringBuilder szFirmwareVer);

        /*********************************************************
        * 编    号: No012
        * 函 数 名: HBI_GetFPDSerialNumber
        * 功能描述: 获取产品序列号
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                In/Out: char *szSn,长度14位
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetFPDSerialNumber(UIntPtr handle, StringBuilder szSn);

        /*********************************************************
        * 编    号: No013
        * 函 数 名: HBI_GetError
        * 功能描述: 获取错误信息
        * 参数说明:
            In:  CodeStringTable* inTable - 错误信息信息列表
                 int count  - 信息列表个数
                 int recode - 错误码
            Out:无
        * 返 回 值：const char *
            非NULL - 成功，错误信息
            NULL   - 失败
        * 备    注:
        *********************************************************/
        //   [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        //   public static extern string  HBI_GetError(const CodeStringTable* inTable, int count, int recode);

        /*********************************************************
        * 编    号: No014
        * 函 数 名: HBI_GetDevCfgInfo
        * 功能描述: 获取ROM参数
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out:RegCfgInfo* pRegCfg,见HbDllType.h。
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetDevCfgInfo(UIntPtr handle, out RegCfgInfo pRegCfg);

        /*********************************************************
        * 编    号: No015
        * 函 数 名: HBI_GetImageProperty
        * 功能描述: 获取图像属性
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: IMAGE_PROPERTY *img_pro,见HbDllType.h。
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetImageProperty(UIntPtr handle, out IMAGE_PROPERTY img_pro);

        /*********************************************************
        * 编    号: No016
        * 函 数 名: HBI_Prepare
        * 功能描述: 准备指令
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        0   成功
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_Prepare(UIntPtr handle);

        /*********************************************************
        * 编    号: No017
        * 函 数 名: HBI_Dumping
        * 功能描述: 清空指令
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_Dumping(UIntPtr handle);

        /*********************************************************
        * 编    号: No018
        * 函 数 名: HBI_SingleAcquisition
        * 功能描述: 单帧采集
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    FPD_AQC_MODE _mode - 采集模式以及参数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SingleAcquisition(UIntPtr handle, FPD_AQC_MODE _mode);

        /*********************************************************
        * 编    号: No019
        * 函 数 名: HBI_TriggerAndCorrectApplay
        * 功能描述: 设置触发模式和图像校正使能（工作站）新版本
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int _triggerMode,1-软触发，3-高压触发，4-FreeAED。
                    IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:替换HBI_WorkStationInitDllCfg和HBI_QuckInitDllCfg接口
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_TriggerAndCorrectApplay(UIntPtr handle, int _triggerMode, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 编    号: No020
        * 函 数 名: HBI_UpdateTriggerMode
        * 功能描述: 设置触发模式
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int _triggerMode,1-软触发，3-高压触发，4-FreeAED。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_UpdateTriggerMode(UIntPtr handle, int _triggerMode);

        /*********************************************************
        * 编    号: No021
        * 函 数 名: HBI_UpdateCorrectEnable
        * 功能描述: 更新图像固件校正使能
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_UpdateCorrectEnable(UIntPtr handle, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 编    号: No022
        * 函 数 名: HBI_GetCorrectEnable
        * 功能描述: 获取图像固件校正使能
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetCorrectEnable(UIntPtr handle, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 编    号: No023
        * 函 数 名: HBI_SetGainMode
        * 功能描述: 设置增益模式
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int mode - 模式
                    [n]-Invalid
                    [1]-0.6pC
                    [2]-1.2pC
                    [3]-2.4pC
                    [4]-3.6pC
                    [5]-4.8pC
                    [6]-7.2pC,默认7.2pC
                    [7]-9.6pC
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetGainMode(UIntPtr handle, int nGainMode);

        /*********************************************************
        * 编    号: No024
        * 函 数 名: HBI_GetGainMode
        * 功能描述: 获取增益模式
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                    [n]-Invalid
                    [1]-0.6pC
                    [2]-1.2pC
                    [3]-2.4pC
                    [4]-3.6pC
                    [5]-4.8pC
                    [6]-7.2pC,默认7.2pC
                    [7]-9.6pC
        * 备    注:
        *********************************************************/
        public static extern int HBI_GetGainMode(UIntPtr handle);

        /*********************************************************
        * 编    号: No025
        * 函 数 名: HBI_SetAcqSpanTm
        * 功能描述: 设置采集时间间隔
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int time - 间隔时间,单位是毫秒ms，>= 1000ms
                Out: 无
        * 返 回 值：int
                    0   - 成功
                    非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetAcqSpanTm(UIntPtr handle, int time);

        /*********************************************************
        * 编    号: No026
        * 函 数 名: HBI_GetAcqSpanTm
        * 功能描述: 获取采集时间间隔
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_time - 时间,单位是毫秒ms
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetAcqSpanTm(UIntPtr handle, out int out_time);

        /*********************************************************
        * 编    号: No027
        * 函 数 名: HBI_SetBinning
        * 功能描述: 设置bingning方式
        * 参数说明:
                In:
                 UIntPtr - 句柄(无符号指针)
                 unsigned char bytebin - 1:1x1,2:2x2,3:3x3(暂不支持),4:4x4，其他不支持
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetBinning(UIntPtr handle, Byte bin);

        /*********************************************************
        * 编    号: No028
        * 函 数 名: HBI_GetBinning
        * 功能描述: 获取bingning方式
        * 参数说明:
                In:
                 UIntPtr - 句柄(无符号指针)
                unsigned char *bin - 0:无效，1:1*1，2:2*2，3:3*3, 4:4*4，
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetBinning(UIntPtr handle, out Byte bin);

        /*********************************************************
        * 编    号: No029
        * 函 数 名: HBI_SetRawStyle
        * 功能描述: 设置是否保存图像以及图像文件形式
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    bool bsave - 保存拟或显示,false:显示不保存，true:保存不显示
                    bool bsingleraw - 报存在单个文件或多个文件,false:1帧数据保存1个文件，true:多帧数据可保存在一个文件中
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetRawStyle(UIntPtr handle, bool bsave, bool bsingleraw = false);

        /*********************************************************
        * 编    号: No030
        * 函 数 名: HBI_SetAqcProperty
        * 功能描述: 设置采集属性
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    FPD_AQC_MODE _mode - 采集模式以及参数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetAqcProperty(UIntPtr handle, FPD_AQC_MODE _mode);

        /*********************************************************
        * 编    号: No031
        * 函 数 名: HBI_ForceStopAcquisition
        * 功能描述: 强制停止采集
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ForceStopAcquisition(UIntPtr handle);

        /*********************************************************
        * 编    号: No032
        * 函 数 名: HBI_RetransMissPacket
        * 功能描述: 单包丢包重传
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_RetransMissPacket(UIntPtr handle);

        /*********************************************************
        * 编    号: No033
        * 函 数 名: HBI_RetransMissFrame
        * 功能描述: 整帧重传
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_RetransMissFrame(UIntPtr handle);

        /*********************************************************
        * 编    号: No034
        * 函 数 名: HBI_SetSysParamCfg
        * 功能描述: 配置系统RAM/ROM参数
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            int cmd -
            int type -
            RegCfgInfo* pRegCfg -
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSysParamCfg(UIntPtr handle, int cmd, int type, ref RegCfgInfo pRegCfg);

        /*********************************************************
        * 编    号: No035
        * 函 数 名: HBI_GetSysParamCfg
        * 功能描述: 回读系统RAM/ROM参数,异步事件，在参数在回调函数中反馈
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            int cmd  - rom or ram
            int type - user or admin
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSysParamCfg(UIntPtr handle, int cmd, int type);

        /*********************************************************
        * 编    号: No036
        * 函 数 名: HBI_RegProgressCallBack
        * 功能描述: 注册回调函数
        * 参数说明: 处理固件升级反馈信息
                In: UIntPtr - 句柄(无符号指针)
                    USER_CALLBACK_HANDLE_ENVENT handleStatusfun - 注册回调函数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_RegProgressCallBack(UIntPtr handle, USER_CALLBACK_HANDLE_PROCESS handleStatusfun, UIntPtr _Object);

        /*********************************************************
        * 编    号: No037
        * 函 数 名: HBI_InitOffsetMode
        * 功能描述: 初始化暗场模板
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                CALIBRATE_INPUT_PARAM calibrate_param,见HbDllType.h。
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InitOffsetMode(UIntPtr handle, CALIBRATE_INPUT_PARAM calibrate_param);

        /*********************************************************
        * 编    号: No038
        * 函 数 名: HBI_InsertOffsetMode
        * 功能描述: 向gain矫正模型中插入数据
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                int group_id - 组ID
                char *filepath - 文件路径
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InsertOffsetMode(UIntPtr handle, int group_id, string filepath);

        /*********************************************************
        * 编    号: No039
        * 函 数 名: HBI_ClearOffsetMode
        * 功能描述: 清空offset矫正模型
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ClearOffsetMode(UIntPtr handle);

        /*********************************************************
        * 编    号: No040
        * 函 数 名: HBI_GenerateOffsetTemp
        * 功能描述: 生成defect模板
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                int raw_num - 暗场图个数
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        public static extern int HBI_GenerateOffsetTemp(UIntPtr handle, int raw_num);

        /*********************************************************
        * 编    号: No041
        * 函 数 名: HBI_InitGainMode
        * 功能描述: 初始化gain矫正模型
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                CALIBRATE_INPUT_PARAM calibrate_param - 矫正参数
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InitGainMode(UIntPtr handle, CALIBRATE_INPUT_PARAM calibrate_param);

        /*********************************************************
        * 编    号: No042
        * 函 数 名: HBI_InsertGainMode
        * 功能描述: 向gain矫正模型中插入数据
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            int group_id - 组ID
            char *filepath - 文件路径
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InsertGainMode(UIntPtr handle, int group_id, string filepath);

        /*********************************************************
        * 编    号: No043
        * 函 数 名: HBI_ClearGainMode
        * 功能描述: 清空gain矫正模型
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ClearGainMode(UIntPtr handle);

        /*********************************************************
        * 编    号: No044
        * 函 数 名: HBI_GenerateGainTemp
        * 功能描述: 生成defect模板
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                int group_sum - 组数
                int per_group_num - 每组包含图片个数
                Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GenerateGainTemp(UIntPtr handle, int group_sum, int per_group_num);

        /*********************************************************
        * 编    号: No045
        * 函 数 名: HBI_InitDefectMode
        * 功能描述: 初始化defect矫正模型
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                CALIBRATE_INPUT_PARAM calibrate_param - 矫正参数
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InitDefectMode(UIntPtr handle, CALIBRATE_INPUT_PARAM calibrate_param);

        /*********************************************************
        * 编    号: No046
        * 函 数 名: HBI_InsertDefectMode
        * 功能描述: 向defect矫正模型中插入数据
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                int group_id - 组ID
                char *filepath - 文件路径
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_InsertDefectMode(UIntPtr handle, int group_id, string filepath);

        /*********************************************************
        * 编    号: No047
        * 函 数 名: HBI_ClearDefectMode
        * 功能描述: 清空defect矫正模型
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_ClearDefectMode(UIntPtr handle);

        /*********************************************************
        * 编    号: No048
        * 函 数 名: HBI_GenerateDefectTemp
        * 功能描述: 生成defect模板
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            int group_sum - 组数
            int per_group_num - 每组包含图片个数
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GenerateDefectTemp(UIntPtr handle, int group_sum, int per_group_num);

        /*********************************************************
        * 编    号: No049
        * 函 数 名: HBI_DownLoadFileToFireware
        * 功能描述: 下发模板
        * 参数说明:

        Out: 无
        * 返 回 值：int
        0   - 成功
        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_DownLoadFileToFireware(UIntPtr handle, ref DOWNLOAD_T_FILE_MODE_ST dparam);

        /*********************************************************
        * 编    号: No50
        * 函 数 名: HBI_GenTemplateByOnce
        * 功能描述: 初始化暗场模板
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                CALIBRATE_INPUT_PARAM calibrate_param,见HbDllType.h。
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_QuickGenTemplate(UIntPtr handle, CALIBRATE_INPUT_PARAM calibrate_param, FPD_AQC_MODE _mode);

        /*********************************************************
        * 编    号: No51
        * 函 数 名: HBI_InsertDefectBadPixel
        * 功能描述: 手动添加坏点
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                 POINT pt 当前像素点 ,见HbDllType.h。
                  int type 类型：0：当前单个像素，1:整行;2、整列
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        //  [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        //  public static extern bool  HBI_InsertDefectBadPixel(UIntPtr handle, BadPixel_T pt, eBadPixel_T type);

        /*********************************************************
        * 编    号: No052
        * 函 数 名: HBI_SetEnableCalibrate
        * 功能描述: 设置校正使能
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    SOFTWARE_CALIBRATE_ENABLE inEnable - 校正使能状态,见HbDllType.h中SOFTWARE_CALIBRATE_ENABLE struct
                Out: 无
        * 返 回 值：int
                0   成功
                非0 失败，见HbDllError.h错误码
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetEnableCalibrate(UIntPtr handle, SOFTWARE_CALIBRATE_ENABLE inEnable);

        /*********************************************************
        * 编    号: No053
        * 函 数 名: HBI_GetEnableCalibrate
        * 功能描述: 获取校正使能
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                SOFTWARE_CALIBRATE_ENABLE inEnable - 校正使能状态,见HbDllType.h中
                                                     SOFTWARE_CALIBRATE_ENABLE struct
            Out: 无
        * 返 回 值：int
                0   成功
                非0 失败，见HbDllError.h错误码
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetEnableCalibrate(UIntPtr handle, out SOFTWARE_CALIBRATE_ENABLE inEnable);

        /*********************************************************
        * 编    号: No054
        * 函 数 名: HBI_SetSystemManufactureInfo
        * 功能描述: Set System Manufacture Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                SysManuInfo* pSysManuInfo,见HbDllType.h。
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSystemManufactureInfo(UIntPtr handle, SysManuInfo pSysManuInfo);

        /*********************************************************
        * 编    号: No055
        * 函 数 名: HBI_SetSystemStatus
        * 功能描述: Set System Status Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                SysStatusInfo* pSysStatus,见HbDllType.h。
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSystemStatus(UIntPtr handle, out SysStatusInfo pSysStatus);

        /*********************************************************
        * 编    号: No056
        * 函 数 名: HBI_GetSystemManufactureInfo
        * 功能描述: Get System Manufacture Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out:SysManuInfo* pSysManuInfo,见HbDllType.h。
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSystemManufactureInfo(UIntPtr handle, out SysManuInfo pSysManuInfo);

        /*********************************************************
        * 编    号: No057
        * 函 数 名: HBI_GetSystemStatus
        * 功能描述: Get System Status Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out:SysStatusInfo* pSysStatus,见HbDllType.h。
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSystemStatus(UIntPtr handle, out SysStatusInfo pSysStatus);

        /*********************************************************
        * 编    号: No058
        * 函 数 名: HBI_SetGigabitEther
        * 功能描述: 设置网络信息参数
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                EtherInfo* pEther,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetGigabitEther(UIntPtr handle, EtherInfo pEther);

        /*********************************************************
        * 编    号: No059
        * 函 数 名: HBI_SetHvgTriggerMode
        * 功能描述: Sety High Voltage Generator Interface Trigger Mode Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                HiVolTriggerModeInfo* pHvgTrigMode,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetHvgTriggerMode(UIntPtr handle, HiVolTriggerModeInfo pHvgTrigMode);

        /*********************************************************
        * 编    号: No060
        * 函 数 名: HBI_SetSystemConfig
        * 功能描述: Set System Configuration Information
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                SysCfgInfo* pSysCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSystemConfig(UIntPtr handle, SysCfgInfo pSysCfg);

        /*********************************************************
        * 编    号: No061
        * 函 数 名: HBI_SetImageCalibration
        * 功能描述: 设置ROM Image firmware calibrate使能
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                ImgCaliCfg* pImgCaliCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetImageCalibration(UIntPtr handle, ImgCaliCfg pImgCaliCfg);

        /*********************************************************
        * 编    号: No062
        * 函 数 名: HBI_SetVoltageAdjustConfig
        * 功能描述: Set Voltage Adjust Configuration.
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                VolAdjustCfg* pVolAdjustCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetVoltageAdjustConfig(UIntPtr handle, VolAdjustCfg pVolAdjustCfg);

        /*********************************************************
        * 编    号: No063
        * 函 数 名: HBI_SetTICOFConfig
        * 功能描述: Set TI COF Parameter Configuration.
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                TICOFCfg* pTICOFCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/

        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetTICOFConfig(UIntPtr handle, TICOFCfg pTICOFCfg);

        /*********************************************************
        * 编    号: No064
        * 函 数 名: HBI_SetCMOSConfig
        * 功能描述: Set CMOS Parameter Configuration.
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                CMOSCfg* pCMOSCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetCMOSConfig(UIntPtr handle, CMOSCfg pCMOSCfg);

        /*********************************************************
        * 编    号: No065
        * 函 数 名: HBI_SetADICOFConfig
        * 功能描述: Set ADI COF Parameter Configuration.
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                ADICOFCfg* pADICOFCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetADICOFConfig(UIntPtr handle, ADICOFCfg pADICOFCfg);

        /*********************************************************
        * 编    号: No066
        * 函 数 名: HBI_GetWiffyProperty
        * 功能描述: 获取无线属性
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
            Out: void *pWiffy - 暂不支持
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetWiffyProperty(UIntPtr handle, UIntPtr pWiffy);

        /*********************************************************
        * 编    号: No067
        * 函 数 名: HBI_WorkStationInitDllCfg
        * 功能描述: 初始化参数（工作站）
        * 参数说明:
            In: UIntPtr - 句柄(无符号指针)
                 SysCfgInfo* pSysCfg,见HbDllType.h。
                 ImgCaliCfg* pFirmwareCaliCfg,见HbDllType.h。
                 SOFTWARE_CALIBRATE_ENABLE* pSoftwareCaliCfg,见HbDllType.h。
            Out:无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_WorkStationInitDllCfg(UIntPtr handle, SysCfgInfo pSysCfg, ImgCaliCfg pFirmwareCaliCfg, SOFTWARE_CALIBRATE_ENABLE pSoftwareCaliCfg);

        /*********************************************************
        * 编    号: No068
        * 函 数 名: HBI_QuckInitDllCfg
        * 功能描述: 快速初始化参数（工作站）旧版本
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int _triggerMode,1-软触发，3-高压触发，4-FreeAED。
                    ImgCaliCfg* pFirmwareCaliCfg,见HbDllType.h。
                    SOFTWARE_CALIBRATE_ENABLE* pSoftwareCaliCfg,见HbDllType.h。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:替换HBI_WorkStationInitDllCfg接口
        *********************************************************/
        //  [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        // public static extern int  HBI_QuckInitDllCfg(UIntPtr handle, int _triggerMode, ImgCaliCfg* pFirmwareCaliCfg, SOFTWARE_CALIBRATE_ENABLE* pSoftwareCaliCfg);

        /*********************************************************
        * 编    号: No069
        * 函 数 名: HBI_DownloadTemplate
        * 功能描述: 下载矫正模板
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    HBIDOWNLOAD_FILE *uploadmode - 上传模型对象指针
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_DownloadTemplate(UIntPtr handle, HBIDOWNLOAD_FILE downloadfile);

        /*********************************************************
        * 编    号: No070
        * 函 数 名: HBI_SetSinglePrepareTime
        * 功能描述: 设置软触发单帧采集清空和采集之间的时间间隔
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int *in_itime - 时间 [0~10000] 单位:mm
                        0-表示软触发单帧采集先HBI_Prepare后HBI_SingleAcquisition完成单帧采集
                        非0-表示软触发单帧采集，只需HBI_Prepare即可按照预定时间完成单帧采集
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSinglePrepareTime(UIntPtr handle, int in_itime);

        /*********************************************************
        * 编    号: No071
        * 函 数 名: HBI_GetSinglePrepareTime
        * 功能描述: 获取软触发单帧采集清空和采集之间的时间间隔
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_itime - 时间 [0~10000] 单位:mm
                        0-表示软触发单帧采集先HBI_Prepare后HBI_SingleAcquisition完成单帧采集
                        非0-表示软触发单帧采集，只需HBI_Prepare即可按照预定时间完成单帧采集
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSinglePrepareTime(UIntPtr handle, out int out_itime);

        /*********************************************************
        * 编    号: No072
        * 函 数 名: HBI_SetClippingValue
        * 功能描述: 设置图像剪裁值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_ivalue - 剪裁值 [0~65,535]
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetClippingValue(UIntPtr handle, int in_ivalue);

        /*********************************************************
        * 编    号: No073
        * 函 数 名: HBI_GetClippingValue
        * 功能描述: 获取图像剪裁值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_ivalue - 剪裁值 [0~65,535]
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetClippingValue(UIntPtr handle, out int out_ivalue);

        /*********************************************************
        * 编    号: No074
        * 函 数 名: HBI_SetAedThreshold
        * 功能描述: 设置AED阈值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                int out_ivalue - 阈值 [25,000~1,000,000]
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetAedThreshold(UIntPtr handle, int in_ivalue);

        /*********************************************************
        * 编    号: No075
        * 函 数 名: HBI_GetAedThreshold
        * 功能描述: 获取AED阈值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_ivalue - 阈值 [10,000~1,000,000]
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetAedThreshold(UIntPtr handle, out int out_ivalue);

        /*********************************************************
        * 编    号: No076
        * 函 数 名: HBI_SetSaturationValue
        * 功能描述: 设置饱和值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                out:int *out_ivalue - 饱和值 [0~65,535]
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_SetSaturationValue(UIntPtr handle, int in_ivalue);

        /*********************************************************
        * 编    号: No077
        * 函 数 名: HBI_GetSaturationValue
        * 功能描述: 获取饱和值
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    int *out_ivalue - 饱和值 [0~65,535]
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern int HBI_GetSaturationValue(UIntPtr handle, out int out_ivalue);

        /*********************************************************
        * 编    号: No078
        * 函 数 名: HBI_GetCurTemplatePath
        * 功能描述: 获取当前模板的(绝对)路径
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
                    emDOWNLOAD_FILE_TYPE enumType - 模板类型
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern string HBI_GetCurTemplatePath(UIntPtr handle, DOWNLOAD_FILE_TYPE enumType);

        /*********************************************************
        * 编    号: No079
        * 函 数 名: HBI_GetCurTemplateDir
        * 功能描述: 获取当前模板的(绝对)目录
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
        * 返 回 值：char *
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern string HBI_GetCurTemplateDir(UIntPtr handle);

        /*********************************************************
        * 编    号: No080
        * 函 数 名: HBI_GetCurImageDir
        * 功能描述: 获取当前图像数据目录
        * 参数说明:
                In: UIntPtr - 句柄(无符号指针)
        * 返 回 值：char *
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
        public static extern string HBI_GetCurImageDir(UIntPtr handle);

    }

}


