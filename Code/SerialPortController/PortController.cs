using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SerialPortController
{
    /// <summary>
    /// 端口通讯控制中心
    /// </summary>
    public class SerialPortControler_RS232PROTOCOL
    {
        const byte STX = 0x02;
        const byte CR = 0x0D;
        const byte SP = 0x20;
        Char StartTag = (Char)(STX);
        Char EndTag = (Char)(0x0D);
        private static SerialPortControler_RS232PROTOCOL _instance;
        private bool _running = true;
        public static SerialPortControler_RS232PROTOCOL Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SerialPortControler_RS232PROTOCOL();
                return _instance;
            }
        }
        private static string PORTPARAPATH = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ControllerPortParam.xml");

        private SerialPortControler_RS232PROTOCOL()
        {
            InitilizeHVStatusThread();
        }
        /// <summary>
        /// 初始化高压XRay曝光状态
        /// </summary>
        private void InitilizeHVStatusThread()
        {
            new Thread(new ThreadStart(delegate
            {
                int tag = 1;
                while (_running)
                {
                    try
                    {
                        if (_serialPort == null)
                            continue;

                        //检查是否需要喂狗
                        if (!_isNeedFeeddingDog)
                        {
                            _isNeedFeeddingDog = true;
                            Thread.Sleep(350 * 1);
                            continue;
                        }

                        GetHVStatus();

                        tag++;
                        tag = tag % 2;

                        if (tag == 0)
                        {
                            Thread.Sleep(150 * 1);
                            SendCommand("MON");
                        }
                        else if (tag == 1)
                        {
                            Thread.Sleep(150 * 1);
                            SendCommand("FLT");
                        }
                    }
                    catch { }
                }
            })) { IsBackground = true }.Start();
        }
        #region 字段
        SerialPort _serialPort;
        #endregion

        #region 属性

        /// <summary>
        /// 数据位
        /// </summary>
        public SerialPortController.Setting.PortPara PortPara
        {
            get
            {
                return ConfigHelper.Get<SerialPortController.Setting.PortPara>(PORTPARAPATH);
            }
        }
        #endregion

        /// <summary>
        /// 打开端口
        /// </summary>
        public void OpenSerialPort()
        {
            _serialPort = new SerialPort(PortPara.PortName, PortPara.BaudRate, PortPara.Parity, PortPara.DataBits, PortPara.StopBits);

            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.WriteTimeout = 1000;
            _serialPort.ReadTimeout = 1000;

            _running = true;

            _serialPort.Open();
        }
        /// <summary>
        /// 打开端口
        /// </summary>
        public void OpenSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopbits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopbits);
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.WriteTimeout = 1000;
            _serialPort.ReadTimeout = 1000;
            _serialPort.Open();
        }
        public void CloseControlSystem()
        {
            _running = false;
            _serialPort.Close();
        }
        string _lastMessage = string.Empty;
        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = sender as SerialPort;

            while (_running && _serialPort.BytesToRead > 0)
            {
                Thread.Sleep(30);
                byte[] buffer = new byte[1024];
                int len = port.Read(buffer, 0, buffer.Length);
                string message = ASCIIEncoding.ASCII.GetString(buffer, 0, len);
#if DEBUG
               // Console.WriteLine("Receive-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + message);
#endif
                if (!string.IsNullOrEmpty(_lastMessage))
                    message = _lastMessage + message;
                _lastMessage = string.Empty;

                if (!message.Contains(EndTag) && !message.Contains(EndTag))
                {
                    continue;
                }
                if (message.Contains(StartTag) && !message.Contains(EndTag))
                {
                    _lastMessage = message;
                    continue;
                }
                string[] messes = message.Split(EndTag);
                if (messes.Length > 0)
                {
                    for (int i = 0; i < messes.Length; i++)
                    {
                        string m = messes[i];
#if DEBUG
                       // Console.WriteLine("ReceiveMid-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + m);
#endif
                        if (m.StartsWith(StartTag.ToString()))
                        {
                            string mes = messes[i].TrimStart(StartTag);
                            ReceivedMessage(mes);
#if DEBUG
                        //    Console.WriteLine("ReceiveCommand-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + mes);
#endif

                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

        }

        public delegate void DoubleValueChangedDelegate(double arg);
        public delegate void BoolValueChangedDelegate(bool arg);
        public delegate void UIntValueChangedDelegate(uint arg);
        public delegate void EventHandler();
        public delegate void ReportedHandler(string report);
        /// <summary>
        /// 电压改变
        /// </summary>
        public event DoubleValueChangedDelegate VoltageChanged;
        /// <summary>
        /// 电压改变
        /// </summary>
        public event DoubleValueChangedDelegate VoltageSettingChanged;
        /// <summary>
        /// 温度改变
        /// </summary>
        public event DoubleValueChangedDelegate TemperatureChanged;
        /// <summary>
        /// 射线管灯丝
        /// </summary>
        public event UIntValueChangedDelegate FilamentMonitorChanged;
        /// <summary>
        /// 电流改变
        /// </summary>
        public event UIntValueChangedDelegate CurrentChanged;
        /// <summary>
        /// 电流改变
        /// </summary>
        public event UIntValueChangedDelegate CurrentSettingChanged;
        /// <summary>
        /// 清空状态错误
        /// </summary>
        public event EventHandler FaultCleared;
        public event EventHandler Connected;
        public event BoolValueChangedDelegate XRayOnChanged;
        public event BoolValueChangedDelegate XRayEnableChanged;
        public event BoolValueChangedDelegate WatchDogEnableChanged;
        public event BoolValueChangedDelegate WatchDogOnChanged;
        public event ReportedHandler StateReported;
        private string _lastCommand;
        private bool _isNeedFeeddingDog = true;
        /// <summary>
        /// 收到命令答复
        /// </summary>
        /// <param name="message"></param>
        private void ReceivedMessage(string message)
        {
            string arg;
            int temp;
            double kv, temperature;
            uint ua, filamentMonitor;
            bool isXrayOn;
            bool isXRayEnabled;
            bool isWatchDogEnabled;
            bool isWatchDogOn;

            //Voltage Program  VP  XXXX  <STX>VPXXXX<CR>  XXXX = 000.0 – Max KV 
            if (message.StartsWith("VP"))
            {
                arg = message.TrimStart("VP".ToArray());
                if (int.TryParse(arg, out temp))
                {
                    kv = temp * 0.1;
                    if (VoltageSettingChanged != null)
                        VoltageSettingChanged(kv);
                }
                return;
            }
            //Current Program  CP  XXXX  <STX>CPXXXX<CR>  XXXX = 0000 – Max uA 
            if (message.StartsWith("CP"))
            {
                arg = message.TrimStart("CP".ToArray());
                if (uint.TryParse(arg, out ua))
                {
                    if (CurrentSettingChanged != null)
                        CurrentSettingChanged(ua);
                }
                return;
            }
            /*Voltage/Current/Temperature/Filament Monitor             
             * MON  N/A  
             * <STX>VVVV<SP>CCCC<SP>TTTT<SP> FFFF<CR> 
                VVVV = 000.0–Max KV 
                CCCC = 0000 –Max uA 
                TTTT = 000.0 – 070.0 
                DegC 
                FFFF = 0000 – 4095 **/
            //Report Fault  FLT  N/A  <STX>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<CR> 
            //Report Fault(Pulsed Sources only) FLT  N/A  <STX>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<SP>X<CR> 
            if (message.Contains(" "))
            {
                string[] args = message.Split(' ');
                if (args.Length == 4)
                {
                    if (int.TryParse(args[0], out temp))
                    {
                        kv = temp * 0.1;
                        if (VoltageChanged != null)
                            VoltageChanged(kv);
                    }
                    if (uint.TryParse(args[1], out ua))
                    {
                        if (CurrentChanged != null)
                            CurrentChanged(ua);
                    }
                    if (int.TryParse(args[2], out temp))
                    {
                        temperature = temp * 0.1;
                        if (TemperatureChanged != null)
                            TemperatureChanged(temperature);
                    }
                    if (uint.TryParse(args[3], out filamentMonitor))
                    {
                        if (FilamentMonitorChanged != null)
                        {
                            FilamentMonitorChanged(filamentMonitor);
                        }
                    }
                }
                if (args.Length == 9 || args.Length == 10)
                {
                    string errorMess = string.Empty;
                    string[] ErrorInfo = new string[]
                    { 
                    "Regulation",
                    "Interlock Open",
                    "Cathode Over KV Fault",
                    "Anode Over KV Fault",
                    "Over Temperature Fault",
                    "Arc Fault",
                    "Over Current Fault",
                    "Power Limit Fault",
                    "Over Voltage Fault",
                    "Duty Cycle mode ON",
                    };

                    string errorCode = "1";
                    for (int i = 0; i < 9; i++)
                    {
                        if (args[i] == errorCode)
                            errorMess += ErrorInfo[8 - i] + "!";
                    }
                    if (args.Length == 10 && args[9] == errorCode)
                        errorMess += "Duty Cycle mode ON";
                    if (StateReported != null && !string.IsNullOrEmpty(errorMess))
                        StateReported(errorMess);

                }
                return;
            }
            //Fault Clear  CLR  N/A  <STX>CLR<CR>   
            if (message.StartsWith("CLR"))
            {
                if (FaultCleared != null)
                {
                    FaultCleared.Invoke();
                }
                return;
            }

            if (message == "1" || message == "0")
            {
                if (_lastCommand == "WSTAT")
                {
                    isWatchDogOn = message == "1";
                    if (WatchDogOnChanged != null)
                    {
                        WatchDogOnChanged(isWatchDogOn);
                    }
                    _lastCommand = string.Empty;
                    return;
                }
                else if (_lastCommand == "STAT")//HV Status  STAT  N/A  <STX>X<CR>  X = 1 is X-Ray On X = 0 is X-Ray Off 
                {
                    isXrayOn = message == "1";
                    if (XRayOnChanged != null)
                    {
                        XRayOnChanged(isXrayOn);
                    }
                    _lastCommand = string.Empty;
                    return;
                }
            }

            //X-Ray Enable   ENBL  X  <STX>ENBLX<CR>  X = 1 Enable X-Ray X = 0 Disable X-Ray 
            if (message.StartsWith("ENBL"))
            {
                arg = message.TrimStart("ENBL".ToArray());
                isXRayEnabled = arg == "1";
                if (XRayEnableChanged != null)
                {
                    XRayEnableChanged(isXRayEnabled);
                }
                return;
            }
            //Watch Dog Timer  WDTE  N/A  <STX>OK<CR> 
            if (message.StartsWith("OK"))
            {
                return;
            }
            //Comm Port Echo  FREV  N/A  <STX>XNNN<CR>  XNNN = 2000 
            if (message.StartsWith("2000"))
            {
                Connected.Invoke();
                return;
            }

            if (message.StartsWith("WDOG"))
            {
                arg = message.TrimStart("WDOG".ToArray());
                if (WatchDogEnableChanged != null)
                {
                    WatchDogEnableChanged(arg == "1");
                }
                return;
            }


        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(string message)
        {
            List<byte> command = new List<byte>() { STX };
            byte[] cmd = ASCIIEncoding.ASCII.GetBytes(message);
            command.AddRange(cmd);
            command.Add(CR);
            _lastCommand = message;

            if (_serialPort.IsOpen)
            {
                _serialPort.Write(command.ToArray(), 0, command.Count);
            }
            _isNeedFeeddingDog = false;
#if DEBUG
         //   Console.WriteLine("Send-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + message);
#endif
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(byte[] message)
        {
            List<byte> command = new List<byte>() { STX };
            command.AddRange(message);
            command.Add(CR);

            _serialPort.Write(command.ToArray(), 0, command.Count);
        }
        /// <summary>
        /// 关闭端口
        /// </summary>
        public void ClosePort()
        {
            try
            {
                _serialPort.Close();
                //ConfigHelper.Save(PORTPARAPATH, PortPara);
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 设定采集张数
        /// </summary>
        /// <param name="times"></param>
        public void SetGrabFrames(Int32 times)
        {
            //WriteMultiRegister(byte.Parse(PortPara.SetGrabFramesADD), BitConverter.GetBytes(times));
        }
        /// <summary>
        /// 设定曝光时间
        /// </summary>
        /// <param name="times"></param>
        public void SetGrabTime(ushort time)
        {
            //WriteMultiRegister(byte.Parse(PortPara.SetGrabTimeADD), BitConverter.GetBytes(time));
        }
        /// <summary>
        /// 设定帧时间
        /// </summary>
        /// <param name="times"></param>
        public void SetFrameTime(ushort time)
        {
            //WriteMultiRegister(byte.Parse(PortPara.SetFrameTimeADD), BitConverter.GetBytes(time));
        }
        /// <summary>
        /// 开始采集
        /// </summary>
        public void StartGrab()
        {
            //WriteSingleRegister(byte.Parse(PortPara.StartAdd), 0);
        }
        /// <summary>
        /// 结束采集
        /// </summary>
        public void StopGrab()
        {
            //WriteSingleRegister(byte.Parse(PortPara.StartAdd), 1);
        }

        ///// <summary>
        ///// 合成指令
        ///// </summary>
        ///// <param name="addr"></param>
        ///// <param name="val"></param>
        //public void WriteSingleRegister(ushort addr, byte val)
        //{
        //    byte[] buf = { 0x7F, 0x01, 0x02, 0x03, 0x03, 0x00, 0x00, 0x00, 0x00 };

        //    buf[5] = (byte)(addr & 0xff);
        //    buf[6] = (byte)(addr >> 8);
        //    buf[7] = val;

        //    buf[8] = GetCheckSum(buf, 7);

        //    _serialPort.Write(buf, 0, buf.Length);
        //}
        ///// <summary>
        ///// 合成多位数据指令
        ///// </summary>
        ///// <param name="addr"></param>
        ///// <param name="buffer"></param>
        //public void WriteMultiRegister(ushort addr, byte[] buffer)
        //{
        //    int len = buffer.Length;
        //    if (len > 64) return;

        //    byte[] vBuf = new byte[128];
        //    byte[] cmd = { 0x7F, 0x01, 0x02, 0x04, 0x05, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };
        //    cmd.CopyTo(vBuf, 0);

        //    vBuf[4] = (byte)(3 + len);
        //    vBuf[5] = (byte)(addr & 0xff);
        //    vBuf[6] = (byte)(addr >> 8);
        //    vBuf[7] = (byte)len;

        //    buffer.CopyTo(vBuf, 8);

        //    vBuf[len + 8] = GetCheckSum(vBuf, len + 7);

        //    _serialPort.Write(vBuf, 0, buffer.Length + 9);
        //}
        /// <summary>
        /// 计算校验位数值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public byte GetCheckSum(byte[] buffer, int len)
        {
            byte sum = 0;

            for (int i = 0 + 1; i < len + 1; i++)
            {
                sum += buffer[i];
            }

            sum = (byte)(~sum + 1);

            return sum;
        }
        /// <summary>
        /// 高压复位
        /// </summary>
        public void ResetHV()
        {
            SendCommand("CLR");
        }
        /// <summary>
        /// 预热
        /// </summary>
        public void Preheat(double kv, int ua)
        {
            new Thread(new ThreadStart(delegate
            {
                System.Threading.Thread.Sleep(200);
                SetKV(kv);
                System.Threading.Thread.Sleep(200);
                SetCurrent(ua);
                System.Threading.Thread.Sleep(200);
                XRayOn();
            })).Start();

        }
        public void XRayOn()
        {
            SendCommand("ENBL1");
        }
        public void XRayOff()
        {
            SendCommand("ENBL0");
        }
        /// <summary>
        /// 设置KV
        /// </summary>
        /// <param name="kv"></param>
        public void SetKV(double kv)
        {
            if (kv < 0)
                kv = 0;
            SendCommand("VP" + ((int)(kv * 10)).ToString("D4"));
        }
        /// <summary>
        /// 设置电流
        /// </summary>
        /// <param name="ua"></param>
        public void SetCurrent(int ua)
        {
            if (ua < 0)
                ua = 0;
            SendCommand("CP" + ua.ToString("D4"));
        }
        /// <summary>
        /// 获取高压状态
        /// </summary>
        public void GetHVStatus()
        {
            SendCommand("STAT");
        }

        public void Connect()
        {
            SendCommand("FREV");
            Thread.Sleep(150);
            SendCommand("MON");
        }
    }
}
