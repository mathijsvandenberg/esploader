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
            _ftdiPort.OpenByIndex(0);
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

        private void DataReceived(object sender,EventArgs e)
        {
            if (DataArrived != null)
                DataArrived(this, EventArgs.Empty);
        }

    }
}
