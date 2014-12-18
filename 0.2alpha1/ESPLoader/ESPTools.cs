using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ESPLoader
{
    //Author: Mathijs van den Berg (initial C# with implementation of the esptool.py file with FTDI support)
    //mathijsvandenberg3@gmail.com
    //(https://github.com/mathijsvandenberg/esploader) of esptool.py (https://github.com/themadinventor/esptool)

    //Author: Jonathan Georgino (made a class of the implementation and added Serial Port support)
    //www.jonathangeorgino.com
    //ESPTools@jonathangeorgino.com


    class ESP8266ProgrammingTool
    {
        // These defines values are taken from esptool.py
        const byte ESP_FLASH_BEGIN = 0x02;
        const byte ESP_FLASH_DATA = 0x03;
        const byte ESP_FLASH_END = 0x04;
        const byte ESP_MEM_BEGIN = 0x05;
        const byte ESP_MEM_END = 0x06;
        const byte ESP_MEM_DATA = 0x07;
        const byte ESP_SYNC = 0x08;
        const byte ESP_WRITE_REG = 0x09;
        const byte ESP_READ_REG = 0x0A;
        const int ESP_RAM_BLOCK = 0x1800;
        const int ESP_FLASH_BLOCK = 0x400;
        const byte ESP_IMAGE_MAGIC = 0xE9;
        const byte ESP_CHECKSUM_MAGIC = 0xEF;

        // Baud rate settings
        const int ESP_BOOTLOADER_BAUD = 75000;
        const int ESP_USER_BAUD = 75000;
        const int ESP_ROM_BAUD = 115200;

        // Global variables
        private byte[] data = new byte[ESP_FLASH_BLOCK * 2];
        private byte[] cmddata = new byte[ESP_FLASH_BLOCK * 2];
        private byte[] txdata = new byte[ESP_FLASH_BLOCK * 2];
        private byte[] rxdata = new byte[ESP_FLASH_BLOCK * 2];

        private List<SourceFile> BinFiles;

        private string _commport_str;
        private InterfacePort _port;
        public List<string> Log { get; private set; }

        public string CurrentStatus { get; private set; }
        public double PercentComplete { get; private set; }

        //events
        public event System.EventHandler<EventArgs> StatusChange;

        public ESP8266ProgrammingTool()
        {
            BinFiles = new List<SourceFile>();
            Log = new List<string>();

            
            UpdateStatus("Ready!");
        }

        public bool AddBinaryFile(string full_filename, int write_to_address)
        {
            SourceFile newsource = new SourceFile(full_filename, write_to_address);
            
            if(newsource.isValid)
            {
                BinFiles.Add(newsource);
                UpdateStatus("Added Source File: " + full_filename);
                return true;
            }
            else
            {
                UpdateStatus("Failed to Add Source File: " + full_filename);
                return false;
            }
            
        }

        public void SetInterface(ref InterfacePort port)
        {
            _port = port;
            _commport_str = port.Portname;

        }


        public void Reset()
        {
            BinFiles.Clear();
            PercentComplete = 0;
            Log.Clear();
            UpdateStatus("Ready!");
        }

        public void ClearBinaryFiles()
        {
            BinFiles.Clear();
            UpdateStatus("All Source Files Removed!");
        }

        public bool Flash()
        {
            PercentComplete = 0;
            Log.Clear();

            UpdateStatus("Beginning Flash Programming!");

            UpdateStatus("Refreshing Source Files...");

            for (int i = 0; i < BinFiles.Count; i++ )
            {
                BinFiles[i].Refresh();
                if(!BinFiles[i].isValid)
                {
                    UpdateStatus("Problem reading Source File + " + BinFiles[i].Filepath);
                    return false;
                }
            }

                Thread.Sleep(500);
                UpdateStatus("Connecting to target ESP8266...");

                bool isSyncSuccessful = false;
                for (int i = 0; i < 3;i++ )
                {
                    if (sync() == 0)
                    {
                        UpdateStatus("Could not sync to ESP8266... Trying again, attempt " + i);
                        _port.DiscardBuffers();
                    }
                    else
                    {
                        //sync'd, so break out of the loop
                        isSyncSuccessful = true;
                        break;
                    }
                }

            if(!isSyncSuccessful)
            {
                UpdateStatus("Could not sync to ESP8266 after multiple tries, giving up!");
                return false;
            }

            UpdateStatus("Synced...");
            Thread.Sleep(500);

            for (int i = 0; i < BinFiles.Count; i++ )
            {
                SourceFile currentfile = BinFiles[i];
                flash_chunks(currentfile.MemoryLocation, currentfile.Contents);
                UpdateStatus("Flashing of  " + currentfile.ShortName + " to 0x" + currentfile.MemoryLocation.ToString("X5"));
            }

            flash_finish(0);
            UpdateStatus("Finished");


            return true;
        }

        #region private behind the scenes stuff

        private void UpdateStatus(string status)
        {
            CurrentStatus = status;

            Log.Add(status);

            if (StatusChange != null)
                StatusChange(this, EventArgs.Empty);
        }

        private bool write(byte[] data, int len)
        {
            int x = 0;
            int i = 0;

            txdata[i++] = 0xC0;
            for (x = 0; x < len; x++)
            {
                if (data[x] == 0xC0) { txdata[i++] = 0xDB; txdata[i++] = 0xDC; }
                else if (data[x] == 0xDB) { txdata[i++] = 0xDB; txdata[i++] = 0xDD; }
                else { txdata[i++] = data[x]; }
            }
            txdata[i++] = 0xC0;

            if (!_port.write(txdata, 0x00, i))
                return false;
            else
                return true;


        }

        private int read(byte[] data, int len)
        {
            int i = 0;
            byte c;

            while (i < len)
            {
                byte[] rxdata;
                rxdata = _port.read(0x00, 1);

                if (rxdata == null)
                    return -1;

                c = rxdata[0];
                if (c == 0xDB)
                {
                    rxdata = _port.read(0x00, 1);

                    if(rxdata == null)
                        return -1;

                    c = rxdata[0];
                    if (c == 0xDC) { data[i++] = 0xC0; }
                    else if (c == 0xDD) { data[i++] = 0xDB; }
                    else { UpdateStatus("Invalid SLIP escape!"); }
                }
                else
                {
                    data[i++] = c;
                }
            }
            return i;
        }

        private byte checksum(int DataLen)
        {
            byte chk = ESP_CHECKSUM_MAGIC;
            int x = 0;
            for (x = 0; x < DataLen; x++)
            {
                chk ^= data[x];
            }
            return chk;
        }

        private int command(byte cmd, byte[] cdata, int len, int chk, bool ignore_err)
        {
            int CorrectReply = 0;
            int i = 0;
            int a = 0;
            int len_ret = 0;
            int x = 0;
            // Some header
            data[i++] = 0x00;
            data[i++] = cmd;
            data[i++] = (byte)((len & 0xFF));
            data[i++] = (byte)((len >> 8) & 0xFF);
            data[i++] = (byte)((chk & 0xFF));
            data[i++] = (byte)((chk >> 8) & 0xFF);
            data[i++] = (byte)((chk >> 16) & 0xFF);
            data[i++] = (byte)((chk >> 24) & 0xFF);
            for (a = 0; a < len; a++)
            {
                data[i++] = cdata[a];
            }
            if(!write(data, i))
            {
                UpdateStatus("Failed to write to target device!");
                return CorrectReply;
            }

            int result = read(data, 1);
            if (result == 0)
            {
                UpdateStatus("No response!");
                return CorrectReply;
            }
            else if(result == -1)
            {
                UpdateStatus("Read Error Occured!");
                return CorrectReply;
            }
            if (data[0] != 0xC0) { UpdateStatus("Invalid head of packet"); return CorrectReply; }
            
            if (read(data, 8) == -1) { UpdateStatus("Read Error Occured"); return CorrectReply; }
            
            if (data[0] != 0x01 || (cmd != data[1])) { UpdateStatus("Invalid response"); return CorrectReply; }
            len_ret = data[2] + data[3] * 256;
            
            if (read(data, len_ret) == -1) { UpdateStatus("Read Error Occured"); return CorrectReply; }
            
            if (len_ret != 2 || data[0] != 0x00 || data[1] != 0x00) // Something bad happened
            {
                //this is a sloppy patch to ignore the false raising of errors on flash_finish()
                if (!ignore_err)
                {
                    
                    StringBuilder hex = new StringBuilder((int)len * 3);
                    for (x = 0; x < len_ret; x++)
                    {
                        hex.AppendFormat(" {0:x2}", data[x]);
                    }
                    UpdateStatus("Failure: got " + len_ret + "Bytes of data: " + hex.ToString());
                }
            }


            if (read(data, 1) == -1){ UpdateStatus("Read Error Occured"); return CorrectReply; };
            if (data[0] == 0xC0)
            {
                CorrectReply = 1;
            }
            else
            {
                UpdateStatus("Invalid end of packet");
            }

            return CorrectReply;
        }
        
        public int sync()
        {
            int x;
            int response = 0;

            _port.DiscardBuffers();

            cmddata[0] = 0x07;
            cmddata[1] = 0x07;
            cmddata[2] = 0x12;
            cmddata[3] = 0x20;
            for (x = 4; x < 36; x++)
            {
                cmddata[x] = 0x55;
            }
            for (x = 0; x < 7; x++)
            {
                response = command(ESP_SYNC, cmddata, 36, 0,false);
            }
            return response;
        }

        private void flash_begin(int ImageLen, int Address)
        {
            _port.DiscardBuffers();

            int i = 0;

            _port.ChangeTimeouts(10000, 10000);

            cmddata[i++] = (byte)((ImageLen & 0xFF));
            cmddata[i++] = (byte)((ImageLen >> 8) & 0xFF);
            cmddata[i++] = (byte)((ImageLen >> 16) & 0xFF);
            cmddata[i++] = (byte)((ImageLen >> 24) & 0xFF);
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x02;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x04;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = (byte)((Address & 0xFF));
            cmddata[i++] = (byte)((Address >> 8) & 0xFF);
            cmddata[i++] = (byte)((Address >> 16) & 0xFF);
            cmddata[i++] = (byte)((Address >> 24) & 0xFF);
            command(ESP_FLASH_BEGIN, cmddata, i, 0, false);

            _port.ChangeTimeouts(500, 500);
        }

        private void flash_block(int DataLen, int Sequence)
        {
            _port.DiscardBuffers();

            int i = 0;
            int x = 0;

            _port.ChangeTimeouts(10000, 10000);

            cmddata[i++] = (byte)((DataLen & 0xFF));
            cmddata[i++] = (byte)((DataLen >> 8) & 0xFF);
            cmddata[i++] = (byte)((DataLen >> 16) & 0xFF);
            cmddata[i++] = (byte)((DataLen >> 24) & 0xFF);
            cmddata[i++] = (byte)((Sequence & 0xFF));
            cmddata[i++] = (byte)((Sequence >> 8) & 0xFF);
            cmddata[i++] = (byte)((Sequence >> 16) & 0xFF);
            cmddata[i++] = (byte)((Sequence >> 24) & 0xFF);
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            cmddata[i++] = 0x00;
            for (x = 0; x < DataLen; x++)
            {
                cmddata[i++] = data[x];
            }
            command(ESP_FLASH_DATA, cmddata, i, (int)(checksum(DataLen)), false);

            _port.ChangeTimeouts(500, 500);

        }

        private void flash_finish(int reboot)
        {
            _port.DiscardBuffers();

            int i = 0;

            _port.ChangeTimeouts(10000, 10000);


            if (reboot == 0)
            {
                cmddata[i++] = 0x01;
            }
            else
            {
                cmddata[i++] = 0x00;
            }
            cmddata[i++] = 0;
            cmddata[i++] = 0;
            cmddata[i++] = 0;
            command(ESP_FLASH_END, cmddata, i, 0, true); //the true will make it stop reporting false (and real) errors at the end

            _port.ChangeTimeouts(500, 500);

        }

        private void flash_chunks(int offset, byte[] flashdata)
        {
            int seq = 0;
            int blocks = 0;
            int len = 0;
            flash_begin(flashdata.Length, offset);
            UpdateStatus("Flash erased");
            blocks = (int)(Math.Ceiling((double)flashdata.Length / (double)(ESP_FLASH_BLOCK)));
            UpdateStatus("File is split up in " + blocks + " chunks of " + ESP_FLASH_BLOCK + " Length");
            for (seq = 0; seq < blocks; seq++)
            {
                UpdateStatus("Sending chunk [" + (seq + 1) + "/" + blocks + "]");
                len = flashdata.Length - (seq * ESP_FLASH_BLOCK);

                if (len > ESP_FLASH_BLOCK)
                {
                    Array.Copy(flashdata, seq * ESP_FLASH_BLOCK, data, 0, ESP_FLASH_BLOCK);
                    flash_block(ESP_FLASH_BLOCK, seq);
                }
                else
                {
                    Array.Copy(flashdata, seq * ESP_FLASH_BLOCK, data, 0, len);
                    flash_block(len, seq);
                }
                PercentComplete = ((seq + 1) * 100) / blocks;
            }
        }

        #endregion
    }

}
