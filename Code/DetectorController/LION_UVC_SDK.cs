using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


namespace Detector
{
    using LU_UINT16 = System.UInt16;
    using LU_RESULT = System.Int32;
    using LU_UINT32 = System.UInt32;
    #region enum

    //设备状态
    public enum LUDEV_STATE
    {
        LUDEVSTATE_UNOPNE = 1,
        LUDEVSTATE_OPEN,
        LUDEVSTATE_WAITTRIGGER,
        LUDEVSTATE_TRIGGERIMAGE,
        LUDEVSTATE_IMAGESAVE,
        LUDEVSTATE_OVERTIME,
    };


    //出图模式
    public enum LU_MODE
    {
        LUMODE_AC,          //AC
        LUMODE_UTC,         //UTC
    };

    //BINNING
    public enum LUDEV_BINNING
    {
        LUDEVBINNING_NO,        //NO
        LUDEVBINNING_YES,       //Yes
    };

    //FPGA Filter
    public enum LUDEV_FILTER
    {
        LUDEVFILTER_NO,         //No
        LUDEVFILTER_YES,        //Yes
    };

    //图片格式
    public enum LUIMG_TYPE
    {
        LUIMG_RAW = 1,          //raw
        LUIMG_BMP,              //bmp
        LUIMG_JPEG,             //jpeg
        LUIMG_PNG,              //png
    };

    //设备参数
    public enum LUDEV_PARAM
    {
        LUDEVPARAM_UNKNOWN = 0,                 //
        LUDEVPARAM_MODE = 1,                    //出图模式
        LUDEVPARAM_BINNING,                     //BINNING     //暂不支持
        LUDEVPARAM_FILTER,                      //FPGA进行坏点过滤
        LUDEVPARAM_XRAY,                        //暂不支持
        LUDEVPARAM_IMAGE,                       //图片格式    //暂不支持
        LUDEVPARAM_DELAYTIME,                   //延迟时间 ms单位  //暂不支持
        LUDEVPARAM_TRIGGERTIME,                 //触发超时设置 s单位  AC=5s VTC=60S
        LUDEVPARAM_READIMAGETIME,               //读图超时设置 s秒	  5s
        LUDEVPARAM_XRESOLUTION,                 //暂不支持
        LUDEVPARAM_YRESOLUTION,                 //暂不支持
        LUDEVPARAM_IMGWIDTH,                    //图片宽度      //暂不支持   
        LUDEVPARAM_IMGHEIGHT,                   //图片高宽      //暂不支持
        LUDEVPARAM_SAMPLESPERPIXEL,             //暂不支持
        LUDEVPARAM_BITESPERPIXEL,               //暂不支持
        LUDEVPARAM_BITSPERSAMPLE,               //暂不支持
        LUDEVPARAM_PLANAR,                      //暂不支持
        LUDEVPARAM_PIXELTYPE,                   //暂不支持
        LUDEVPARAM_COMPRESSION,                 //暂不支持 
        LUDEVPARAM_BRIGHTNESS,                  //亮度	0 - 100      //暂不支持
        LUDEVPARAM_CONTRAST,                    //对比度	0 - 100   //暂不支持
        LUDEVPARAM_SATURATION,                  //饱和度	0 - 100   //暂不支持
        LUDEVPARAM_SHARPEN,                     //锐化		0 - 100   //暂不支持
        LUDEVPARAM_EMBOSS,                      //浮雕		0 - 100   //暂不支持
        LUDEVPARAM_BLUR,                        //模糊    0 - 100      //暂不支持
        LUDEVPARAM_GAMMA,                       //直方图		0 - 100    //暂不支持
        LUDEVPARAM_FFCDARKENABLE,               //暗场FFC校准使能          //暂不支持
        LUDEVPARAM_FFCLIGHTENABLE,              //明场FFC校准使能          //暂不支持
        LUDEVPARAM_FCCDARK,                     //暗场FFC校准     文件路径   //暂不支持
        LUDEVPARAM_FCCLIGHT,                    //明场FFC校准		文件路径  //暂不支持
    };

    //TWPARAMETER  //暂不支持
    public enum TWDEV_PARAM
    {
        TW_CAP_UNKONWN = 0,
        TW_CAP_CUSTOMBASE = 0x8000,                  /* Base of custom capabilities */
                                                     /* all data sources are REQUIRED to support these caps */
        TW_CAP_XFERCOUNT = 0x0001,
        /* image data sources are REQUIRED to support these caps */
        TW_ICAP_COMPRESSION = 0x0100,
        TW_ICAP_PIXELTYPE = 0x0101,
        TW_ICAP_UNITS = 0x0102,
        TW_ICAP_XFERMECH = 0x0103,

        /* all data sources MAY support these caps */
        TW_CAP_AUTHOR = 0x1000,
        TW_CAP_CAPTION = 0x1001,
        TW_CAP_FEEDERENABLED = 0x1002,
        TW_CAP_FEEDERLOADED = 0x1003,
        TW_CAP_TIMEDATE = 0x1004,
        TW_CAP_SUPPORTEDCAPS = 0x1005,
        TW_CAP_EXTENDEDCAPS = 0x1006,
        TW_CAP_AUTOFEED = 0x1007,
        TW_CAP_CLEARPAGE = 0x1008,
        TW_CAP_FEEDPAGE = 0x1009,
        TW_CAP_REWINDPAGE = 0x100a,
        TW_CAP_INDICATORS = 0x100b,
        TW_CAP_PAPERDETECTABLE = 0x100d,
        TW_CAP_UICONTROLLABLE = 0x100e,
        TW_CAP_DEVICEONLINE = 0x100f,
        TW_CAP_AUTOSCAN = 0x1010,
        TW_CAP_THUMBNAILSENABLED = 0x1011,
        TW_CAP_DUPLEX = 0x1012,
        TW_CAP_DUPLEXENABLED = 0x1013,
        TW_CAP_ENABLEDSUIONLY = 0x1014,
        TW_CAP_CUSTOMDSDATA = 0x1015,
        TW_CAP_ENDORSER = 0x1016,
        TW_CAP_JOBCONTROL = 0x1017,
        TW_CAP_ALARMS = 0x1018,
        TW_CAP_ALARMVOLUME = 0x1019,
        TW_CAP_AUTOMATICCAPTURE = 0x101a,
        TW_CAP_TIMEBEFOREFIRSTCAPTURE = 0x101b,
        TW_CAP_TIMEBETWEENCAPTURES = 0x101c,
        TW_CAP_MAXBATCHBUFFERS = 0x101e,
        TW_CAP_DEVICETIMEDATE = 0x101f,
        TW_CAP_POWERSUPPLY = 0x1020,
        TW_CAP_CAMERAPREVIEWUI = 0x1021,
        TW_CAP_DEVICEEVENT = 0x1022,
        TW_CAP_SERIALNUMBER = 0x1024,
        TW_CAP_PRINTER = 0x1026,
        TW_CAP_PRINTERENABLED = 0x1027,
        TW_CAP_PRINTERINDEX = 0x1028,
        TW_CAP_PRINTERMODE = 0x1029,
        TW_CAP_PRINTERSTRING = 0x102a,
        TW_CAP_PRINTERSUFFIX = 0x102b,
        TW_CAP_LANGUAGE = 0x102c,
        TW_CAP_FEEDERALIGNMENT = 0x102d,
        TW_CAP_FEEDERORDER = 0x102e,
        TW_CAP_REACQUIREALLOWED = 0x1030,
        TW_CAP_BATTERYMINUTES = 0x1032,
        TW_CAP_BATTERYPERCENTAGE = 0x1033,
        TW_CAP_CAMERASIDE = 0x1034,
        TW_CAP_SEGMENTED = 0x1035,
        TW_CAP_CAMERAENABLED = 0x1036,
        TW_CAP_CAMERAORDER = 0x1037,
        TW_CAP_MICRENABLED = 0x1038,
        TW_CAP_FEEDERPREP = 0x1039,
        TW_CAP_FEEDERPOCKET = 0x103a,
        TW_CAP_AUTOMATICSENSEMEDIUM = 0x103b,
        TW_CAP_CUSTOMINTERFACEGUID = 0x103c,
        TW_CAP_SUPPORTEDCAPSSEGMENTUNIQUE = 0x103d,
        TW_CAP_SUPPORTEDDATS = 0x103e,
        TW_CAP_DOUBLEFEEDDETECTION = 0x103f,
        TW_CAP_DOUBLEFEEDDETECTIONLENGTH = 0x1040,
        TW_CAP_DOUBLEFEEDDETECTIONSENSITIVITY = 0x1041,
        TW_CAP_DOUBLEFEEDDETECTIONRESPONSE = 0x1042,
        TW_CAP_PAPERHANDLING = 0x1043,
        TW_CAP_INDICATORSMODE = 0x1044,
        TW_CAP_PRINTERVERTICALOFFSET = 0x1045,
        TW_CAP_POWERSAVETIME = 0x1046,
        TW_CAP_PRINTERCHARROTATION = 0x1047,
        TW_CAP_PRINTERFONTSTYLE = 0x1048,
        TW_CAP_PRINTERINDEXLEADCHAR = 0x1049,
        TW_CAP_PRINTERINDEXMAXVALUE = 0x104A,
        TW_CAP_PRINTERINDEXNUMDIGITS = 0x104B,
        TW_CAP_PRINTERINDEXSTEP = 0x104C,
        TW_CAP_PRINTERINDEXTRIGGER = 0x104D,
        TW_CAP_PRINTERSTRINGPREVIEW = 0x104E,
        TW_CAP_SHEETCOUNT = 0x104F,

        /* image data sources MAY support these caps */
        TW_ICAP_AUTOBRIGHT = 0x1100,
        TW_ICAP_BRIGHTNESS = 0x1101,
        TW_ICAP_CONTRAST = 0x1103,
        TW_ICAP_CUSTHALFTONE = 0x1104,
        TW_ICAP_EXPOSURETIME = 0x1105,
        TW_ICAP_FILTER = 0x1106,
        TW_ICAP_FLASHUSED = 0x1107,
        TW_ICAP_GAMMA = 0x1108,
        TW_ICAP_HALFTONES = 0x1109,
        TW_ICAP_HIGHLIGHT = 0x110a,
        TW_ICAP_IMAGEFILEFORMAT = 0x110c,
        TW_ICAP_LAMPSTATE = 0x110d,
        TW_ICAP_LIGHTSOURCE = 0x110e,
        TW_ICAP_ORIENTATION = 0x1110,
        TW_ICAP_PHYSICALWIDTH = 0x1111,
        TW_ICAP_PHYSICALHEIGHT = 0x1112,
        TW_ICAP_SHADOW = 0x1113,
        TW_ICAP_FRAMES = 0x1114,
        TW_ICAP_XNATIVERESOLUTION = 0x1116,
        TW_ICAP_YNATIVERESOLUTION = 0x1117,
        TW_ICAP_XRESOLUTION = 0x1118,
        TW_ICAP_YRESOLUTION = 0x1119,
        TW_ICAP_MAXFRAMES = 0x111a,
        TW_ICAP_TILES = 0x111b,
        TW_ICAP_BITORDER = 0x111c,
        TW_ICAP_CCITTKFACTOR = 0x111d,
        TW_ICAP_LIGHTPATH = 0x111e,
        TW_ICAP_PIXELFLAVOR = 0x111f,
        TW_ICAP_PLANARCHUNKY = 0x1120,
        TW_ICAP_ROTATION = 0x1121,
        TW_ICAP_SUPPORTEDSIZES = 0x1122,
        TW_ICAP_THRESHOLD = 0x1123,
        TW_ICAP_XSCALING = 0x1124,
        TW_ICAP_YSCALING = 0x1125,
        TW_ICAP_BITORDERCODES = 0x1126,
        TW_ICAP_PIXELFLAVORCODES = 0x1127,
        TW_ICAP_JPEGPIXELTYPE = 0x1128,
        TW_ICAP_TIMEFILL = 0x112a,
        TW_ICAP_BITDEPTH = 0x112b,
        TW_ICAP_BITDEPTHREDUCTION = 0x112c,
        TW_ICAP_UNDEFINEDIMAGESIZE = 0x112d,
        TW_ICAP_IMAGEDATASET = 0x112e,
        TW_ICAP_EXTIMAGEINFO = 0x112f,
        TW_ICAP_MINIMUMHEIGHT = 0x1130,
        TW_ICAP_MINIMUMWIDTH = 0x1131,
        TW_ICAP_AUTODISCARDBLANKPAGES = 0x1134,
        TW_ICAP_FLIPROTATION = 0x1136,
        TW_ICAP_BARCODEDETECTIONENABLED = 0x1137,
        TW_ICAP_SUPPORTEDBARCODETYPES = 0x1138,
        TW_ICAP_BARCODEMAXSEARCHPRIORITIES = 0x1139,
        TW_ICAP_BARCODESEARCHPRIORITIES = 0x113a,
        TW_ICAP_BARCODESEARCHMODE = 0x113b,
        TW_ICAP_BARCODEMAXRETRIES = 0x113c,
        TW_ICAP_BARCODETIMEOUT = 0x113d,
        TW_ICAP_ZOOMFACTOR = 0x113e,
        TW_ICAP_PATCHCODEDETECTIONENABLED = 0x113f,
        TW_ICAP_SUPPORTEDPATCHCODETYPES = 0x1140,
        TW_ICAP_PATCHCODEMAXSEARCHPRIORITIES = 0x1141,
        TW_ICAP_PATCHCODESEARCHPRIORITIES = 0x1142,
        TW_ICAP_PATCHCODESEARCHMODE = 0x1143,
        TW_ICAP_PATCHCODEMAXRETRIES = 0x1144,
        TW_ICAP_PATCHCODETIMEOUT = 0x1145,
        TW_ICAP_FLASHUSED2 = 0x1146,
        TW_ICAP_IMAGEFILTER = 0x1147,
        TW_ICAP_NOISEFILTER = 0x1148,
        TW_ICAP_OVERSCAN = 0x1149,
        TW_ICAP_AUTOMATICBORDERDETECTION = 0x1150,
        TW_ICAP_AUTOMATICDESKEW = 0x1151,
        TW_ICAP_AUTOMATICROTATE = 0x1152,
        TW_ICAP_JPEGQUALITY = 0x1153,
        TW_ICAP_FEEDERTYPE = 0x1154,
        TW_ICAP_ICCPROFILE = 0x1155,
        TW_ICAP_AUTOSIZE = 0x1156,
        TW_ICAP_AUTOMATICCROPUSESFRAME = 0x1157,
        TW_ICAP_AUTOMATICLENGTHDETECTION = 0x1158,
        TW_ICAP_AUTOMATICCOLORENABLED = 0x1159,
        TW_ICAP_AUTOMATICCOLORNONCOLORPIXELTYPE = 0x115a,
        TW_ICAP_COLORMANAGEMENTENABLED = 0x115b,
        TW_ICAP_IMAGEMERGE = 0x115c,
        TW_ICAP_IMAGEMERGEHEIGHTTHRESHOLD = 0x115d,
        TW_ICAP_SUPPORTEDEXTIMAGEINFO = 0x115e,
        TW_ICAP_FILMTYPE = 0x115f,
        TW_ICAP_MIRROR = 0x1160,
        TW_ICAP_JPEGSUBSAMPLING = 0x1161,

        /* image data sources MAY support these audio caps */
        TW_ACAP_XFERMECH = 0x1202,
    };
    #endregion enum

    #region struct
    [StructLayout(LayoutKind.Sequential)]
    public struct LU_VERSION
    {
        LU_UINT16 MajorNum;
        LU_UINT16 MinorNum;
        LU_UINT16 Language;
        LU_UINT16 Country;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] Info;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TWD_IDENTITY
    {
        LU_UINT32 Id;
        LU_VERSION Version;
        LU_UINT16 ProtocolMajor;
        LU_UINT16 ProtocolMinor;
        LU_UINT32 SupportedGroups;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        char[] Manufacturer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        char[] ProductFamily;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        char[] ProductName;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct LU_IDENTITY
    {
        LU_UINT32 Id;                   //设备ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        char[] Name;             //设备名称
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        char[] DevSerial;         //设备序列号
        LU_UINT16 MajorNum;         //固件版本高位
        LU_UINT16 MinorNum;			//固件版本低位
    };
    public struct LU_DEVICE
    {
        LU_IDENTITY uvcIdentity;
        TWD_IDENTITY twIdentity;
    };
    //参数
    public struct LU_PARAM
    {

    LU_UINT16 param;        //参数 ,开方式参数形式，用于兼容
    LU_UINT16 type;         //存储空间数据类型, data实际数据类型为int, short, char.....
    LU_UINT32 size;         //存储空间大小,byte个数
    IntPtr data;			//存储空间，用于存储实际参数值
    };

    #endregion struct


    #region function
    class LION_UVC_SDK
    {
        /*********************************************************************************************************************
         * @brief 获取设备个数
         *
         * @param[in] enumLion 枚举设备类型，1: uvc, 0: twain
         * @return 返回设备个数
        *********************************************************************************************************************/
        [DllImport("LionSDK.dll", EntryPoint = "GetDeviceCount", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern LU_RESULT GetDeviceCount(LU_UINT32 enumLion = 1);


        /*********************************************************************************************************************
        * @brief 获取设备
        * 
        * @param[in] index 设备索引 0, 1, 2, 3....
        * @param[in] enumLion 枚举设备类型，1: uvc, 0: twain
        * @return 返回设备
       *********************************************************************************************************************/
        [DllImport("LionSDK.dll", EntryPoint = "GetDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ref LU_DEVICE GetDevice(LU_UINT32 index, LU_UINT32 enumLion = 1);

        /*********************************************************************************************************************
      * @brief 打开设备
      *
      * @param[in] pDevice需要打开的设备路径
      * @return LU_SUCCESS：成功，其他：失败
     *********************************************************************************************************************/
        [DllImport("LionSDK.dll", EntryPoint = "OpenDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern LU_RESULT OpenDevice(IntPtr pDevice);


        /*********************************************************************************************************************
     * @brief 关闭设备
     *
     * @param[in] pDevice需要关闭的设备路径
     * @return LU_SUCCESS：成功，其他：失败
    *********************************************************************************************************************/
        [DllImport("LionSDK.dll", EntryPoint = "CloseDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern LU_RESULT CloseDevice(IntPtr pDevice);


        /*********************************************************************************************************************
     * @brief 查询设备状态，只限于uvc设备
     *
     * @param[in] pDevice需要查询的设备路径
     * @param[out] devState 返回的设备状态
     * @return LU_SUCCESS：成功，其他：失败
    *********************************************************************************************************************/
        [DllImport("LionSDK.dll", EntryPoint = "GetDeviceState", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern LU_RESULT GetDeviceState(IntPtr pDevice, ref LUDEV_STATE devState);

        [DllImport("LionSDK.dll", EntryPoint = "GetImage", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern LU_RESULT GetImage(IntPtr pDevice, LU_UINT32 showUi, PLionImageCallback callback);

    }
    #endregion

    #region delegate
    // Notice: call back function
    // @USER_CALLBACK_HANDLE_ENVENT
    // @byteEventid:enum eEventCallbackCommType
    // @ufpdId:平板设备ID
    // @PVEventParam1:fpd config or image data buffer addr
    // @nEventParam2:参数2，例如data size或状态
    // @nEventParam3:参数3，例如帧率 frame rate或状态等
    // @nEventParam4:参数4，例如pcie事件id或预留扩展
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate LU_RESULT PLionImageCallback(LU_DEVICE device, byte pImgData, LU_UINT32 nDataBuf, byte pFile, LU_UINT32 nFileBuf);


    #endregion
}
