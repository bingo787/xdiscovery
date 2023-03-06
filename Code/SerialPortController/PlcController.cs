using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortController
{
    public class PlcController
    {

        const byte HEAD_BYTE = 0x55;
        const byte CMD_MOVE_X = 0x01;
        const byte CMD_MOVE_Y = 0x02;
        const byte CMD_MOVE_Z = 0x03;

        private static PlcController _instance;
        #region 字段
        SerialPort _serialPort;
        #endregion

        public static PlcController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PlcController();
                return _instance;
            }
        }
        private PlcController()
        {
            try
            {

                OpenSerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

            }
            catch
            {
                MessageBox.Show("COM3 串口打开失败 ");
            }
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

    

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = sender as SerialPort;

            while (_serialPort.BytesToRead > 0)
            {
                Thread.Sleep(30);
                byte[] buffer = new byte[1024];
                int len = port.Read(buffer, 0, buffer.Length);
                Console.WriteLine("response: ");
                for (int i = 0; i < len; i++)
                {
                    Console.Write("0x" + buffer[i].ToString("X") + " ");
                }
                Console.WriteLine("");
            }
        }


        /*
        0x55	0x01	byte1 byte2			X轴运动到指定位置
        0x55	0x02				Y轴运动到指定位置
        0x55	0x03				Z轴运动到指定位置

         */
        byte CalculateXorCheckCode(List<byte> data)
        {

            byte result = data[0];
            for (int i = 1; i < data.Count; i++)
            {
                result ^= data[i];
            }
            return result;
        }
        void Move(byte cmd, double x)
        {
            UInt16 ix = (ushort)(x * 10);

            byte ix_low_byte = (byte)(ix & 0xFF);
            byte ix_high_byte = (byte)((ix >> 8) & 0xFF);
            Console.WriteLine("ix {0}, high {1}, low {2}", ix, ix_high_byte, ix_low_byte);
            List<byte> command = new List<byte>();
            command.AddRange(new byte[] { HEAD_BYTE, cmd, ix_high_byte, ix_low_byte });
            byte checkCode = CalculateXorCheckCode(command);
            command.Add(checkCode);


            Console.WriteLine("command: ");
            for (int i = 0; i < command.Count; i++)
            {
                Console.Write("0x" + command[i].ToString("X") + " ");
            }
            Console.WriteLine("");
            _serialPort.Write(command.ToArray(), 0, command.Count);

        }

        public void MoveTo(double x, double y, double z)
        {

            Move(CMD_MOVE_X, x);
            Thread.Sleep(500);
            // Move(CMD_MOVE_Y, y);
            // Thread.Sleep(500);
            Move(CMD_MOVE_Z, z);
        }

        public void GetXYZ() {
            Thread.Sleep(100);
            List<byte> command = new List<byte>();
            command.AddRange(new byte[] { HEAD_BYTE, 0x07, 0, 0 });
            byte checkCode = CalculateXorCheckCode(command);
            command.Add(checkCode);
            Console.WriteLine("command: ");
            for (int i = 0; i < command.Count; i++)
            {
                Console.Write("0x" + command[i].ToString("X") + " ");
            }
            Console.WriteLine("");
            _serialPort.Write(command.ToArray(), 0, command.Count);
        }

    }
}
