using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ESPLoader
{
    class COMPort
    {

        static SerialPort _serialPort;

        //constructor opens the comm port
        public COMPort(string port_name, int baud_rate )
        {
            _serialPort = new SerialPort();

            _serialPort.PortName = port_name;
            _serialPort.BaudRate = baud_rate;

            _serialPort.ReadTimeout = 10000;
            _serialPort.WriteTimeout = 10000;

            _serialPort.Open();

        }

        //destructor to make sure the serial port is closed
        ~COMPort()
        {
            if(_serialPort.IsOpen)
                _serialPort.Close();
        }

        public void Disconnect()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public void ChangeTimeouts(int read_timeout, int write_timeout)
        {
            _serialPort.ReadTimeout = read_timeout;
            _serialPort.WriteTimeout = write_timeout;
        }

        public void ChangeBaudRate(int new_baud_rate)
        {
            Disconnect();

            _serialPort.BaudRate = new_baud_rate;

            _serialPort.Open();
        }

        public void DiscardBuffers()
        {
            _serialPort.DiscardOutBuffer();
            _serialPort.DiscardInBuffer();
        }

        public void write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }


        public byte[] read(int offset, int count)
        {
            byte[] buffer = new byte[count];
            _serialPort.Read(buffer, offset, count);

            return buffer;
        }

        //send a string over the com port
        public void WriteLine(string message)
        {
            _serialPort.WriteLine(message);
        }

        //try reading a string from the com port
        public string ReadLine()
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
