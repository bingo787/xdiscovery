using System;
using System.Windows;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SerialPortController
{


    /// <summary>
    /// 端口通讯控制中心
    /// </summary>
    public class SerialPortControler_RS232PROTOCOL_MC110
    {
        const byte STX = 0x3C;
        const byte CR = 0x3E;
        const byte SP = 0x20;
        Char StartTag = (Char)(STX);
        Char EndTag = (Char)(CR);
        private static SerialPortControler_RS232PROTOCOL_MC110 _instance;
        private bool _running = true;

        public static SerialPortReporter_RS485PROTOCOL_PLC PostionReporter = SerialPortReporter_RS485PROTOCOL_PLC.Instance;

        public static SerialPortControler_RS232PROTOCOL_MC110 Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SerialPortControler_RS232PROTOCOL_MC110();
                return _instance;
            }
        }
        private static string PORTPARAPATH = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ControllerPortParam.xml");

        private SerialPortControler_RS232PROTOCOL_MC110()
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
                while (_running)
                {
                    try
                    {
                        if (_serialPort == null)
                            continue;
                        GetHVStatus();
                        Thread.Sleep(1000);

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
                Console.WriteLine("Receive-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + message);
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
                     //  Console.WriteLine("ReceiveMid-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + m);
#endif
                        if (m.StartsWith(StartTag.ToString()))
                        {
                            string mes = messes[i].TrimStart(StartTag);
                            ReceivedMessage(mes);
#if DEBUG
                           // Console.WriteLine("ReceiveCommand-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + mes);
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
        private bool _isNeedFeeddingDog = false;
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

            string RES_OK = "[ERR:0]";

            Console.WriteLine("[ReceivedMessage][" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] " + message + " lastComand:" + _lastCommand);


            if (_lastCommand.StartsWith("EH:") && message == RES_OK)
            {
                isXRayEnabled = _lastCommand[3] == '1';
                if (Connected != null)
                {
                    Connected();
                }
                System.Console.WriteLine("Enable Heater Success!!");
                _lastCommand = string.Empty;
            }
            else if (_lastCommand.StartsWith("EP:") && message == RES_OK)
            {
                isXrayOn = _lastCommand[3] == '1';
                if (XRayEnableChanged != null)
                {
                    XRayEnableChanged(isXrayOn);
                }
                _lastCommand = string.Empty;

                if (isXrayOn)
                {
                    PostionReporter.SendCommand("1");
                }
                else
                {
                    PostionReporter.SendCommand("0");
                }


            }
            else if (_lastCommand.StartsWith("SAV:") && message == RES_OK)
            {
                arg = _lastCommand.TrimStart("SAV:".ToArray());
                if (int.TryParse(arg, out temp))
                {
                    kv = temp * 0.001;
                    if (VoltageSettingChanged != null)
                        VoltageSettingChanged(kv);
                }
                _lastCommand = string.Empty;
            }
            else if (_lastCommand.StartsWith("SPWR:") && message == RES_OK)
            {
                arg = _lastCommand.TrimStart("SPWR:".ToArray());
                if (int.TryParse(arg, out temp))
                {
                    //todo: 这里需要好好显示下
                    
                     if (CurrentSettingChanged != null)
                        CurrentSettingChanged((uint)temp);  //W
                }
                _lastCommand = string.Empty;
            }
            else if (message.StartsWith("AV:") && message.EndsWith(RES_OK))
            {
                //< A V: 3 9 0 0 0 [E R R: 0] >   单位V
                arg = message.TrimStart("AV:".ToArray()).Replace(RES_OK, "");
                if (int.TryParse(arg, out temp))
                {
                    double value = temp / 1000.0f;
                    if (VoltageChanged != null)
                        VoltageChanged(value);
                }
                _lastCommand = string.Empty;
            }
            else if (message.StartsWith("TC:") && message.EndsWith(RES_OK))
            {
                //< HC: 4 5 0 0[E R R: 0] > 单位0.1uA
                arg = message.TrimStart("TC:".ToArray()).Replace(RES_OK, "");
                if (int.TryParse(arg, out temp))
                {
                    if (CurrentChanged != null)
                        CurrentChanged((uint)temp);
                }
                _lastCommand = string.Empty;
            }
            else if (message.StartsWith("HC:") && message.EndsWith(RES_OK)) {
                //< HC: 4 5 0 0[E R R: 0] > 单位mA
                arg = message.TrimStart("HC:".ToArray()).Replace(RES_OK, "");
                if (int.TryParse(arg, out temp))
                {
                    if (FilamentMonitorChanged != null)
                        FilamentMonitorChanged((uint)temp);
                }
                _lastCommand = string.Empty;

            }
            else if (message.StartsWith("TMP1:") && message.EndsWith(RES_OK))
            {
                //< TMP1: - 1 0 [E R R: 0] > 单位C
                arg = message.TrimStart("TMP1:".ToArray()).Replace(RES_OK, "");
                if (int.TryParse(arg, out temp))
                {
                    int value = temp;
                    if (TemperatureChanged != null)
                        TemperatureChanged(value);
                }
                _lastCommand = string.Empty;
            }

           if (message.StartsWith("[ERR:"))
            {
                /// 错误码

                string error_code = message.Split(':').ElementAt(1).TrimEnd(']');
                string error_message = string.Empty;
                switch (error_code)
                {
                    case "0":
                        {
                            // error_message = "No Error";
                            if (FaultCleared != null)
                            {
                                FaultCleared();
                            }
                        }
                        break;
                    case "1": error_message = "Invalid Command"; break;
                    case "2": error_message = "Exceed Maximum Specified Value"; break;
                    case "3": error_message = "Exceed Voltage Limit"; break;
                    case "4": error_message = "Exceed Current Limit"; break;
                    case "5": error_message = "Exceed Power Limit"; break;
                    case "6": error_message = "Heater Disabled"; break;
                    case "7": error_message = "Power Supply Disabled"; break;
                    case "8": error_message = "Ade Voltage Interlock Disabled"; break;
                    case "9": error_message = "Heater Interlock Disabled"; break;
                    case "10": error_message = "Curve Power Same"; break;
                    case "11": error_message = "Heater Stabilizing"; break;
                    case "12": error_message = "Over Voltage Fault"; break;
                    case "13": error_message = "Under Voltage Fault"; break;
                    case "14": error_message = "Over Current Fault"; break;
                    case "15": error_message = "Under Current Fault"; break;
                    case "16": error_message = "Temperature Fault"; break;
                    case "17": error_message = "Arc Event Fault"; break;
                    case "18": error_message = "Heater Fault"; break;
                    case "19": error_message = "Reserved"; break;
                    case "20": error_message = "Reserved"; break;
                    case "21": error_message = "I2C Fault"; break;
                    case "22": error_message = "Reserved"; break;
                    case "23": error_message = "Input Voltage Fault"; break;
                    case "24": error_message = "Input Current Fault"; break;
                    case "25": error_message = "G1 Fault"; break;
                    case "26": error_message = "Reserved"; break;
                    case "27": error_message = "G3 Fault"; break;
                    case "28": error_message = "Temperature Preheat"; break;
                    default: error_message = "Unknow"; break;
                }

                System.Console.WriteLine("Error message : " + error_message);
                if (StateReported != null && !string.IsNullOrEmpty(error_message))
                    StateReported(error_message);
            }

            return;
           
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

            if (!string.IsNullOrEmpty(_lastCommand)) {
                Console.WriteLine("上一次的命令是 " + _lastCommand);
                Thread.Sleep(300);
            }
           
            _lastCommand = message;

            if (_serialPort.IsOpen)
            {
                _serialPort.Write(command.ToArray(), 0, command.Count);
            }
            _isNeedFeeddingDog = false;
#if DEBUG
           Console.WriteLine("Send [" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] " + message);
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
           // SendCommand("UPDT");
        }
        /// <summary>
        /// 预热
        /// </summary>
        public void Preheat(double kv, double p)
        {
            new Thread(new ThreadStart(delegate
            {
                System.Threading.Thread.Sleep(200);
                SetKV(kv);
                System.Threading.Thread.Sleep(200);
                SetPower(p);
                System.Threading.Thread.Sleep(200);
                XRayOn();

            })).Start();

        }
        public void MC110UpdateCMD(double kv, double p) {

            Console.WriteLine("更新光源设置参数 电压：{0}， 功率：{1}",kv,p);

            new Thread(new ThreadStart(delegate
            {
                System.Threading.Thread.Sleep(200);
                SetKV(kv);
                System.Threading.Thread.Sleep(200);
                SetPower(p);
                System.Threading.Thread.Sleep(200);
                SendCommand("UPDT");

            })).Start();
        }
        bool isStabled = false;
        public void XRayOn()
        {
            Console.WriteLine("执行打开光源");
            SendCommand("EP:1");
        }
        public void XRayOff()
        {
            Console.WriteLine("执行关闭光源");
            SendCommand("EP:0");
        }

        /// <summary>
        /// 设置KV
        /// </summary>
        /// <param name="kv"></param>
        public void SetKV(double kv)
        {
            if (kv < 0)
                kv = 0;
            uint v = ((uint)(kv * 1000));
            Console.WriteLine("设置电压为：{0} v", v);
            SendCommand("SAV:" + v.ToString());
        }
        /// <summary>
        /// 设置电流
        /// </summary>
        /// <param name="ua"></param>
        public void SetCurrent(int ua)
        {
            // MC110光源不支持设置电流
        }
        /// <summary>
        /// 设置功率
        /// </summary>
        /// <param name="p"></param>
        public void SetPower(double p)
        {
            if (p < 0)
                p = 0;
            uint power = ((uint)(p * 10));
            Console.WriteLine("设置功率为：{0}",power);
            SendCommand("SPWR:" + power.ToString());
        }

        /// <summary>
        /// 获取高压状态
        /// </summary>
        public void GetHVStatus()
        {
            /*
             Get Anode Voltage Monitor  <GAVM>   20000-110000    1V 
             Get Tube Current Monitor   <GTCM> 0-3750     0.1uA
             Get Heater Voltage Monitor  <GHVM>     0-10000     1mV 
             Get Heater Current Monitor  <GHCM>     0-1000     1mA 
             Get Grid 1 Voltage Monitor  <GG1V> 0-2500     10mV
             Get Grid 1 Current Monitor  <GG1C> 0-5000     1uA 
             Get Grid 2 Voltage Monitor  <GG2V> 0-1000 1V 
             Get Grid 2 Current Monitor  <GG2C> 0-5000     1uA 
             Get Grid 3 Voltage Monitor  <GG3V> 0-1500 1V Get 
             Grid 3 Current Monitor  <GG3C> 0-5000     1uA 
             Get Date Code <GDC> YYWW 
             Get Firmware Version    <GFV> 
             Get Part Number     <GPN> 
             Get Tube Number     <GTN> 
             Get Operating Time    <GOT> H:M:S 
             Get Serial Number    <GSN> 
             Get Temperature    <GTMP:x     x=1,2> °C 
             Enable Bootloader Mode   <EBLM> Get Error Report     <GRPT> 
             */
            SendCommand("GAVM");
            Thread.Sleep(1000);
            SendCommand("GTCM");
            Thread.Sleep(1000);
            SendCommand("GTMP:1");
            Thread.Sleep(1000);
            SendCommand("GHCM");
        }

        public void Connect()
        {
            /*
        POWER SUPPLY START - UP SEQUENCING
        The following sequence is needed at a minimum to start up the power supply. 
            1.Program the Anode Voltage level - < SAV:XXXXXX >
            2.Program Power Level - < SPWR:XXXX >
            3.Enable the Heater - < EH:1 >
            4.Enable the Power Supply - < EP:1 >
            */

            SendCommand("EH:1");
            Thread.Sleep(5 * 1000);
            

        }
    }
}
