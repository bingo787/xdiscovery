using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace BrightVisionSDKSample
{
    public delegate bool FrameProcCbFunc(IntPtr pFrameInShort, ushort iFrameID, int iFrameWidth, int iFrameHeight, IntPtr pParam);
    public class BrightVisionSDK
    {
        const string sSDKDllName = "BrightVisionSDK.dll";

        //Init
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static void Init();

        //SearchforDevice
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static uint SearchforDevice();

        //GetDeviceMAC
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetDeviceMAC(uint i);

        //GetDeviceInfo
        [StructLayout(LayoutKind.Sequential)]
        public struct SDeviceInfo
        {
            public IntPtr pMAC;
            public IntPtr pIP;
            public int iCtrlPort;
            public int iDataPort;
            public IntPtr pMask;
            public IntPtr pGateway;
            public IntPtr pVenderName;
            public IntPtr pModelName;
            public IntPtr pVersion;
            public IntPtr pSerialNumber;
            public bool bReachable;
        };
        [DllImport(sSDKDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void GetDeviceInfo(IntPtr pDeviceMAC, ref SDeviceInfo pDeviceInfo);

        //GetLocalNICInfo
        [StructLayout(LayoutKind.Sequential)]
        public struct SNICInfo
        {
            public IntPtr pMAC;
            public IntPtr pIP;
            public IntPtr pMask;
            public IntPtr pInterfaceName;
            public IntPtr pBroadcast;
        };
        [DllImport(sSDKDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void GetLocalNICInfo(IntPtr pDeviceMAC, ref SNICInfo pNICInfo);

        //ForceIP
        [StructLayout(LayoutKind.Sequential)]
        public struct SForceInfo
        {
            public IntPtr pMAC;
            public IntPtr pIP;
            public IntPtr pMask;
            public IntPtr pGateway;
        };

        [DllImport(sSDKDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool ForceIP(ref SForceInfo pSendValue);

        //SetCurrentDevice
        [DllImport(sSDKDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool SetCurrentDevice(IntPtr pDeviceMAC);

        //OpenCurrentDevice
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool OpenCurrentDevice();

        //GetNumberOfAttribute
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static uint GetNumberOfAttribute();

        //GetAttributeName
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetAttributeName(uint i);

        //GetAttributeType
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetAttributeType(IntPtr pAttrName);

        //GetAttrInt
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrInt(IntPtr pAttrName, ref long iValue, int iAttrLocation);

        //GetAttrMaxInt
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrMaxInt(IntPtr pAttrName, ref long iMaximum);

        //GetAttrMinInt
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrMinInt(IntPtr pAttrName, ref long iMinimum);

        //GetAttrIncInt
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrIncInt(IntPtr pAttrName, ref long iIncrement);

        //GetAttrFloat
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrFloat(IntPtr pAttrName, ref double fValue, int iAttrLocation);

        //GetAttrMaxFloat
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrMaxFloat(IntPtr pAttrName, ref double fMaximum);

        //GetAttrMinFloat
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrMinFloat(IntPtr pAttrName, ref double fMinimum);

        //GetAttrIncFloat
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrIncFloat(IntPtr pAttrName, ref double fIncrement);

        //GetAttrString
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool GetAttrString(IntPtr pAttrName, StringBuilder sAttrString, int iAttrLocation);

        //GetNumberOfEntry
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetNumberOfEntry(IntPtr pAttrName);

        //GetEntryID
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static uint GetEntryID(IntPtr pAttrName, uint i);

        //GetEntryName
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetEntryName(IntPtr pAttrName, uint i);

        //GetEntryNameByID
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetEntryNameByID(IntPtr pAttrName, uint iEntryID);

        //GetAttributeAccessMode
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetAttributeAccessMode(IntPtr pAttrName);

        //SetAttrInt
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool SetAttrInt(IntPtr pAttrName, long iValue, int iAttrLocation);

        //SetAttrFloat
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool SetAttrFloat(IntPtr pAttrName, double iValue, int iAttrLocation);

        //CalibrateDark
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool CalibrateDark(IntPtr pWorkDir, int iFrameNum);

        //CalibrateBright
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool CalibrateBright(IntPtr pWorkDir, int iFrameNum);

        //DownloadCalDataToDevice
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool DownloadCalDataToDevice(int iCalType, bool bFlashEnable, IntPtr pWorkDir, bool bVerify);

        //OpenCalibrate
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool OpenCalibrate();

        //CloseCalibrate
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool CloseCalibrate();

        //CalibrateDefect
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool CalibrateDefect();

        //StartStream
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool StartStream(FrameProcCbFunc pFrameProcCb, IntPtr pCbParam);

        //GetFrame
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetFrame(ref ushort iFrameID, ref int iWidth, ref int iHeight, ref int iPixelBits);

        //GetRawPixelValue
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static ushort GetRawPixelValue(IntPtr pFrame, int iPixelBits, int iPixelIndex);

        //GetFrameInShort
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetFrameInShort(ref ushort iFrameID, ref int iWidth, ref int iHeight);

        //StopStream
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static void StopStream();

        //Uninit
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static void Uninit();

        //GetLastErrorText
        [DllImport(sSDKDllName, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetLastErrorText();

        //GetVersionText
        [DllImport(sSDKDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr GetVersionText();
    }
    class Program
    {
        static bool FrameProc(IntPtr pFrameInShort, ushort iFrameID, int iFrameWidth, int iFrameHeight, IntPtr pParam)
        {
            Console.WriteLine("Get a frame in short bytes ID={0} Width={1} Height={2} from the callback function", iFrameID, iFrameWidth, iFrameHeight);
            return false;
        }
        static void Main(string[] args)
        {
            //Init
            BrightVisionSDK.Init();

            //GetVersion
            //Console.WriteLine("SDK Version: {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetVersionText()));
            Console.WriteLine("SDK Version: {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetVersionText()));

            //SearchforDevice
            uint iDeviceNumber = BrightVisionSDK.SearchforDevice();
            Console.WriteLine("Total number of devices: {0}", iDeviceNumber);

            //Attribute related APIs
            for (uint i = 0; i < iDeviceNumber; i++)
            {
                //Get device MAC
                IntPtr pDeviceMAC = BrightVisionSDK.GetDeviceMAC(i);
                Console.WriteLine("***************************************************************");
                Console.WriteLine("  Device Information:");

                //GetDeviceInfo
                BrightVisionSDK.SDeviceInfo sDeviceInfo = new BrightVisionSDK.SDeviceInfo();
                BrightVisionSDK.GetDeviceInfo(pDeviceMAC, ref sDeviceInfo);
                Console.WriteLine("    MAC={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pMAC));
                Console.WriteLine("    IP={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pIP));
                Console.WriteLine("    Control Port={0:D}", sDeviceInfo.iCtrlPort);
                Console.WriteLine("    Data Port={0:D}", sDeviceInfo.iDataPort);
                Console.WriteLine("    Mask={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pMask));
                Console.WriteLine("    Gateway={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pGateway));
                Console.WriteLine("    VenderName={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pVenderName));
                Console.WriteLine("    ModelName={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pModelName));
                Console.WriteLine("    Version={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pVersion));
                Console.WriteLine("    SerialNumber={0}", Marshal.PtrToStringAnsi(sDeviceInfo.pSerialNumber));
                if (sDeviceInfo.bReachable)
                {
                    Console.WriteLine("    Reachable");
                }
                else
                {
                    Console.WriteLine("    Unreachable");
                }

                //LocalInfo
                BrightVisionSDK.SNICInfo sLocalInfo = new BrightVisionSDK.SNICInfo();
                BrightVisionSDK.GetLocalNICInfo(pDeviceMAC, ref sLocalInfo);
                Console.WriteLine("  Local Network Adapter Information:");
                Console.WriteLine("    MAC={0}", Marshal.PtrToStringAnsi(sLocalInfo.pMAC));
                Console.WriteLine("    IP={0}", Marshal.PtrToStringAnsi(sLocalInfo.pIP));
                Console.WriteLine("    Mask={0}", Marshal.PtrToStringAnsi(sLocalInfo.pMask));
                Console.WriteLine("    InterfaceName={0}", Marshal.PtrToStringAnsi(sLocalInfo.pInterfaceName));
                Console.WriteLine("    Broadcast={0}", Marshal.PtrToStringAnsi(sLocalInfo.pBroadcast));

                //ForceIP
                BrightVisionSDK.SForceInfo sForceInfo = new BrightVisionSDK.SForceInfo();
                sForceInfo.pMAC = sDeviceInfo.pMAC;
                sForceInfo.pIP = sDeviceInfo.pIP;
                sForceInfo.pMask = sLocalInfo.pMask;
                sForceInfo.pGateway = sDeviceInfo.pGateway;
                if (BrightVisionSDK.ForceIP(ref sForceInfo))
                {
                    Console.WriteLine("  ForceIP=OK");
                }
                else
                {
                    Console.WriteLine("  ForceIP={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                //SetCurrentDevice
                if (BrightVisionSDK.SetCurrentDevice(pDeviceMAC))
                {
                    Console.WriteLine("  SetCurrentDevice=OK");
                }
                else
                {
                    Console.WriteLine("  SetCurrentDevice=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //OpenCurrentDevice
                if (BrightVisionSDK.OpenCurrentDevice())
                {
                    Console.WriteLine("  OpenCurrentDevice=OK");
                }
                else
                {
                    Console.WriteLine("  OpenCurrentDevice=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //The following code is to demonstrate how to set or get related parameters of each attribute 
                //Get the number of attributes
                uint iAttributeNumber = BrightVisionSDK.GetNumberOfAttribute();
                Console.WriteLine("  Attribute information:");
                for (uint j = 0; j < iAttributeNumber; j++)
                {
                    //GetAttributeName
                    IntPtr pAttrName = BrightVisionSDK.GetAttributeName(j);
                    Console.WriteLine("++++Name={0}", Marshal.PtrToStringAnsi(pAttrName));
                    //GetAttributeType, GetAttribute, SetAttribute
                    int iAttrType = BrightVisionSDK.GetAttributeType(pAttrName);
                    if (iAttrType == 0)
                    {
                        Console.WriteLine("    Value Type=int");
                        long iValue = 0;
                        if (BrightVisionSDK.GetAttrMaxInt(pAttrName, ref iValue))
                        {
                            Console.WriteLine("    Maximum={0:D}", iValue);
                        }
                        else
                        {
                            Console.WriteLine("    Maximum=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrMinInt(pAttrName, ref iValue))
                        {
                            Console.WriteLine("    Minimum={0:D}", iValue);
                        }
                        else
                        {
                            Console.WriteLine("    Minimum=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrIncInt(pAttrName, ref iValue))
                        {
                            Console.WriteLine("    Increment={0:D}", iValue);
                        }
                        else
                        {
                            Console.WriteLine("    Increment=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrInt(pAttrName, ref iValue, 0))
                        {
                            Console.WriteLine("    Current Value={0:D}", iValue);
                        }
                        else
                        {
                            Console.WriteLine("    Current Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.SetAttrInt(pAttrName, iValue, 0))
                        {
                            Console.WriteLine("    New Value={0:D}", iValue);
                        }
                        else
                        {
                            Console.WriteLine("    New Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                    }
                    else if (iAttrType == 1)
                    {
                        Console.WriteLine("    Value Type=float");
                        double fValue = 0;
                        if (BrightVisionSDK.GetAttrMaxFloat(pAttrName, ref fValue))
                        {
                            Console.WriteLine("    Maximum={0:f}", fValue);
                        }
                        else
                        {
                            Console.WriteLine("    Maximum=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrMinFloat(pAttrName, ref fValue))
                        {
                            Console.WriteLine("    Minimum={0:f}", fValue);
                        }
                        else
                        {
                            Console.WriteLine("    Minimum=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrIncFloat(pAttrName, ref fValue))
                        {
                            Console.WriteLine("    Increment={0:f}", fValue);
                        }
                        else
                        {
                            Console.WriteLine("    Increment=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.GetAttrFloat(pAttrName, ref fValue, 0))
                        {
                            Console.WriteLine("    Current Value={0:f}", fValue);
                        }
                        else
                        {
                            Console.WriteLine("    Current Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.SetAttrFloat(pAttrName, fValue, 0))
                        {
                            Console.WriteLine("    New Value={0:f}", fValue);
                        }
                        else
                        {
                            Console.WriteLine("    New Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                    }
                    else if (iAttrType == 2)
                    {
                        Console.WriteLine("    Value Type=string");
                        StringBuilder sAttrString = new StringBuilder(128);
                        if (BrightVisionSDK.GetAttrString(pAttrName, sAttrString, 0))
                        {
                            Console.WriteLine("    Current Value={0}", sAttrString.ToString());
                        }
                        else
                        {
                            Console.WriteLine("    Current Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                    }
                    else if (iAttrType == 3)
                    {
                        Console.WriteLine("    Value Type=enumeration");
                        Console.WriteLine("    Enumeration Entries:");
                        //Get number of entries
                        int iEntryNumber = BrightVisionSDK.GetNumberOfEntry(pAttrName);
                        for (int k = 0; k < iEntryNumber; k++)
                        {
                            Console.WriteLine("      {0:D}={1}", BrightVisionSDK.GetEntryID(pAttrName, (uint)k), Marshal.PtrToStringAnsi(BrightVisionSDK.GetEntryName(pAttrName, (uint)k)));
                        }

                        long iEntryID = 0;
                        if (BrightVisionSDK.GetAttrInt(pAttrName, ref iEntryID, 0))
                        {
                            Console.WriteLine("    Current Value={0:D} text={1}", iEntryID, Marshal.PtrToStringAnsi(BrightVisionSDK.GetEntryNameByID(pAttrName, (uint)iEntryID)));
                        }
                        else
                        {
                            Console.WriteLine("    Current Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                        if (BrightVisionSDK.SetAttrInt(pAttrName, iEntryID, 0))
                        {
                            Console.WriteLine("    New Value={0:D}", iEntryID);
                        }
                        else
                        {
                            Console.WriteLine("    Current Value=Error:{0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                        }
                    }
                    else
                    {
                        Console.WriteLine("    Value Type=Unsupported");
                    }
                    //GetAttributeAccessMode
                    int iAccessMode = BrightVisionSDK.GetAttributeAccessMode(pAttrName);
                    if (iAccessMode == 0)
                    {
                        Console.WriteLine("    Access Mode=RO");
                    }else{
                        Console.WriteLine("    Access Mode=RW");
                    }
                }
                //The following code is to demonstrate how to set some most commonly used attributes. Before setting, one getting is called to get the default value.
                //Set OffsetX
                long iOffsetX = 0;
                IntPtr pOffsetX = Marshal.StringToHGlobalAnsi("OffsetX");
                if (BrightVisionSDK.GetAttrInt(pOffsetX, ref iOffsetX, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pOffsetX, iOffsetX, 0))
                    {
                        Console.WriteLine("Set OffsetX to {0:D}", iOffsetX);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set OffsetX, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get OffsetX, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pOffsetX);
                //Set OffsetY
                long iOffsetY = 0;
                IntPtr pOffsetY = Marshal.StringToHGlobalAnsi("OffsetY");
                if (BrightVisionSDK.GetAttrInt(pOffsetY, ref iOffsetY, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pOffsetY, iOffsetY, 0))
                    {
                        Console.WriteLine("Set OffsetY to {0:D}", iOffsetY);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set OffsetY, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get OffsetY, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pOffsetY);
                //Set ImageWidth
                long iImageWidth = 0;
                IntPtr pImageWidth = Marshal.StringToHGlobalAnsi("ImageWidth");
                if (BrightVisionSDK.GetAttrInt(pImageWidth, ref iImageWidth, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pImageWidth, iImageWidth, 0))
                    {
                        Console.WriteLine("Set ImageWidth to {0:D}", iImageWidth);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ImageWidth, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ImageWidth, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pImageWidth);
                //Set ImageHeight
                long iImageHeight = 0;
                IntPtr pImageHeight = Marshal.StringToHGlobalAnsi("ImageHeight");
                if (BrightVisionSDK.GetAttrInt(pImageHeight, ref iImageHeight, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pImageHeight, iImageHeight, 0))
                    {
                        Console.WriteLine("Set ImageHeight to {0:D}", iImageHeight);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ImageHeight, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ImageHeight, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pImageHeight);
                //Set Image Binning
                long iImageBinning = 0;
                IntPtr pImageBinning = Marshal.StringToHGlobalAnsi("ImageBinning");
                if (BrightVisionSDK.GetAttrInt(pImageBinning, ref iImageBinning, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pImageBinning, iImageBinning, 0))
                    {
                        Console.WriteLine("Set ImageBinning to {0:D}", iImageBinning);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ImageBinning, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ImageBinning, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pImageBinning);
                //Set ReverseX
                long iReverseX = 0;
                IntPtr pReverseX = Marshal.StringToHGlobalAnsi("ReverseX");
                if (BrightVisionSDK.GetAttrInt(pReverseX, ref iReverseX, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pReverseX, iReverseX, 0))
                    {
                        Console.WriteLine("Set ReverseX to {0:D}", iReverseX);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ReverseX, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ReverseX, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pReverseX);
                //Set ReverseY
                long iReverseY = 0;
                IntPtr pReverseY = Marshal.StringToHGlobalAnsi("ReverseY");
                if (BrightVisionSDK.GetAttrInt(pReverseY, ref iReverseY, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pReverseY, iReverseY, 0))
                    {
                        Console.WriteLine("Set ReverseY to {0:D}", iReverseY);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ReverseY, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ReverseY, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pReverseY);
                //Set Analog Gain
                double fAnalogGain = 0;
                IntPtr pAnalogGain = Marshal.StringToHGlobalAnsi("AnalogGain");
                if (BrightVisionSDK.GetAttrFloat(pAnalogGain, ref fAnalogGain, 0))
                {
                    if (BrightVisionSDK.SetAttrFloat(pAnalogGain, fAnalogGain, 0))
                    {
                        Console.WriteLine("Set AnalogGain to {0:f}", fAnalogGain);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set AnalogGain, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get AnalogGain, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pAnalogGain);
                //Set ExposureTime
                double fExposureTime = 0;
                IntPtr pExposureTime = Marshal.StringToHGlobalAnsi("ExposureTime");
                if (BrightVisionSDK.GetAttrFloat(pExposureTime, ref fExposureTime, 0))
                {
                    if (BrightVisionSDK.SetAttrFloat(pExposureTime, fExposureTime, 0))
                    {
                        Console.WriteLine("Set ExposureTime to {0:f}", fExposureTime);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ExposureTime, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ExposureTime, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pExposureTime);
                //Set AcquisitionMode
                long iAcquisitionMode = 0;
                IntPtr pAcquisitionMode = Marshal.StringToHGlobalAnsi("AcquisitionMode");
                if (BrightVisionSDK.GetAttrInt(pAcquisitionMode, ref iAcquisitionMode, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pAcquisitionMode, iAcquisitionMode, 0))
                    {
                        Console.WriteLine("Set AcquisitionMode to {0:D}", iAcquisitionMode);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set AcquisitionMode, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get AcquisitionMode, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pAcquisitionMode);
                //Set ExtTriggerEdge;
                long iExtTriggerEdge = 0;
                IntPtr pExtTriggerEdge = Marshal.StringToHGlobalAnsi("ExtTriggerEdge");
                if (BrightVisionSDK.GetAttrInt(pExtTriggerEdge, ref iExtTriggerEdge, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pExtTriggerEdge, iExtTriggerEdge, 0))
                    {
                        Console.WriteLine("Set ExtTriggerEdge to {0:D}", iExtTriggerEdge);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set ExtTriggerEdge, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get ExtTriggerEdge, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pExtTriggerEdge);
                //Set FD Mode
                long iFDMode = 0;
                IntPtr pFDMode = Marshal.StringToHGlobalAnsi("FDMode");
                if (BrightVisionSDK.GetAttrInt(pFDMode, ref iFDMode, 0))
                {
                    if (BrightVisionSDK.SetAttrInt(pFDMode, iFDMode, 0))
                    {
                        Console.WriteLine("Set FDMode to {0:D}", iFDMode);
                    }
                    else
                    {
                        Console.WriteLine("Warnning: Fail to set FDMode, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    }
                }
                else
                {
                    Console.WriteLine("Warnning: Fail to get FDMode, {0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                }
                Marshal.FreeHGlobal(pFDMode);
                //Calirate_Dark
                if (BrightVisionSDK.CalibrateDark(IntPtr.Zero, 0))
                {
                    Console.WriteLine("++++CalibrateDark=OK");
                }
                else
                {
                    Console.WriteLine("++++CalibrateDark={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //CalibrateBright
                if (BrightVisionSDK.CalibrateBright(IntPtr.Zero, 0))
                {
                    Console.WriteLine("++++CalibrateBright=OK");
                }
                else
                {
                    Console.WriteLine("++++CalibrateBright={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //DownloadCalDataToDevice
                if (BrightVisionSDK.DownloadCalDataToDevice(2, false, IntPtr.Zero, false))
                {
                    Console.WriteLine("++++DownloadCalDataToDevice=OK");
                }
                else
                {
                    Console.WriteLine("++++DownloadCalDataToDevice={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //OpenCalibrate
                if (BrightVisionSDK.OpenCalibrate())
                {
                    Console.WriteLine("++++OpenCalibrate=OK");
                }
                else
                {
                    Console.WriteLine("++++OpenCalibrate={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //CalibrateDefect
                if (BrightVisionSDK.CalibrateDefect())
                {
                    Console.WriteLine("++++CalibrateDefect=OK");
                }
                else
                {
                    Console.WriteLine("++++CalibrateDefect={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //There are two ways to process frames:
                //1:Use a callback function to process frame
                //Start Stream
                FrameProcCbFunc pCb = new FrameProcCbFunc(FrameProc);
                if (BrightVisionSDK.StartStream(pCb, IntPtr.Zero))
                {
                    Console.WriteLine("++++StartStream=OK");
                }
                else
                {
                    Console.WriteLine("++++StartStream={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //Thread.Sleep(100);
                //StopStream
                BrightVisionSDK.StopStream();
                Console.WriteLine("++++StopStream=OK");
                //2:Use GetFrame or GetFrameInShort to get frame data
                //Start Stream
                if (BrightVisionSDK.StartStream(null, IntPtr.Zero))
                {
                    Console.WriteLine("++++StartStream=OK");
                }
                else
                {
                    Console.WriteLine("++++StartStream={0}", Marshal.PtrToStringAnsi(BrightVisionSDK.GetLastErrorText()));
                    continue;
                }
                //GetFrame
                ushort iFrameID = 0;
                int iWidth = 0;
                int iHeight = 0;
                int iPixelBits = 0;
                IntPtr pFrame = BrightVisionSDK.GetFrame(ref iFrameID, ref iWidth, ref iHeight, ref iPixelBits);
                if (pFrame != IntPtr.Zero)
                {
                    FileStream sFileStream = new FileStream("frame.raw", FileMode.OpenOrCreate, FileAccess.Write);
                    BinaryWriter sBinaryWriter = new BinaryWriter(sFileStream);

                    int iPixelNum = iWidth * iHeight;
                    for (int k = 0; k < iPixelNum; k++)
                    {
                        ushort iPixelValue = BrightVisionSDK.GetRawPixelValue(pFrame, iPixelBits, k);
                        sBinaryWriter.Write(iPixelValue);
                    }
                    sFileStream.Close();
                    Console.WriteLine("Save the frame id={0} to frame.raw", iFrameID);
                }
                else
                {
                    Console.WriteLine("Fail to get one frame");
                }
                //GetFrameInShort
                IntPtr pFrameInShort = BrightVisionSDK.GetFrameInShort(ref iFrameID, ref iWidth, ref iHeight);
                if (pFrameInShort != IntPtr.Zero)
                {
                    int iPixelNum = iWidth * iHeight;
                    byte[] pFrameInByte = new byte[iPixelNum * 2];
                    Marshal.Copy(pFrameInShort, pFrameInByte, 0, iPixelNum * 2);
                    FileStream sFileStream = new FileStream("frame2.raw", FileMode.OpenOrCreate, FileAccess.Write);
                    sFileStream.Write(pFrameInByte, 0, iPixelNum * 2);

                    sFileStream.Close();
                    Console.WriteLine("Save the frame id={0} in short bytes to frame2.raw", iFrameID);
                }
                else
                {
                    Console.WriteLine("Fail to get one frame in short bytes");
                }
                //StopStream
                BrightVisionSDK.StopStream();
                Console.WriteLine("++++StopStream=OK");
            }
            //Uninit
            BrightVisionSDK.Uninit();
            Console.WriteLine("Press any key to exit\n");
            Console.ReadKey();
        }
    }
}
