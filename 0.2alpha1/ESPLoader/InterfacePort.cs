using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESPLoader
{
    class InterfacePort
    {

        public String Portname;
       //constructor
        public InterfacePort()
        {
        }

        //deconstructor
        ~InterfacePort()
        {
        }

        public virtual int OpenPort(string port_name, int baud_rate)
        {
            return -1;
        }

        public virtual void Open()
        {

        }

        public virtual int Close()
        {
            return -1;
        }

        public virtual void ChangeBaudRate(int new_baud_rate)
        {
            
        }

        public virtual void WriteLine(string message)
        {

        }

        public virtual string ReadLine()
        {
            return "";
        }

        public virtual string read()
        {
            return "";
        }




        public virtual void Reset()
        {
        }

        public virtual string[] GetPortNames()
        {
            return null;
        }

        public virtual void DiscardBuffers()
        {
        }

        public virtual bool write(byte[] buffer, int offset, int count)
        {
            return false;
        }

        public virtual byte[] read(int offset, int count)
        {
            return null;
        }

        public virtual void ChangeTimeouts(int read_timeout, int write_timeout)
        {
        }



    }

}

