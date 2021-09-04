/*---------------------------------------------------------------------------
* Copyright (c) 2019 上海昊博影像科技有限公司
* All rights reserved.
*
* 文件名称: HBI_FPD_DLL.cs
* 文件标识:
* 摘    要: 
*
* 当前版本: 1.0.4.2
* 作    者: mhyang
* 创建日期: 2020/09/23
* 修改日期: 
----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Detector
{



    public class HBI_FPD_DLL
    {

        /// <summary>
        /// 获取错误码的内容
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetErrorMsgByCode(int code)
        {

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


        /// <summary>
        /// 记录与Detector连接后的句柄
        /// </summary>
        public static IntPtr _handel;

        #region functions
        /*********************************************************
        * 函 数 名: HBI_Init
        * 功能描述: 初始化动态库
        * 参数说明:
        * 返 回 值：IntPtr 
		            失败：NULL,成功：非空
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_Init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr HBI_Init();

        /*********************************************************
        * 函 数 名: HBI_Destroy
        * 功能描述: 释放动态库资源
        * 参数说明:
                In: IntPtr handle - 句柄
                Out: 无
        * 返 回 值：void
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_Destroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void HBI_Destroy(IntPtr handle);

        /*********************************************************
        * 函 数 名: HBI_RegEventCallBackFun
        * 功能描述: 注册回调函数
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                USER_CALLBACK_HANDLE_ENVENT handleEventfun - 注册回调函数
                IntPtr pContext - 对象指针
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_RegEventCallBackFun", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_RegEventCallBackFun(IntPtr handle, USER_CALLBACK_HANDLE_ENVENT handleEventfun);

        /*********************************************************
        * 函 数 名: HBI_ConnectDetector
        * 功能描述: 建立连接
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                    String szRemoteIp - 平板IP地址,如192.168.10.40
                    ushort remotePort - 平板端口,如32897(0x8081)
                    String szlocalIp - 上位机地址,如192.168.10.20
                    ushort localPort -上位机端口,如192.168.10.40
			        int doOffsetTemp - 非1:连接成功后固件不重新做offset模板
							           1:连接成功后固件重新做offset模板
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_ConnectDetector", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_ConnectDetector(IntPtr handle, string szRemoteIp, ushort remotePort, string szlocalIp, ushort localPort, int doOffsetTemp = 0);

        /*********************************************************
        * 函 数 名: HBI_DisConnectDetector
        * 功能描述: 断开连接
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                    0   - 成功
                    非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_DisConnectDetector", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_DisConnectDetector(IntPtr handle);

        /*********************************************************
        * 函 数 名: HBI_GetDevCfgInfo
        * 功能描述: 获取固件ROM参数
        * 参数说明:
	        In: void *handle - 句柄(无符号指针)
	        Out:RegCfgInfo* pRegCfg,见HbDllType.h。
        * 返 回 值：int
	        0   - 成功
	        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetDevCfgInfo", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetDevCfgInfo(IntPtr handle, ref RegCfgInfo pRegCfg);

        /*********************************************************
        * 函 数 名: HBI_Prepare
        * 功能描述: 软触发，清空准备指令
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        0   成功
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_Prepare", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_Prepare(IntPtr handle);

        /*********************************************************
        * 函 数 名: HBI_SingleAcquisition
        * 功能描述: 单帧采集
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                    FPD_AQC_MODE _mode - 采集模式以及参数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_SingleAcquisition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_SingleAcquisition(IntPtr handle, FPD_AQC_MODE _mode);

        /*********************************************************
        * 函 数 名: HBI_LiveAcquisition
        * 功能描述: 连续采集
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                    FPD_AQC_MODE _mode - 采集模式以及参数
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_LiveAcquisition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_LiveAcquisition(IntPtr handle, FPD_AQC_MODE _mode);

        /*********************************************************
        * 函 数 名: HBI_StopAcquisition
        * 功能描述: 停止连续采集
        * 参数说明:
                In: IntPtr handle - 句柄(无符号指针)
                Out: 无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备     注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_StopAcquisition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_StopAcquisition(IntPtr handle);

        /*********************************************************
        * 函 数 名: HBI_GetImageProperty
        * 功能描述: 获取图像属性
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
		        Out: IMAGE_PROPERTY *img_pro,见HbDllType.h。
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetImageProperty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetImageProperty(IntPtr handle, ref IMAGE_PROPERTY img_pro);

        /*********************************************************
        * 函 数 名: HBI_UpdateTriggerMode
        * 功能描述: 设置触发模式
        * 参数说明:
                In: void *handle - 句柄(无符号指针)
                    int _triggerMode,1-软触发，3-高压触发，4-FreeAED。
                Out:无
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_UpdateTriggerMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_UpdateTriggerMode(IntPtr handle, int _triggerMode);

        /*********************************************************
        * 函 数 名: HBI_TriggerAndCorrectApplay
        * 功能描述: 设置触发模式和图像校正使能（工作站）新版本
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
			        int _triggerMode,1-软触发，3-高压触发，4-FreeAED。
			        IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
		        Out:无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_TriggerAndCorrectApplay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_TriggerAndCorrectApplay(IntPtr handle, int _triggerMode, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 函 数 名: HBI_UpdateCorrectEnable
        * 功能描述: 更新图像固件校正使能
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
			        IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
		        Out:无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_UpdateCorrectEnable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_UpdateCorrectEnable(IntPtr handle, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 函 数 名: HBI_GetCorrectEnable
        * 功能描述: 获取图像固件校正使能
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
			        IMAGE_CORRECT_ENABLE* pCorrect,见HbDllType.h。
		        Out:无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetCorrectEnable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetCorrectEnable(IntPtr handle, ref IMAGE_CORRECT_ENABLE pCorrect);

        /*********************************************************
        * 函 数 名: HBI_GetFirmareVerion
        * 功能描述: 获取固件版本号
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
		        Out: char *szVer
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetFirmareVerion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetFirmareVerion(IntPtr handle, StringBuilder szFirmwareVer);

        /*********************************************************
        * 函 数 名: HBI_GetSDKVerion
        * 功能描述: 获取SDK版本号
        * 参数说明:
          In: void *handle - 句柄(无符号指针)
          Out: char *szVer
        * 返 回 值：int
          0   - 成功
          非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetSDKVerion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetSDKVerion(IntPtr handle, StringBuilder szSDKVer);

        /*********************************************************
        * 函 数 名: HBI_SetLiveAcquisitionTime
        * 功能描述: 设置采集时间间隔
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
			        int time - 间隔时间,单位是毫秒ms，>= 1000ms
		        Out: 无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_SetAcqSpanTm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_SetAcqSpanTm(IntPtr handle, int time);

        /*********************************************************
        * 函 数 名: HBI_GetLiveAcquisitionTime
        * 功能描述: 获取采集时间间隔
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
		        out:int *out_time - 时间,单位是毫秒ms
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetAcqSpanTm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetAcqSpanTm(IntPtr handle, ref int time);

        /*********************************************************
        * 函 数 名: HBI_SetSinglePrepareTime
        * 功能描述: 设置软触发单帧采集清空和采集之间的时间间隔
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
		            int *in_itime - 时间,>=0,单位:mm
				        0-表示软触发单帧采集先HBI_Prepare后HBI_SingleAcquisition完成单帧采集
				        非0-表示软触发单帧采集，只需HBI_Prepare即可按照预定时间完成单帧采集
        * 返 回 值：int
		        0   - 成功(
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_SetSinglePrepareTime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_SetSinglePrepareTime(IntPtr handle, int intime);

        /*********************************************************
        * 函 数 名: HBI_GetPreAcqTm
        * 功能描述: 获取软触发单帧采集清空和采集之间的时间间隔
        * 参数说明:
		        In: void *handle - 句柄(无符号指针)
		        out:int *out_itime - 时间 >=0 单位:mm
		                0-表示软触发单帧采集先HBI_Prepare后HBI_SingleAcquisition完成单帧采集
				        非0-表示软触发单帧采集，只需HBI_Prepare即可按照预定时间完成单帧采集
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GetSinglePrepareTime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GetSinglePrepareTime(IntPtr handle, ref int outtime);

        /*********************************************************
        * 函 数 名: HBI_16UCConvertTo8UC
        * 功能描述: 16位数据转换为8位
        * 参数说明:
                In:     void *handle - 句柄(无符号指针)
                In/Out: unsigned char *imgbuff
                In/Out: int *nbufflen
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_16UCConvertTo8UC", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_16UCConvertTo8UC(IntPtr handle, IntPtr imgbuff, ref int nbufflen);

        /*********************************************************
        * 编    号: No041
        * 函 数 名: HBI_GenerateTemplateEx
        * 功能描述: 快速生成模板（集成开发）
        * 参数说明:
	        In: void *handle - 句柄(无符号指针)
		        EnumGENERATE_TEMPLATE_TYPE templateType - 生成模板类型,见HbDllType.h。
	        Out: 无
        * 返 回 值：int
	        0   - 成功
	        非0 - 失败
        * 备    注:
        * 1、OFFSET_TEMPLATE：生成暗场模板，注意球管关闭
        * 2、GAIN_TEMPLATE：生成增益矫正模板，需要打开球管，调节好剂量开，始平板采集亮场图像并生成gain模板
        * 3、DEFECT_TEMPLATE_GROUP1：生成坏点矫正模板，需要打开球管，调节好剂量，开始平板采集第1组亮场图像
        * 4、DEFECT_TEMPLATE_GROUP2：生成坏点矫正模板，需要打开球管，调节好剂量，开始平板采集第2组亮场图像
        * 5、DEFECT_TEMPLATE_GROUP3：生成坏点矫正模板，需要打开球管，调节好剂量，开始平板采集第3组亮场图像并生成defect模板
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_GenerateTemplateEx", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_GenerateTemplateEx(IntPtr handle, EnumGENERATE_TEMPLATE_TYPE templateType);

        /*********************************************************
        * 编    号: No006
        * 函 数 名: HBI_RegProgressCallBack
        * 功能描述: 注册回调函数
        * 参数说明: 处理固件下载或者固件升级反馈信息
		        In: void *handle - 句柄(无符号指针)
			        USER_CALLBACK_HANDLE_ENVENT handleStatusfun - 注册回调函数
			        void* _Object - 上位机某个模块对象指针，也可为空值(例如C/C++为NULL)
		        Out: 无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_RegProgressCallBack", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_RegProgressCallBack(IntPtr handle, USER_CALLBACK_HANDLE_PROCESS handleStatusfun, IntPtr pContext);

        /*********************************************************
        * 编    号: No049
        * 函 数 名: HBI_DownloadTemplateEx
        * 功能描述: 下发gain和defect矫正模板
        * 参数说明:
            In: void *handle - 句柄(无符号指针)
                DOWNLOAD_T_FILE- 模板类型和模板文件路径
            Out: 无
        * 返 回 值：int
            0   - 成功
            非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_DownloadTemplateEx", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_DownloadTemplateEx(IntPtr handle, emTFILE_MODE emfiletype);

        /*********************************************************
        * 函 数 名: HBI_TriggerBinningAcqTime
        * 功能描述: 设置触发模式、binning方式以及帧率（采集图像时间间隔）
        * 参数说明:
		In: void *handle - 句柄(无符号指针)
		    int triggerMode - 触发模式
			静态平板（每秒1帧）：1-软触发，2-Clear,3-高压触发，4-FreeAED
			动态平板（每秒2帧以上）：05-Dynamic:Hvg Sync Mode,06-Dynamic:Fpd Sync Mode,7-Continue Mode
		    unsigned char binning - 1:1x1,2:2x2,3:3x3,4:4x4，其他不支持
		    int time - 间隔时间,单位是毫秒ms,大于0
        * 返 回 值：int
                0   - 成功
                非0 - 失败
        * 备    注:void *handle, int triggerMode, unsigned char binning, int time
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_TriggerBinningAcqTime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_TriggerBinningAcqTime(IntPtr handle, int triggerMode, byte binning, int time);
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        //// 其他接口类似,用户根据需求添加
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        ///


        /*********************************************************
        * 编    号: No027
        * 函 数 名: HBI_SetBinning
        * 功能描述: 设置bingning方式
        * 参数说明:
		        In:
		         void *handle - 句柄(无符号指针)
		         unsigned char bytebin - 1:1x1,2:2x2,3:3x3(暂不支持),4:4x4，其他不支持
		        Out:无
        * 返 回 值：int
		        0   - 成功
		        非0 - 失败
        * 备    注:
        *********************************************************/
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_SetBinning", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_SetBinning(IntPtr handle, byte binning);


        /*********************************************************
        * 编    号: No023
        * 函 数 名: HBI_SetGainMode
        * 功能描述: 设置增益模式
        * 参数说明:
                In: void *handle - 句柄(无符号指针)
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
        [DllImport("HBI_FPD_E.dll", EntryPoint = "HBI_SetGainMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HBI_SetGainMode(IntPtr handle, int nGainMode);

        #endregion
    }

    #region delegate
    // Notice: call back function
    // @USER_CALLBACK_HANDLE_ENVENT
    // @cmd:enum eEventCallbackCommType
    // @buff
    // @len
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int USER_CALLBACK_HANDLE_ENVENT(byte cmd, IntPtr buff, int len, int id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int USER_CALLBACK_HANDLE_PROCESS(byte cmd, int retcode, IntPtr pContext);
    #endregion

    #region enum


    public enum EFpdStatusType
    {
        FPD_STATUS_DISCONN    = 0x00,
        FPD_PREPARE_STATUS    = 0x01,
        FPD_READY_STATUS      = 0x02,
        FPD_DOOFFSET_TEMPLATE = 0x03,
        FPD_EXPOSE_STATUS     = 0x04,
        FPD_CONTINUE_READY    = 0x05,
        FPD_DWONLOAD_GAIN     = 0x06,
        FPD_DWONLOAD_DEFECT   = 0x07,
        FPD_DWONLOAD_OFFSET   = 0x08,
        FPD_UPDATE_FIRMARE    = 0x09,
        FPD_CONN_STATUS       = 0x0A,
        FPD_STATUS_AED        = 0x0B
    };

    // Notice:Fpd Trigger Work Mode
    public enum EnumTRIGGER_MODE
    {
        INVALID_TRIGGER_MODE         = 0x00,     // 无效触发模式
        STATIC_SOFTWARE_TRIGGER_MODE = 0x01,     // 软触发模式
        STATIC_PREP_TRIGGER_MODE     = 0x02,     // Clear Mode
        STATIC_HVG_TRIGGER_MODE      = 0x03,     // 高压触发
        STATIC_AED_TRIGGER_MODE      = 0x04,     // AED
        DYNAMIC_HVG_TRIGGER_MODE     = 0x05,     // Dynamic:Hvg Sync Mode
        DYNAMIC_FPD_TRIGGER_MODE     = 0x06,     // Dynamic:Fpd Sync Mode
        DYNAMIC_FPD_CONTINUE_MODE    = 0x07      // Dynamic:Continue Mode  
    };

    // Notice: acq mode:static and dynamic
    public enum EnumIMAGE_ACQ_MODE
    {
        STATIC_ACQ_DEFAULT_MODE = 0x00, // 默认单帧采集
        DYNAMIC_ACQ_DEFAULT_MODE,       // 默认连续采集
        DYNAMIC_ACQ_BARK_MODE,          // 创建Offset模板-连续采集暗场图
        DYNAMIC_ACQ_BRIGHT_MODE,        // 创建Gain模板-连续采集亮场图
        STATIC_ACQ_BRIGHT_MODE,         // 创建Gain模板-单帧采集亮场图
        STATIC_DEFECT_ACQ_MODE,         // 创建Defect模板-单帧采集亮场图
        DYNAMIC_DEFECT_ACQ_MODE,        // 创建Defect模板-连续采集亮场图
    };

    // Notice:Generate Template Type
    public enum EnumGENERATE_TEMPLATE_TYPE
    {
        OFFSET_TEMPLATE = 0x00,    // 连续采集一组暗场图 - offset 
        GAIN_TEMPLATE,             // 连续采集一组亮场图 - gain
        DEFECT_TEMPLATE_GROUP1,    // 连续采集一组亮场图 - defect group1
        DEFECT_TEMPLATE_GROUP2,    // 连续采集一组亮场图 - defect group2
        DEFECT_TEMPLATE_GROUP3     // 连续采集一组亮场图 - defect group3 and generate template
    };

    // Notice: Live Acquisition'properties 
    public enum emGRAY_MODE
    {
        GRAY_8 = 0x00,
        GRAY_16 = 0x01,
        GRAY_32 = 0x02
    };

    // Notice: upload template file
    // template file type
    public enum emTFILE_MODE
    {
        GAIN_T       = 0x00,
        DEFECT_T     = 0x01,
        OFFSET_T     = 0x02
    };

    // upload process status
    public enum emUPLOAD_FILE_STATUS
    {
        UPLOAD_FILE_START = 0x00,
        UPLOAD_FILE_DURING = 0x01,
        UPLOAD_FILE_STOP = 0x02
    };

    // Notice: After Each Member Variables, show Variables enum , 
    // before '-' is variables' value, after '-' is the meaning of the value;
    // each value departed by ';' symbol
    public enum eRequestCommType
    {
	    EDL_COMMON_TYPE_INVALVE               = 0x00,
	    EDL_COMMON_TYPE_GLOBAL_RESET          = 0x01,
	    EDL_COMMON_TYPE_PREPARE               = 0x02,
	    EDL_COMMON_TYPE_SINGLE_SHORT          = 0x03,
	    EDL_COMMON_TYPE_LIVE_ACQ              = 0x04,
	    EDL_COMMON_TYPE_STOP_ACQ              = 0x05,
	    EDL_COMMON_TYPE_PACKET_MISS           = 0x06,
	    EDL_COMMON_TYPE_FRAME_MISS            = 0x07,
	    EDL_COMMON_TYPE_DUMMPING              = 0x08,
	    EDL_COMMON_TYPE_END_RESPONSE          = 0x0A, // End response packet
        EDL_COMMON_TYPE_CONNECT_FPD           = 0x0B, // connect command
        EDL_COMMON_TYPE_DISCONNECT_FPD        = 0x0C,
        EDL_COMMON_TYPE_SET_RAM_PARAM_CFG     = 0x10,
	    EDL_COMMON_TYPE_GET_RAM_PARAM_CFG     = 0x11,
	    EDL_COMMON_TYPE_SET_ROM_PARAM_CFG     = 0x12,
	    EDL_COMMON_TYPE_GET_ROM_PARAM_CFG     = 0x13,
	    EDL_COMMON_TYPE_SET_FACTORY_PARAM_CFG = 0x14,
	    EDL_COMMON_TYPE_GET_FACTORY_PARAM_CFG = 0x15,
	    EDL_COMMON_TYPE_RESET_FIRM_PARAM_CFG  = 0x16,
        EDL_COMMON_UPLOAD_OFFSET_TEMPLATE     = 0x2E, //add by MH.YANG 30/12
        EDL_COMMON_UPLOAD_GAIN_TEMPLATE       = 0x2F, // Upload gain template
	    EDL_COMMON_UPLOAD_DEFECT_TEMPLATE     = 0x30, // Upload defect template
	    EDL_COMMON_ERASE_FIRMWARE             = 0x4F, // Erase old firmware package request
	    EDL_COMMON_UPDATE_FIRMWARE            = 0x50, // Update new firmware package request
	    EDL_COMMON_TYPE_SET_AQC_MODE          = 0xFF
    };

    // Notice: After Each Member Variables, show Variables enum , 
    // before '-' is variables' value, after '-' is the meaning of the value;
    // each value departed by ';' symbol
    public enum eCallbackEventCommType
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
        ECALLBACK_TYPE_NET_ERR_MSG = 0XAA,
        ECALLBACK_TYPE_SET_CFG_OK = 0XAB, // 设置参数成功
        ECALLBACK_TYPE_ACQ_SUCCESS       = 0XAC, // 反馈采集到一帧图像成功
        ECALLBACK_TYPE_GENERATE_TEMPLATE = 0XAD, // 生成模板
        ECALLBACK_TYPE_FILE_NOTEXIST     = 0XAE  // 文件不存在
    };

    public enum eCallbackUpdateFirmwareStatus
    {
        ECALLBACK_UPDATE_STATUS_START    = 0XB0,
	    ECALLBACK_UPDATE_STATUS_PROGRESS = 0XB1,
	    ECALLBACK_UPDATE_STATUS_RESULT   = 0XB2
    };

    enum eCallbackDownloadTemplateFileStatus
    {
        ECALLBACK_DTFile_STATUS_CONNECT  = 0XC0,
        ECALLBACK_DTFile_STATUS_ERASE    = 0XC1,
        ECALLBACK_DTFile_STATUS_PROGRESS = 0XC2,
        ECALLBACK_DTFile_STATUS_RESULT   = 0XC3,
        ECALLBACK_DTFile_STATUS_NET      = 0XC4,
        ECALLBACK_DTFile_STATUS_CMD_ERR  = 0XC5,
        ECALLBACK_DTFile_STATUS_SEND     = 0XC6,
        ECALLBACK_DTFile_STATUS_NET_ERR  = 0XC7
    };

    public enum HBIRETCODE
    {
	    HBI_SUCCSS = 0,
	    HBI_ERR_OPEN_DETECTOR_FAILED     = 8001,
	    HBI_ERR_INVALID_PARAMS           = 8002,
	    HBI_ERR_CONNECT_FAILED           = 8003,
	    HBI_ERR_MALLOC_FAILED            = 8004,
	    HBI_ERR_RELIMGMEM_FAILED         = 8005,
	    HBI_ERR_RETIMGMEM_FAILED         = 8006,
	    HBI_ERR_NODEVICE                 = 8007,
	    HBI_ERR_NODEVICE_TRY_CONNECT     = 8008,
	    HBI_ERR_DEVICE_BUSY              = 8009,
	    HBI_ERR_SENDDATA_FAILED          = 8010,
	    HBI_ERR_RECEIVE_DATA_FAILED      = 8011,
	    HBI_ERR_COMMAND_DISMATCH         = 8012,
	    HBI_ERR_NO_IMAGE_RAW             = 8013,
	    HBI_ERR_PTHREAD_ACTIVE_FAILED    = 8014,
	    HBI_ERR_STOP_ACQUISITION         = 8015,
	    HBI_ERR_INSERT_FAILED            = 8016,
	    HBI_ERR_GET_CFG_FAILED           = 8017,
	    HBI_NOT_SUPPORT                  = 8018,
	    HBI_REGISTER_CALLBACK_FAILED     = 8019,
	    HBI_SEND_MESSAGE_FAILD           = 8020,
	    HBI_ERR_WORKMODE                 = 8021,
        HBI_FAILED = 8022,
        HBI_FILE_NOT_EXISTS = 8023,
        HBI_COMM_TYPE_ERR = 8024,
        HBI_TYPE_IS_NOT_EXISTS = 8025,
        HBI_SAVE_FILE_FAILED = 8026,
        HBI_INIT_PARAM_FAILED = 8027,
        HBI_TIMEOUT = 8028,
        HBI_SYSTEM_EXCEPTION = 8028, //System exception
        HBI_END
    };

    public enum eCallbackTemplateStatus
    {
        ECALLBACK_TEMPLATE_BEGIN          = 0X00,
        ECALLBACK_TEMPLATE_INVALVE_PARAM  = 0X01,
        ECALLBACK_TEMPLATE_MALLOC_FAILED  = 0X02,
        ECALLBACK_TEMPLATE_SEND_FAILED    = 0X03,
        ECALLBACK_TEMPLATE_STATUS_ABORMAL = 0X04,
        ECALLBACK_TEMPLATE_FRAME_NUM      = 0X05,
        ECALLBACK_TEMPLATE_TIMEOUT        = 0X06,
        ECALLBACK_TEMPLATE_MEAN           = 0X07,
        ECALLBACK_TEMPLATE_GENERATE       = 0X08,
        ECALLBACK_TEMPLATE_RESULT         = 0X09
    };

    public enum EnumTempType
    {
        OFFSETFILE = 0x00,
        GAINFILE,
        BADPIXFILE,
        GAINA,
        GAINB
    };

    //public static const CodeStringTable CrErrStrList[] = {
    //{ 0,  HBI_SUCCSS, "Success" },
    //{ 1,  HBI_ERR_OPEN_DETECTOR_FAILED, "Open device driver failed" },
    //{ 2,  HBI_ERR_INVALID_PARAMS, "Parameter error" },
    //{ 3,  HBI_ERR_CONNECT_FAILED, "Connect failed" },
    //{ 4,  HBI_ERR_MALLOC_FAILED, "Malloc memory failed" },
    //{ 5,  HBI_ERR_RELIMGMEM_FAILED, "Releaseimagemem fail" },
    //{ 6,  HBI_ERR_RETIMGMEM_FAILED, "ReturnImageMem fail" },
    //{ 7,  HBI_ERR_NODEVICE, "No Init DLL Instance" },
    //{ 8,  HBI_ERR_NODEVICE_TRY_CONNECT, "Disconnect status" },
    //{ 9,  HBI_ERR_DEVICE_BUSY, "Fpd is busy" },
    //{ 10, HBI_ERR_SENDDATA_FAILED, "SendData failed" },
    //{ 11, HBI_ERR_RECEIVE_DATA_FAILED, "Receive Data failed" },
    //{ 12, HBI_ERR_COMMAND_DISMATCH, "Command dismatch" },
    //{ 13, HBI_ERR_NO_IMAGE_RAW, "No Image raw" },
    //{ 14, HBI_ERR_PTHREAD_ACTIVE_FAILED, "Pthread active failed" },
    //{ 15, HBI_ERR_STOP_ACQUISITION, "Pthread stop data acquisition failed" },
    //{ 16, HBI_ERR_INSERT_FAILED, "insert calibrate mode failed" },
    //{ 17, HBI_ERR_GET_CFG_FAILED, "get Fpd config failed" },
    //{ 18, HBI_NOT_SUPPORT, "not surport yet" },
    //{ 19, HBI_REGISTER_CALLBACK_FAILED, "failed to register callback function" },
    //{ 20, HBI_SEND_MESSAGE_FAILD, "send message failed" },
    //{ 21, HBI_ERR_WORKMODE, "switch work mode failed" },
    //{ 22, HBI_FAILED, "operation failed" },
    //{ 23, HBI_FILE_NOT_EXIST, "file does not exist" },
    //{ 24, HBI_TYPE_IS_NOT_EXISTS, "this type is not exists"},
    //{ 25, HBI_SAVE_FILE_FAILED, "save file failed"},
    //{ 26, HBI_INIT_PARAM_FAILED, "Init dll param failed"},
    //{ 27, HBI_END, "Exit monitoring"}
    //};
    #endregion

    #region struct
    #region

    // Notice:generate calibrate template input param
    public struct CALIBRATE_INPUT_PARAM
    {
        public int image_w;       // image width
        public int image_h;       // image height
        public int binning;       // binning
        public int roi_x;         // ROI left
        public int roi_y;         // ROI top
        public int roi_w;         // ROI width
        public int roi_h;         // ROI height
        public int group_sum;     // group sum
        public int per_group_num; // num per group
    };

    public struct FPD_AQC_MODE
    {
        public EnumIMAGE_ACQ_MODE aqc_mode;
        public int ngroupno;
        public int nframesum;
        public int ndiscard;
        public int nframeid;
        /* nLiveMode
        01：Image+OffsetTemplate 带有该参数点击Live Acquisition命令时，探测器会先做OFFSET模板再进行采集图像
        02：Only Image	         带有该参数点击Live Acquisition命令时,探测器会直接进行上图。不会有做模板的时间
        03：Only OffsetTemplate   带有该参数点击Live Acquisition命令时,探测器会只做模板。做完模板就会结束状态。
        */
        public int nLiveMode;  // 只限于连续采集(LIVE_ACQ_DEFAULT_TYPE)详细任务,1-固件做offset模板并上图；2-只上图；3-固件做只做offset模板。
        public bool isOverLap;
        public bool bSimpleGT;
        public emGRAY_MODE nGrayBit;//16,32
    };

    public struct IMAGE_PROPERTY
    {
        public int nSize;                //fpd_size
        public int nwidth;               //image width
        public int nheight;              //image height
        public int datatype;             //data type：0-unsigned char,1-char,2-unsigned short,3-float,4-double
        public int ndatabit;             //data bit:0-16bit,1-14bit,2-12bit,3-8bit
        public int nendian;              //endian:0-little endian,1-biger endian
        public int packet_sum;           //fpd send packet sum per frame
        public int frame_size;           //data size per frame
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct CodeStringTable
    {
        int num;
        int HBIRETCODE;
        String errString;// const char *errString;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SOFTWARE_CALIBRATE_ENABLE
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool enableOffsetCalib;
        [MarshalAs(UnmanagedType.U1)]
        public bool enableGainCalib;
        [MarshalAs(UnmanagedType.U1)]
        public bool enableDefectCalib;
        [MarshalAs(UnmanagedType.U1)]
        public bool enableDummyCalib;
    };
    // Notice:fpd software calibrate enable info.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_CORRECT_ENABLE
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool bFeedbackCfg;         //true-ECALLBACK_TYPE_ROM_UPLOAD Event,false-ECALLBACK_TYPE_SET_CFG_OK Event
        public char ucOffsetCorrection;   //00-"Do nothing";01-"prepare Offset Correction";  02-"post Offset Correction";
        public char ucGainCorrection;     //00-"Do nothing";01-"Software Gain Correction";   02-"Hardware Gain Correction"
        public char ucDefectCorrection;   //00-"Do nothing";01-"Software Defect Correction"; 02-"Software Defect Correction"
        public char ucDummyCorrection;    //00-"Do nothing";01-"Software Dummy Correction";  02-"Software Dummy Correction"
    };

    // Notice: calibrate template raw file call back info
    // 2019/09/03
    [StructLayout(LayoutKind.Sequential)]
    public struct ECALLBACK_RAW_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        char[] szRawName;	// 返回数据文件名称
        double dMean;               // 灰度均值
    };

    // lxm0712
    [StructLayout(LayoutKind.Sequential)]
    public struct GainInfo
    {
        int id;
        int dosvalue;         //剂量值
        int groupnum;         //获取测试组总数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        char[] offsetpath;    //亮场offset文件路径
        int averagegray;      //该剂量下所有帧的平均灰度
    };
    #endregion
    // Notice: calibrate lxm0712
    // System Manufacture Information：50
    // Notice:Region of interest in images：9
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ImageROI
    {
        public char m_byRoiEnable;        //01 ROI enbale or disable,1-enbale,非1-disable
        public ushort m_sRoiLeft;         //02 ROI rect left
        public ushort m_sRoiTop;          //02 ROI rect top
        public ushort m_sRoiWidth;        //02 ROI rect Width
        public ushort m_sRoiHeight;       //02 ROI rect Height
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SysBaseInfo
    {
        // Register_1
        public char m_byProductType;      				//01	//01-Flat Panel Detector;  02-Linear Array Detector
        // Register_2
        public char m_byXRaySensorType;   				//01	//01-Amorphous Silicon; 02-CMOS; 03-IGZO; 04-LTPS; 05-Amorphous Selenium
        // Register_3
        public char m_byPanelSize;        				//01	//01-1717 inch Panel Size; 02-1417; 03-1414; 04-1212; 
                                                                //05-1012; 06-0912; 07-0910; 08-0909; 09-0506; 10-0505; 11-0503
        // Register_4
        public char m_byPixelPitch;						//01	//01-27 um; 02-50; 03-75; 04-85; 05-100; 06-127; 07-140; 08-150; 09-180; 10-200 11-205; 12-400; 13-1000
        // Register_5
        public char m_byPixelMatrix;     				//01	//01-"3072 x 3072"; 02-"2536 x 3528"; 03-"2800 x 2304"; 04-"2304 x 2304"; 05-"2048 x 2048"; 06-"1536 x 1536"; 
                                                                //07-"1024 x 1536"; 08-"1024 x 1024"; 09-"1024 x 0768"; 10-"1024 x 0512"; 11-"0768 x 0768"; 
                                                                //12-"0512 x 0512"; 13-"0512 x 0256"; 14-"0256 x 0256"
        // Register_6
        public char m_byScintillatorType;				//01	//01-DRZ Standard; 02-DRZ High; 03-DRZ Plus; 04-PI-200; 05-Structured GOS; 06-CSI Evaporation; 07-CSI Attach;
        // Register_7
        public char m_byScanLineFanoutMode;				//01	//01-Single Side Single Gate; 02-Dual Side Single Gate; 03-Single Side Dual Gate; 04-Dual Side Dual Gate;
        // Register_8
        public char m_byReadoutLineFanoutMode;  		//01	//01-Single Side Single Read; 02-Dual Side Single Read; 03-Single Side Dual Read; 04-Dual Side Dual Read;
        // Register_9
        public char m_byFPDMode;						//01	//01-Static; 02-Dynamic;
        // Register_10
        public char m_byFPDStyle;						//01	//01-Fixed; 02-Wireless; 03-Portable;
        // Register_11
        public char m_byApplicationMode;        		//01	//01-Medical; 02-Industry;
        // Register_12
        public char m_byGateChannel;					//01	//01-"600 Channel"; 02-"512 Channel"; 03-"384 Channel"; 04-"256 Channel"; 05-"128 Channel"; 06-"064 Channel"
        // Register_13
        public char m_byGateType;						//01	//01-"HX8677"; 02-"HX8698D"; 03-"NT39565D"
        // Register_14
        public char m_byAFEChannel;						//01	//01-"64 Channel"; 02-"128 Channel"; 03-"256 Channel"; 04-"512 Channel";
        // Register_15
        public char m_byAFEType;						//01	//01-"AFE0064"; 02-"TI COF 2256"; 03-"AD8488"; 04-"ADI COF 2256";
        // Register_16~Register_17
        public ushort m_sGateNumber;					//02	//Gate Number
        // Register_18~Register_19
        public ushort m_sAFENumber;						//02	//AFE Number
        // Register_20~Register_21
        public ushort m_sImageWidth;                    //02	//Image Width // modified by mhyang 20191220
        // Register_22~Register_23
        public ushort m_sImageHeight;                   //02	//Image Height
        // Register_24~Register_37
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] m_cSnNumber;				        //14	//sn number   // modified by mhyang 20200401
        // Register_38~Register_46.
        public ImageROI m_stRoi;                        //9     Region Of Interest;
        // Register_47~Register_100.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public char[] m_abyReservedFrom48To100;	        //53//////Registers 38To100(include) Are Reserved;
    };

    // System Manufacture Information：50
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SysManuInfo
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5~Register_16
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abyFPDSerialNumber;   		        //12	//FPD SN Byte 01-12;
        // Register_17~Register_18
        public short m_sYear;							    //02	//Year
        // Register_19~Register_20
        public short m_sMouth;							    //02	//Month
        // Register_21~Register_22
        public short m_sDay;								//02	//Day
        // Register_23~Register_24
        public short m_sEOSFirmwareVersion;				    //02	//EOS Firmware Version
        // Register_25~Register_26
        public short m_sEOSFirmwareBuildingTime;			//02	//EOS Firmware Building Time
        // Register_27~Register_28
        public short m_sMasterFPGAFirmwareVersion;		    //02	//Master FPGA Firmware Version
        // Register_29~Register_31
        public char m_byMasterFPGAFirmwareBuildingTime1;    //01	//Master FPGA Firmware Building Time1
        public char m_byMasterFPGAFirmwareBuildingTime2;    //01	//Master FPGA Firmware Building Time2
        public char m_byMasterFPGAFirmwareBuildingTime3;    //01	//Master FPGA Firmware Building Time3
        // Register_32~Register_34
        public char m_byMasterFPGAFirmwareStatus1;          //01	//Master FPGA Firmware status1
        public char m_byMasterFPGAFirmwareStatus2;          //01	//Master FPGA Firmware status2
        public char m_byMasterFPGAFirmwareStatus3;          //01	//Master FPGA Firmware status3
        // Register_35~Register_36
        public short m_sMCUFirmwareVersion;				    //02	//MCU Firmware Version
        // Register_37~Register_38
        public short m_sMCUFirmwareBuildingTime;			//02	//MCU Firmware Building Time
        // Register_39~Register_50
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abySysManuInfoReserved;
    };

    // System Status Information：28
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SysStatusInfo
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5~Register_8
        public int m_unTemperature;					    //04	//Temperature
        // Register_9~Register_12
        public int m_unHumidity;						//04	//Humidity
        // Register_13~Register_16
        public int m_unDose;							//04	//Dose
        // Register_17~Register_28
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abySysStatusInfoReserved;
    };

    // Gigabit Ethernet Information：40
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct EtherInfo
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_9~Register_14
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] m_bySourceMAC;                   //06	//Source MAC
        // Register_15~Register_18
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_bySourceIP;                    //04	//Source IP
        // Register_5~Register_6
        public ushort m_sSourceUDPPort;					//02	//Source UDP Port
        // Register_19~Register_24
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] m_byDestMAC;                     //06	//Dest MAC
        // Register_25~Register_28
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_byDestIP;                      //04	//Dest IP
        // Register_7~Register_8
        public ushort m_sDestUDPPort;					//02	//Dest UDP Port	
        // Register_29~Register_40
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abyEtherInfoReserved;
    };

    // High Voltage Generator Interface Trigger Mode Information：21
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct HiVolTriggerModeInfo
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5
        public char m_byPrepSignalValidTrigger;			//01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                                //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
        // Register_6
        public char m_byExposureEnableSignal;			//01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                                //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
        // Register_7
        public char m_byXRayExposureSignalValid;		//01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                                //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid"
        // Register_8
        public char m_bySyncInSignalValidTrigger;		//01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid"
                                                                //03-"Positive Edge Trigger, Negative Edge Invalid"; 04-"Negative Edge Trigger, Positive Edge Invalid";
        // Register_9
        public char m_bySyncOutSignalValidTrigger;		//01	//01-"High Level Trigger, Low Level Invalid"; 02-"Low Level Trigger, High Level Invalid";

        // Register_10~Register_21
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]//wss
        public char[] m_abyEtherInfoReserved;
    };

    // System Configuration Information：127
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SysCfgInfo
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5
        public char m_byWorkMode;						//01	// 01-"Static: Software Mode" 02-"Static: Prep Mode"; 03-"Static Hvg Trigger Mode"; 
                                                                // 04-Static AED Trigger Mode, 05-Dynamic Hvg In Mode,06-Dynamic Fpd Out Mode.
        // Register_6
        public char m_byPreviewModeTransmission;		//01	//00-"Just transmmit full resolution image matrix"; 01-"Just transmmit the preview image"; 
                                                                //02-"Transmmit the preview image and full resolution image"
        // Register_7
        public char m_byPositiveVsNegativeIntegrate;	//01	//01-"Electron"; 02-"Hole" //04-"Static: Inner Trigger Mode"; 05-"Dynamic: Sync In Mode"; 06-"Dynamic: Sync Out Mode"
        // Register_8
        public char m_byBinning;						//01	//01-"1x1"; 02-"2x2 Binning"; 03-"3x3 Binning"; 04-"4x4 Binning";
        // Register_9
        public char m_byIntegratorCapacitorSelection;	//01	//01-"Integrator Capacitor PGA0"; 02-"Integrator Capacitor PGA1"; 03-"PGA2";......;08-"Integrator Capacitor PGA7"
        // Register_10
        public char m_byAmplifierGainSelection;			//01	//01-"Amplifier Gain AMP0"; 02-"Amplifier Gain AMP1"; 03-"Amplifier Gain AMP2"; 04-"Amplifier Gain AMP3";
                                                                //05-"Amplifier Gain AMP4"; 06-"Amplifier Gain AMP5"; 07-"Amplifier Gain AMP6"; 08-"Amplifier Gain AMP7";
        // Register_11
        public char m_bySelfDumpingEnable;				//01	//01-"Enable Dumping Process"; 02-"Disable Dumping Process";
        // Register12
        public char m_bySelfDumpingProcessing;			//01	//01-"Clear Process for Dumping"; 02-"Acquisition Process for Dumping";
        // add 20190705
        public char m_byClearFrameNumber;               //01	//01-"Clear Frame"; 02-"Clear Frame";03-"Clear Frame";04-"Clear Frame";
        // Register_13
        public char m_byPDZ;							//01	//01-"Disable PDZ"; 02-"Enable PDZ"
        // Register_14
        public char m_byNAPZ;							//01	//01-"Disable NAPZ"; 02-"Enable NAPZ"
        // Register15
        public char m_byInnerTriggerSensorSelection;	//01	//00-"No Selection"; 01-"I Sensor Selection"; 02-"II Sensor Selection"; 
                                                                //03-"III Sensor Selection"; 04-"IV Sensor Selection"; 05-"V Sensor Selection"                                                                
        // Register_16~Register_17
        public short m_sZoomBeginRowNumber;				//02	//Zoom Begin Row Number
        // Register_18~Register_19
        public short m_sZoomEndRowNumber;				//02	//Zoom End Row Number
        // Register_20~Register_21
        public short m_sZoomBeginColumnNumber;			//02	//Zoom Begin Column Number
        // Register_22~Register_23
        public short m_sZoomEndColumnNumber;			//02	//Zoom End Column Number
        // Register24~Register27
        public int m_unSelfDumpingSpanTime;			    //04	//unit: ms
        // Register28~Register31
        public int m_unContinuousAcquisitionSpanTime;	//04	//unit: ms
        // Register32~Register35
        public int m_unPreDumpingDelayTime;			    //04	//unit: ms
        // Register36~Register39
        public int m_unPreAcquisitionDelayTime;		    //04	//unit: ms
        // Register40~Register43
        public int m_unPostDumpingDelayTime;			//04	//unit: ms
        // Register44~Register47
        public int m_unPostAcquisitionDelayTime;		//04	//unit: ms
        // Register48~Register51
        public int m_unSyncInExposureWindowTime;		//04	//unit: ms
        // Register52~Register55
        public int m_unSyncOutExposureWindowTime;		//04	//unit: ms
        // Register56~Register59
        public int m_unTFTOffToIntegrateOffTimeSpan;	//04	//unit: ms
        // Register_60~Register_63
        public int m_unIntegrateTime;					//04	//unit: ms
        // Register_64~Register_67
        public int m_unPreFrameDelay;					//04	//unit: ms
        // Register_68~Register_71
        public int m_unPostFrameDelay;					//04	//unit: ms
        // Register_72~Register_75
        public int m_unPreRowDelay;					    //04	//unit: ms
        // Register_76~Register_79
        public int m_unPostRowDelay;					//04	//unit: ms
        // Register_80~Register_127
        public short m_sHvgRdyMode;                     //02	//0-"Edge Trigger"; 非0-"[1~255]:Config Delay,unit:n*128 ms"
        public short m_sSaecPreRdyTm;                   //02	//SAEC pre ready time
        public short m_sSaecPostRdyTm;                  //02	//SAEC post ready time
        public char m_byDefectThreshold;                //01	//Defect correction:Calculating the threshold of bad points
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
        public char[] m_abySysCfgInfoReserved;
    };
    // Image Calibration Configuration：20
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ImgCaliCfg
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;

        public byte m_byOffsetCorrection;
        //[FieldOffset(1)]
        public byte m_byGainCorrection;
        //[FieldOffset(2)]
        public byte m_byDefectCorrection;
        // Register_8
        //[FieldOffset(3)]
        public byte m_byDummyCorrection;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abyImgCaliCfgReserved;
    };

    // Voltage Adjust Configuration：48
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct VolAdjustCfg
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5~Register_8
        public int m_unVCOM;							//04	//VCOM
        // Register_9~Register_12
        public int m_unVGG;							    //04	//VGG
        // Register_13~Register_16
        public int m_unVEE;							    //04	//VEE
        // Register_17~Register_20
        public int m_unVbias;							//04	//Vbias	
        // Register_21~Register_36
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] m_unComp;						    //04	//Comp1	// Register114
        // Register_37~Register_48
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] m_abyVolAdjustCfgReserved;		//12//////Registers 114(include) Are Reserved;
    };

    // TI COF Parameter Configuration：84
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TICOFCfg
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5~Register_64
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public short[] m_sTICOFRegister;
        // Register_65~Register_84
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] m_abyTICOFCfgReserved;
    };

    //CMOS Parameter Configuration：116
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct CMOSCfg
    {
        // Register_1~Register_4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] m_byHead;
        // Register_5~Register_68
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public short[] m_sCMOSRegister;
        // Register_69~Register_116
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public char[] m_abyCMOSCfgReserved;
    };

    // ADI COF Parameter Configuration：390
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ADICOFCfg
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        // Register_1~Register_4
        public char[] m_byHead;
        // Register_5~Register_19
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public short[] m_sADICOFRegister;
        // Register_20~Register_375  355
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 355)]
        public char[] m_abyADICOFCfgReserved;
    };

    // 1024 byte regedit
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
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
    };
    #endregion
}