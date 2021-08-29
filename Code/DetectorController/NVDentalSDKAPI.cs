using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Detector
{
    public delegate void ImageCallbackHandler(byte wnd, NV_ImageInfo image);
    public delegate void TemperatureCallbackHandler(float a, float b);
    public delegate void SystemStatusCallbackHandler(int a, int b);
    public delegate void ExcutedFinishCallbackHandler(bool isSuccess);
    public delegate void ExcutedCallbackHandler();
    public delegate void ExcutedCallbackHandlerWithValue(int targetValue);

    public unsafe struct NV_ImageInfo
    {
        public Int32 iPixelType;			///< 像素格式[暂时未用到]
        public Int32 iSizeX;             ///< 图像宽
        public Int32 iSizeY;             ///< 图像高
        public Int32 iImageSize;         ///< 图像所占的字节数

        public ushort* pImageBuffer;		///< 图像数据指针
        public Int32 iTimeStamp;			///< 时间戳
        public Int32 iMissingPackets;    ///< 丢失的包数量
        public Int32 iAnnouncedBuffers;  ///< 声明缓存区大小[暂为0]
        public Int32 iQueuedBuffers;     ///< 队列缓存区大小[暂为0]
        public Int32 iOffsetX;           ///< x方向偏移量[暂未设置]
        public Int32 iOffsetY;           ///< y方向偏移量[暂未设置]
        public Int32 iAwaitDelivery;     ///< 等待传送的帧数[暂为0]
        public Int32 iBlockId;			///< GVSP协议的block-id
    }
#if NV4343
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

    /// @brief 图像像素格式(位数)
    public enum NV_PixelFormat
    {
        NV_PF_Mono16 = 0x01100007,  ///< 16位图像数据(默认值)
        NV_PF_Mono14 = 0x01100025,  ///< 14位图像数据
        NV_PF_Mono12 = 0x01100005,  ///< 12位图像数据
        NV_PF_Mono8 = 0x01080001    ///< 8位图像数据
    }







    public static class NVDentalSDK
    {


      //  [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Auto)]
     //   public static extern UIntPtr HBI_Init();
 
     //   [DllImport("HBI_FPD_E.dll", CharSet = CharSet.Ansi)]
     //   public static extern int HBI_GetSDKVerion(UIntPtr handle, StringBuilder szVer);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						打开探测器，读取配置文件初始化到硬件，并启动温度和状态监控线程
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						该函数用户打开探测器，当探测器不再使用时必须调用 @c NV_CloseDet()
        /// @sa @c NV_CloseDet()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_OpenDet();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						关闭探测器，并停止温度和状态监控线程
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						当探测器关闭时，清理所用资源
        /// @sa @c NV_OpenDet()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_CloseDet();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取传感器尺寸
        /// @param [out] sensorWidth	传感器宽
        /// @param [out] sensorHeight	传感器高
        /// @param [out] bits			传感器位数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetSensorSize(out int sensorWidth, out int sensorHeight, out int bits);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取版本信息
        /// @param [out] version		探测器版本信息
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetVersion(out NV_Version version);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取探测器序列号
        /// @param [out] pBuffer		探测器序列号
        /// @param [out] pSize			序列号缓存长度
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetSerialNum(out byte[] pBuffer, out  ulong pSize);

        /* AcquisitionControl */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置采集模式
        /// @param [in] aMode			采集模式 
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetAcquisitionMode(NV_AcquisitionMode aMode);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置快门模式	
        /// @param [in] sMode			快门模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetShutterMode(NV_ShutterMode sMode);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置Binning模式
        /// @param [in] bMode			Binning模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetBinningMode(NV_BinningMode bMode);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取采集模式
        /// @param [out] aMode			采集模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetAcquisitionMode(out NV_AcquisitionMode aMode);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取快门模式
        /// @param [out] sMode			快门模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetShutterMode(out NV_ShutterMode sMode);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取Binning模式
        /// @param [out] bMode			Binning模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetBinningMode(out NV_BinningMode bMode);

        /* WorkControl */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置增益档
        /// @param [in] gain			增益档
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetGain(NV_Gain gain);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置ROI模式图像范围
        /// @param [in] offsetX			图像起始列
        /// @param [in] offsetY			图像起始行
        /// @param [in] width			图像宽
        /// @param [in] height			图像高
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetImageRange(int offsetX, int offsetY, int width, int height);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置探测器的积分时间(即X射线出线时间)，单位0.1ms。当AcquisitionMode是CONTINUE、FALLING_EDGE_TRRIGER及RISING_EDGE_TRIGGER时，以该设定值作为Sensor的积分窗口时间
        /// @param [in] expTime			探测器的积分时间(即X射线出线时间)
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetExpTime(int expTime);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置该序列的最大图像帧数。
        /// @param [in] maxFrames		该序列的最大图像帧数。当为0时，不限制图像帧数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						当maxFrames不为0时，达到最大帧数会自动停止采集。
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetMaxFrames(int maxFrames);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取增益档
        /// @param [out] gain			增益档
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetGain(out NV_Gain gain);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取ROI模式图像范围
        /// @param [out] offsetX		图像起始列
        /// @param [out] offsetY		图像起始行
        /// @param [out] width			图像宽
        /// @param [out] height			图像高
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetImageRange(out int offsetX, out int offsetY, out int width, out int height);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取探测器的积分时间(即X射线出线时间)，单位0.1ms。当AcquisitionMode是CONTINUE、FALLING_EDGE_TRRIGER及RISING_EDGE_TRIGGER时，以该设定值作为Sensor的积分窗口时间
        /// @param [out] expTime		探测器的积分时间(即X射线出线时间)
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetExpTime(out int expTime);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取该序列的最大图像帧数。
        /// @param [out] maxFrames		该序列的最大图像帧数。当为0时，不限制图像帧数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetMaxFrames(out int maxFrames);

        /* Acq */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置采集回调
        /// @param [in] pCallback		采集回调函数
        /// @param [in] pCallbackData	回调上下文
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetAcqCallback(ImageCallbackHandler pCallback, object context);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						创建采图缓冲区
        /// @param [in] imgCount		要缓存的图像总张数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						调用该函数之前必须确保探测器已经打开，否则会返回失败。
        ///								如果缓冲区满则覆盖最早的数据，读索引向后移动。
        ///								不再使用缓冲区时必须调用 @c NV_AcqBufferDestroy()释放。如果之前的缓冲区没有释放，则函数返回失败。
        ///								如果图像大小改变(改变Binning模式或设置图像宽高)，SDK自动调整到适合图像宽高的缓冲区，缓存的图像总张数不变。
        /// @sa @c NV_AcqBufferDestroy()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_AcqBufferCreate(uint imgCount);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取采图缓冲区可读图像张数
        /// @param [out] readableNum	采图缓冲区可读图像张数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_AcqBufferReadableNum(out uint readableNum);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						读取采图缓冲区一张图像
        /// @param [out] imgInfo		读出的图像内容
        /// @param [in] milliseconds	等待读取的时间
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						先进先出，顺序读取，即：最早采集到的图像最先读出。
        ///								如果有数据被覆盖，读索引会自动向后移动。读到的数据始终是最先进入队列的数据。
        ///								若设置milliseconds为0，则没有可读数据会立即返回；若设置milliseconds>0，则阻塞milliseconds毫秒直到超时或有数据可读
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_AcqBufferReadImage(out NV_ImageInfo imgInfo, ulong milliseconds = 0);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						清空采图缓冲区，即全置0
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_AcqBufferClear();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						释放采图缓冲区
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						该函数负责回收资源。
        ///								不再使用采图缓冲区时，必须调用该函数，否则会造成内存泄漏。
        /// @sa @c NV_AcqBufferCreate()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_AcqBufferDestroy();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						开始采集
        /// @param [in] pCallback		采集到达最大帧回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						该函数创建新线程接收数据，并发送"AcquisitionStart"命令启动采集，然后立即返回。
        ///								如果MaxFrame不为0，采集到最大帧数时，自动发送"AcquisitionStop"命令停止采集，并回调用户提供的回调函数。
        ///								采集过程中用户可以自己调用 @c NV_StopAcq()结束采集。
        ///								SDK提供两种方式获取图像数据：1.设置回调函数 @c NV_SetAcqCallback(); 2.创建缓冲区 @c NV_AcqBufferCreate() @c NV_AcqBufferReadImage()
        /// @sa @c NV_StopAcq() @c NV_SetAcqCallback() @c NV_AcqBufferCreate() @c NV_AcqBufferReadImage() ..
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_StartAcq(ExcutedCallbackHandler pCallback);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						结束采集
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						该函数发送"AcquisitionStop"命令停止采集，并结束 @c NV_StartAcq()创建的接收数据线程。
        ///								采集过程中用户调用该函数以结束采集。
        /// @sa @c NV_StartAcq()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_StopAcq();

        /* Calibration */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						启动本底校正线程，读取配置文件进行校正，校正完成后保存本底模板文件
        /// @param [in] pCallback		本底校正完成回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						启动新线程后台工作，该函数立刻返回。线程结束时回调通知本底校正是否成功
        ///								调用该函数之前需要调用 @c NV_ResetCorrection()重置校正使能
        /// @sa @c NV_CancelOffsetCal() @c NV_ResetCorrection()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_RunOffsetCalThread(ExcutedFinishCallbackHandler pCallback);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						取消本底校正
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						本底校正线程会被终止
        /// @sa @c NV_RunOffsetCalThread()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_CancelOffsetCal();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						启动增益校正线程，读取配置文件进行校正，校正完成后保存增益模板文件
        /// @param [in] pCallback		增益校正完成回调函数
        /// @param [in] pOpenXRay		打开X光提示回调
        /// @param [in] pCloseXRay		关闭X光提示回调
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						启动新线程后台工作，该函数立刻返回。线程结束时回调通知增益校正是否成功
        ///								调用该函数之前需要调用 @c NV_ResetCorrection()重置校正使能
        /// @sa @c NV_CancelGainCal() @c NV_ResetCorrection()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_RunGainCalThread(ExcutedFinishCallbackHandler pCallback, ExcutedCallbackHandlerWithValue pOpenXRay, ExcutedCallbackHandler pCloseXRay);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						取消增益校正
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						增益校正线程会被终止
        /// @sa @c NV_RunGainCalThread()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_CancelGainCal();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						启动自动坏点校正线程，读取配置文件进行校正，校正完成后保存自动坏点模板文件
        /// @param [in] pCallback		自动坏点校正完成回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						启动新线程后台工作，该函数立刻返回。线程结束时回调通知自动坏点校正是否成功
        ///								调用该函数之前需要调用 @c NV_ResetCorrection()重置校正使能
        /// @sa @c NV_ResetCorrection()
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_RunAutoDefectCalThread(ExcutedFinishCallbackHandler pCallback);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						保存手动坏点模板(坏点用非0数表示)
        /// @param [in] pixel			手动坏点模板的数据区域
        /// @param [in] width			图像宽
        /// @param [in] height			图像高
        /// @param [in] mode			Binning模式
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SaveManDefectData(ushort pixel, int width, int height, NV_BinningMode mode);

        /* ImageProcessing */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置本底校正使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetOffsetCal(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置增益校正使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetGainCal(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置坏点校正使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetDefectCal(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置水平镜像使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetMirrorX(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置垂直镜像使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetMirrorY(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置图像反白使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetInversion(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置Gamma校正使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetDigitalGamma(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置LUT校正使能
        /// @param [in] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetDigitalLUT(NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置图像校正完成后整体的像素灰度偏移量
        /// @param [in] imageOffset		整体的像素灰度偏移量，默认为0
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetImageOffset(int imageOffset);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						重置校正使能
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						校正使能全部设为不使能，偏移量设为0，采图范围设为默认值
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_ResetCorrection();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取本底校正使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetOffsetCal(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取增益校正使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetGainCal(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取坏点校正使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetDefectCal(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取水平镜像使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetMirrorX(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取垂直镜像使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetMirrorY(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取图像反白使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetInversion(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取Gamma校正使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetDigitalGamma(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取LUT校正使能状态
        /// @param [out] type			校正类型
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetDigitalLUT(out NV_CorrType type);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取图像校正完成后整体的像素灰度偏移量
        /// @param [out] imageOffset	整体的像素灰度偏移量
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetImageOffset(out int imageOffset);

        /* SystemStatus  */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						主动获取硬件温度
        /// @param [out] temperature1	温度1
        /// @param [out] temperature2	温度2
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetTemperature(out float temperature1, out float temperature2);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						主动获取硬件系统状态
        /// @param [out] status1		系统状态1
        /// @param [out] status2		系统状态2
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetSystemStatus(out int status1, out int status2);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						监视硬件温度，如果发生改变则回调
        /// @param [in] pTempCallback	硬件温度改变回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_MonitorTemperature(TemperatureCallbackHandler pTempCallback);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						监视硬件系统状态，如果发生改变则回调
        /// @param [in] pStatusCallback 硬件系统状态改变回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_MonitorSystemStatus(SystemStatusCallbackHandler pStatusCallback);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						监视网络连接断开，如果连接断开则回调
        /// @param [in] pCallback		网络连接断开回调函数
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @attention					还未实现
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_MonitorConnbreak(ExcutedCallbackHandler pCallback);

        /* ErrorMsg	*/

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取详细错误信息
        /// @param [out] errorMsg		错误信息缓存区
        /// @param [in] bufflen			缓存区长度
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        /// @note						如果函数接口返回错误码，用该函数获取详细错误信息
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_LastErrorMsg([MarshalAs(UnmanagedType.LPStr)]StringBuilder errorMsg, uint bufflen);

        /* ParamFile */

        /////////////////////////////////////////////////////////////////////////
        /// @brief						重置探测器所有参数信息
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_ResetAllParam();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						加载参数到配置文件
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_LoadParamFile();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						保存参数到配置文件
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SaveParamFile();

        /////////////////////////////////////////////////////////////////////////
        /// @brief						设置TestDAC
        /// @param [in] tdac	
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_SetTestDAC(int tdac);

        /////////////////////////////////////////////////////////////////////////
        /// @brief						获取TestDAC
        /// @param [out] tdac	
        /// @retval	NV_StatusCodes		返回状态码，若成功则返回NV_SC_SUCCESS
        [DllImport("nvDentalDet.dll", CharSet = CharSet.Auto)]
        public static extern NV_StatusCodes NV_GetTestDAC(out int tdac);
    }

#endif

}
