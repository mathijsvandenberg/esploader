using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace ESPLoader
{
    class COMPort : InterfacePort
    {
        static SerialPort _serialPort;

        //events
        public event System.EventHandler<EventArgs> DataArrived;

        //constructor opens the comm port
        public COMPort()
        {
            _serialPort = new SerialPort();
            _serialPort.ReadTimeout = 10000;
            _serialPort.WriteTimeout = 10000;
            _serialPort.DataReceived += DataReceived;
        }

        public override int OpenPort(string port_name, int baud_rate)
        {
            _serialPort.PortName = port_name.Substring(6);
            _serialPort.BaudRate = baud_rate;
            _serialPort.Open();
            return 0;
        }


        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DataArrived != null)
                DataArrived(this, EventArgs.Empty);
        }

        //destructor to make sure the serial port is closed
        ~COMPort()
        {
            if(_serialPort.IsOpen)
                _serialPort.Close();
        }

        public override string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }


        public override void Open()
        {
            _serialPort.Open();
        }

        public override int Close()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
            return 0;
        }

        public override void ChangeTimeouts(int read_timeout, int write_timeout)
        {
            _serialPort.ReadTimeout = read_timeout;
            _serialPort.WriteTimeout = write_timeout;
        }

        public override void ChangeBaudRate(int new_baud_rate)
        {
            Close();

            _serialPort.BaudRate = new_baud_rate;

            _serialPort.Open();
        }

        public override void DiscardBuffers()
        {
            try
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
            }
            catch
            {
                _serialPort.Close();
                Thread.Sleep(500);
                _serialPort.Open();
                DiscardBuffers();
            }

        }

        public override bool write(byte[] buffer, int offset, int count)
        {
            try
            {
                _serialPort.Write(buffer, offset, count);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public override string read()
        {
            int len = _serialPort.BytesToRead;
            char[] buffer = new char[len];
            _serialPort.Read(buffer, 0, len);
            string result = new string(buffer);
            return result;
        }

        public override byte[] read(int offset, int count)
        {
            byte[] buffer = new byte[count];
            
            try
            {
                _serialPort.Read(buffer, offset, count);
                return buffer;
            }
            catch
            {
                return null;
            }
        
        }        

        //send a string over the com port
        public override void WriteLine(string message)
        {
            _serialPort.WriteLine(message);
        }

        //try reading a string from the com port
        public override string ReadLine()
        {
            string message;

            try
            {
                message = _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                message = "[TIMEOUT]";
            }

            return message;
        }
    }
}
