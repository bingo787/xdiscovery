using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;


/*
 通信格式采用RS485，两线制。波特率9600，数据位8位，停止位1位，无奇偶校验，
 每秒钟发送2帧数据，每帧数据7个字节，帧头是FE，帧尾是FF
 */


namespace SerialPortController
{
    /// <summary>
    /// 端口通讯控制中心
    /// </summary>
    public class CustomProtocl
    {

        Char StartTag = (Char)(0xFE);
        Char EndTag = (Char)(0xFF);

        public double AxisZDistance_mm;
        private static CustomProtocl _instance;
        private bool _running = true;
        public static CustomProtocl Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CustomProtocl();
                return _instance;
            }
        }
        private static string PORTPARAPATH = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "ReporterPortParam.xml");

        private CustomProtocl()
        {

            try {
                OpenSerialPort();
                // InitilizeHVStatusThread();
            }
            catch (Exception) {

            }
            
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

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(string message)
        {
            List<byte> command = new List<byte>() { 0xFE };
            byte[] cmd = ASCIIEncoding.ASCII.GetBytes(message);
            command.AddRange(cmd);
            command.Add(0xFF);

            if (_serialPort.IsOpen)
            {
                _serialPort.Write(command.ToArray(), 0, command.Count);
            }

#if DEBUG
           Console.WriteLine("Send-" + DateTime.Now.ToString("HH:mm:ss.ffff") + "=" + message);
#endif
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = sender as SerialPort;
            /*
             通信格式采用RS485，两线制。波特率9600，数据位8位，停止位1位，无奇偶校验，每秒钟发送2帧数据
             每帧数据7个字节，帧头是FE，帧尾是FF
             */
            while (_running && _serialPort.BytesToRead > 0)
            {
                Thread.Sleep(30); // 30ms
                byte[] buffer = new byte[1024];
                int len = port.Read(buffer, 0, buffer.Length);
                if (buffer[0] != StartTag || buffer[len - 1] != EndTag) {
                    continue;
                }
                AxisZDistance_mm =  buffer[1]*1000 +  buffer[2]* 100 + buffer[3]*10 + buffer[4] + buffer[5]/10.0;
#if DEBUG
              //  Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") + "AxisZDistance_mm =" + AxisZDistance_mm.ToString());
#endif
            }

        }


    }

}
