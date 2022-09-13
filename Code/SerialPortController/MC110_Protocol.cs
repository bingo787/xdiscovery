using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace SerialPortController
{


    /// <summary>
    /// 端口通讯控制中心
    /// </summary>
    public class MC110Protocol
    {
        const byte STX = 0x3C;
        const byte CR = 0x3E;
        const byte SP = 0x20;
        Char StartTag = (Char)(STX);
        Char EndTag = (Char)(CR);
        private static MC110Protocol _instance;
        private bool _running = false;


        public static MC110Protocol Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MC110Protocol();
                return _instance;
            }
        }
        private static string PORTPARAPATH = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ControllerPortParam.xml");

        private MC110Protocol()
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
                while (true)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        if (_serialPort == null)
                            continue;
                        GetHVStatus();
                       

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

            Console.WriteLine("PortPara {0},{1},{2},{3},{4}",PortPara.PortName, PortPara.BaudRate, PortPara.Parity, PortPara.DataBits, PortPara.StopBits);

            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.WriteTimeout = 1000;
            _serialPort.ReadTimeout = 1000;

            _serialPort.Open();

            _running = true;
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
                Console.WriteLine("Receive[" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] " + message);
#endif


                string[] messes = message.Split(EndTag);
                if (messes.Length > 0)
                {
                    for (int i = 0; i < messes.Length; i++)
                    {
                        string m = messes[i];

                        if (m.StartsWith(StartTag.ToString()))
                        {
                            string mes = messes[i].TrimStart(StartTag);

                            try
                            {
                                ReceivedMessage(mes);
                            }
                            catch (Exception ex) {
                                MessageBox.Show(ex.ToString());
                            }
                           
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
        public event BoolValueChangedDelegate WatchDogEnableChanged;
        public event BoolValueChangedDelegate WatchDogOnChanged;
        public event ReportedHandler StateReported;
        private string _lastCommand = "NO";
        private bool _isNeedFeeddingDog = false;
        public bool IsWarming  = false;


        public string TS_Step = "";
        public string TS_Volt_Step="";
        public string TS_Pwr_Step = "";
        public string TS_Elapsed_Time = "";

        /// <summary>
        /// 收到命令答复
        /// </summary>
        /// <param name="message"></param>
        private void ReceivedMessage(string message)
        {

            string arg;
            int temp;
            string RES_OK = "[ERR:0]";
          //  Console.WriteLine("[ReceivedMessage][" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] " + message + " lastComand:" + _lastCommand);
            ///处理错误代码
            //if (message.StartsWith("[ERR:"))
            {
                /// 错误码
                // ERR: 27, 29]
                // 首先从接收到的信息里面提取错误码，错误码一般都是在最后一个出现，[Err:29]
                /*
                 <STATUS:0,0,0,0,1,2,60000,90,0:6,67,33[ERR:27,29]>
                    <TMP1:35[ERR:27,29]>
                    <STATUS:0,0,0,0,1,2,60000,90,0:10,67,33[ERR:27,29]>
                 */
                string ResponseError = message.Split('[').Last().TrimEnd(']'); // --> ERR:27,29
               // Console.WriteLine("Error Code : {0}", ResponseError);

                string[] ResponseErrorCode = ResponseError.Split(new Char[] { ':', ',' });

               
                string error_message = string.Empty;
                for (int i = 1; i< ResponseErrorCode.Length; i++) {


                    string error_code = ResponseErrorCode[i]; //ERR 27 29
                    switch (error_code)
                    {
                        case "0":
                            {
                                error_message = string.Empty;
                                //if (FaultCleared != null)
                                //{
                                //    FaultCleared();
                                //}
                            }
                            break;
                        case "1": error_message += "Invalid Command"; break;
                        case "2": error_message += "Exceed Maximum Specified Value"; break;
                        case "3": error_message += "Exceed Voltage Limit"; break;
                        case "4": error_message += "Exceed Current Limit"; break;
                        case "5": error_message += "Exceed Power Limit"; break;
                        case "6": error_message += "Heater Disabled"; break;
                        case "7": error_message += "Power Supply Disabled"; break;
                        case "8": error_message += "Ade Voltage Interlock Disabled"; break;
                        case "9": error_message += "Heater Interlock Disabled"; break;
                        case "10": error_message += "Curve Power Same"; break;
                        case "11": error_message += "Heater Stabilizing"; break;
                        case "12": error_message += "Over Voltage Fault"; break;
                        case "13": error_message += "Under Voltage Fault"; break;
                        case "14": error_message += "Over Current Fault"; break;
                        case "15": error_message += "Under Current Fault"; break;
                        case "16": error_message += "Temperature Fault"; break;
                        case "17": error_message += "Arc Event Fault"; break;
                        case "18": error_message += "Heater Fault"; break;
                        case "19": error_message += "Reserved"; break;
                        case "20": error_message += "Reserved"; break;
                        case "21": error_message += "I2C Fault"; break;
                        case "22": error_message += "Reserved"; break;
                        case "23": error_message += "Input Voltage Fault"; break;
                        case "24": error_message += "Input Current Fault"; break;
                        case "25": error_message += "G1 Fault"; break;
                        case "26": error_message += "Reserved"; break;
                        case "27": error_message += "G3 Fault"; break;
                        case "28": error_message += "Temperature Preheat"; break;
                        case "29": error_message += "Tube Conditioning"; break;
                        case "30": error_message += "Tube Conditioning Profile"; break;
                        case "31": error_message += "Loda Date"; break;
                        default: error_message += "Unknow"; break;
                    }
                }

               // System.Console.WriteLine("Error message : " + error_message);
                if (StateReported != null && !string.IsNullOrEmpty(error_message))
                    StateReported(error_message);
            }



            if (_lastCommand.StartsWith("SDATE:") && message == RES_OK)
            {
                System.Console.WriteLine("Set Tube Date Success!!");
            
                _lastCommand = string.Empty;
            }
            else if (_lastCommand.StartsWith("EH:") && message == RES_OK)
            {

                System.Console.WriteLine("Enable Heater Success!!");
                if (Connected != null)
                {
                    System.Console.WriteLine("Connected!!");
                    Connected();
                }
                _lastCommand = string.Empty;
            }
            else if (_lastCommand.StartsWith("EP:") && message == RES_OK)
            {
                bool isXrayOn = _lastCommand[3] == '1';
                if (XRayOnChanged != null)
                {
                    XRayOnChanged(isXrayOn);
                }
                _lastCommand = string.Empty;

            }
            else if (_lastCommand.StartsWith("SAV:") && message == RES_OK)
            {
                arg = _lastCommand.TrimStart("SAV:".ToArray());
                if (int.TryParse(arg, out temp))
                {
                    double kv = temp * 0.001;
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
            else if (message.StartsWith("STATUS:"))
            {

                /**
                 Examples (Tube Conditioning ON):
                1. <STATUS:0,0,1,1,1,2,60000,90,1:45,60000, 150[ERR:29]>

                Command String:
                <GSTAT>
                Response String:
                Tube Conditioning Not Active:
                <STATUS:HT-I, AV-I,HT-E,AV-E, Tube Conditioning Active, Anode Voltage, Tube Current[Err:X]> 
                Tube Conditioning Active:
                <STATUS:HT-I, AV-I,HT-E,AV-E, Tube Conditioning Active, TS Volt Step, TS Pwr Step, TS Elapsed Time, 
                Anode 
                Voltage, Tube Current[Err:X]>
                Examples (Tube Conditioning OFF):
                1. Status when the Interlocks are active.
                A. <STATUS:1,1,0,0,0,0,0[ERR:8,9]>
                2. Status when the heater is enabled and is warming up.
                A. <STATUS:0,0,1,0,0,0,0[ERR:11]>
                3. Status when the power supply is in active operation.
                A. <STATUS:0,0,1,1,0,30000,3640[ERR:0]>
                Examples (Tube Conditioning ON):
                1. <STATUS:0,0,1,1,1,2,60000,90,1:45,60000, 150[ERR:29]>

                 */

                string[] msg = message.Split(',');
                string TubeContionActivate = msg.ElementAt(4);
                string AV = "";
                string TC = "";

                if ("1" == TubeContionActivate)
                {
                    IsWarming = true;
                  //  Console.WriteLine("Tube Conditioning ON");

                    TS_Step = msg.ElementAt(5);
                    TS_Volt_Step = msg.ElementAt(6);
                    TS_Pwr_Step = msg.ElementAt(7);
                    TS_Elapsed_Time = msg.ElementAt(8);
                    AV = msg.ElementAt(9);
                    TC = msg.ElementAt(10).Split('[').ElementAt(0);
                }
                else
                {
                    IsWarming = false;
                   // Console.WriteLine("Tube Conditioning OFF");
                    AV = msg.ElementAt(5);
                    TC = msg.ElementAt(6).Split('[').ElementAt(0);
                   
                }

                // 调用回调函数显示
              //  Console.WriteLine("AV {0},TC {1}",AV,TC);
                if (int.TryParse(AV, out temp))
                {
                    double value = temp / 1000.0f;
                    if (VoltageChanged != null)
                        VoltageChanged(value);
                }

                if (int.TryParse(TC, out temp))
                {
                    if (CurrentChanged != null)
                        CurrentChanged((uint)temp);
                }
            }
            else if (message.StartsWith("TC Date:")) {

                // 7-30
                // 30-90
                // 大于90 
                // 75 min , 47 min. 16 min
                // TC Date:30:7:2022[Error,xx,xx]
                try
                {
                    string[] date = message.Split((new Char[] { ':', '[' }));
                    DateTime last = Convert.ToDateTime(date[3] + "-" + date[2] + "-" + date[1]);


                    DateTime current = DateTime.Now;
                    TimeSpan sp = current.Subtract(last);
                    int days = sp.Days;
                    Console.WriteLine(string.Format("上次日期:{0}，当前日期:{1} 间隔了{2}天", last.ToString(), current.ToLocalTime(),days.ToString()) );


                }
                catch {


                }
              
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
             //   Console.WriteLine("上一次的命令是 " + _lastCommand);
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
            SendCommand("CLERR");
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

            SendCommand("GTMP:1");
            Thread.Sleep(1000);
            SendCommand("GSTAT");

        }

        public void Connect()
        {
            /*
            1 Send the current date and wait for conditioning to complete – <SDATE:DD,MM,YYYY>
            2 Program the Anode Voltage Level – <SAV:XXXXXX>
            3 Program Power Level - <SPWR:XXXX>
            4 Enable the Heater - <EH:1>
            5 Enable the Power Supply - <EP:1>
            */




            DateTime dt = DateTime.Now;
            string year = dt.Year.ToString();
            string day = dt.Day.ToString();
            string month = dt.Month.ToString();
            string date = day + "," + month + "," + year;

           // date = "31,12,2023";
            Console.WriteLine("SDATE {0}", date);
           
            SendCommand("SDATE:" + date);
            Thread.Sleep(200);
          
            // 为了获取热机的时间
           // SendCommand("GDATE");
            //Thread.Sleep(200);
            //SendCommand("SAV:1");
            //Thread.Sleep(200);
            //SendCommand("SPWR:1");
            //Thread.Sleep(200);
            SendCommand("EH:1");
            //Thread.Sleep(200);
            //SendCommand("EP:1");

        }
    }
}
