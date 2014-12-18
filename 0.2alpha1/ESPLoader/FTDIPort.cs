using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTD2XX_NET;

namespace ESPLoader
{
    class FTDIPort:InterfacePort
    {
        static FTDI _ftdiPort;
        FTDI.FT_STATUS status = new FTDI.FT_STATUS();
        uint RxQueue;
        uint numBytesRead = 0;
        uint numBytesWritten = 0;
        int portindex;


        //events
        public event System.EventHandler<EventArgs> DataArrived;

        //constructor opens the comm port
        public FTDIPort()
        {
            _ftdiPort = new FTDI();
            _ftdiPort.SetTimeouts(250,250);
           // _ftdiPort. += DataReceived;
        }
        //destructor to make sure the serial port is closed
        ~FTDIPort()
        {
            if (_ftdiPort.IsOpen)
                _ftdiPort.Close();
        }


        public override int OpenPort(string port_name, int baud_rate)
        {
            portindex = Convert.ToInt32(port_name.Substring(6, 2));

            Open();
            _ftdiPort.SetBaudRate((uint)baud_rate);
            return 0;
        }

        public override string[] GetPortNames()
        {
            List<String> portnames = new List<String>();

            FTDI.FT_DEVICE_INFO_NODE[] list = new FTDI.FT_DEVICE_INFO_NODE[32];
            status = _ftdiPort.GetDeviceList(list);
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                return null;
            }

            foreach (FTDI.FT_DEVICE_INFO_NODE node in list)
            {
                if (node != null) { portnames.Add(node.ftHandle + " " + node.Description); }
            }
            return portnames.ToArray();
        }

        public override void Open()
        {
            _ftdiPort.OpenByIndex((uint)portindex);
        }

        public override int Close()
        {
            if (_ftdiPort.Close() == FTDI.FT_STATUS.FT_OK)
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }

        public override void ChangeTimeouts(int read_timeout, int write_timeout)
        {
            _ftdiPort.SetTimeouts((uint)read_timeout, (uint)write_timeout);
        }

        public override void ChangeBaudRate(int new_baud_rate)
        {
            _ftdiPort.SetBaudRate((uint)new_baud_rate);
        }



        public override void DiscardBuffers()
        {
            _ftdiPort.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
        }

        public override bool write(byte[] buffer, int offset, int count)
        {
            try
            {
                // Offset?!
                _ftdiPort.Write(buffer, count, ref numBytesWritten);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public override string read()
        {
            _ftdiPort.GetRxBytesAvailable(ref RxQueue);
            string buffer;
            _ftdiPort.Read(out buffer, RxQueue, ref numBytesRead);
            return buffer;
        }

        public override byte[] read(int offset, int count)
        {
            byte[] buffer = new byte[count];

            try
            {
                _ftdiPort.Read(buffer, (uint)count, ref numBytesRead);
                return buffer;
            }
            catch
            {
                return null;
            }

        }

        //send a string over the ftdi port
        public override void WriteLine(string message)
        {
            _ftdiPort.Write(message, message.Length, ref numBytesWritten);
        }

        //try reading a string from the com port
        public override string ReadLine()
        {
            string message;
            message = "[READLINE NOT VALID FOR FTDI]";

            return message;
        }

        private void DataReceived(object sender,EventArgs e)
        {
            if (DataArrived != null)
                DataArrived(this, EventArgs.Empty);
        }


    }
}
