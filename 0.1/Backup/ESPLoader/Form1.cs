using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using FTD2XX_NET;
using System.IO;

namespace ESPLoader
{
    public partial class Form1 : Form
    {
        // Create FTDI handle
        FTDI esp = new FTDI();
        FTDI.FT_STATUS status = new FTDI.FT_STATUS();

        // Name of your FTDI device. Mine is called TTL232R-3V3. Might be
        // be 'USB-Serial Converter' on other cables.
        const string FTDI_Description = "TTL232R-3V3";

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

        // variables for the FTDI device
        uint RxQueue = 0;
        uint numBytesRead = 0;
        uint numBytesWritten = 0;

        // Global variables
        int CorrectReply = 0;


        byte[] data = new byte[ESP_FLASH_BLOCK * 2];
        byte[] cmddata = new byte[ESP_FLASH_BLOCK * 2];
        byte[] txdata = new byte[ESP_FLASH_BLOCK * 2];
        byte[] rxdata = new byte[ESP_FLASH_BLOCK * 2];

        byte[] file1 = new byte[0x40000];  // Init file1 with 256K
        byte[] file2 = new byte[0x40000];  // Init file2 with 256K

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            status = esp.OpenByDescription(FTDI_Description);
            esp.SetBaudRate(ESP_BOOTLOADER_BAUD);
            Console.WriteLine(status);
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                log.AppendText("Port open success!\r\n");
                esp.SetTimeouts(250, 250);
                timer1.Enabled = true;
            }
            else
            {
                log.AppendText("Error opening port: " + status + "\r\n");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            status = esp.Close();
            Console.WriteLine(status);
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                log.AppendText("Closing port!\r\n");

            }
            else
            {
                log.AppendText("Error closing port: " + status + "\r\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            log.AppendText("Reset ESP8266 (RTS)\r\n");
            esp.SetRTS(false);
            Thread.Sleep(250);
            esp.SetRTS(true);
        }

        private void btnFlash_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Thread.Sleep(500);
            log.AppendText("Connecting to target..\r\n");
            esp.SetBaudRate(ESP_ROM_BAUD);
            sync();

            if (CorrectReply == 0)
            {
                log.AppendText("Could not sync target..\r\n");
                return;
            }

            log.AppendText("Synced..\r\n");
            Thread.Sleep(500);
            flash_chunks(0x00000, file1);
            log.AppendText("Flash part 1 done\r\n");
            flash_chunks(0x40000, file2);
            log.AppendText("Flash part 2 done\r\n");


            flash_finish(0);
            log.AppendText("Finished\r\n");
            esp.SetBaudRate(ESP_USER_BAUD);
            timer1.Enabled = true;
        }

        private void btnOpenFiles_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string file;

            openFileDialog.Title = "Select 0x00000.bin file";
            openFileDialog.FileName = "0x00000.bin";
            result = openFileDialog.ShowDialog();
            file = openFileDialog.FileName;
            try
            {
                file1 = File.ReadAllBytes(file);
                f1.Text = file;
                log.AppendText("Opening file " + file + "\r\n");
            }
            catch (IOException)
            {
                log.AppendText("Cannot open file " + file + "\r\n");
                f1.Text = "";
            }

            openFileDialog.Title = "Select 0x40000.bin file";
            openFileDialog.FileName = "0x40000.bin";
            result = openFileDialog.ShowDialog();
            file = openFileDialog.FileName;
            try
            {
                file2 = File.ReadAllBytes(file);
                log.AppendText("Opening file " + file + "\r\n");
                f2.Text = file;
            }
            catch (IOException)
            {
                log.AppendText("Cannot open file " + file + "\r\n");
                f2.Text = "";
            }


            log.AppendText("Done Opening files\r\n");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            esp.GetRxBytesAvailable(ref RxQueue);
            if (RxQueue > 0)
            {
                Array.Clear(data, 0, 1024);
                esp.Read(data, RxQueue, ref numBytesRead);
                var str = System.Text.Encoding.Default.GetString(data);
                Console.WriteLine(numBytesRead);
                log.AppendText(str);

            }
        }

        private void write(byte[] data, int len)
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
            esp.Write(txdata, i, ref numBytesWritten);

        }

        private int read(byte[] data, int len)
        {
            int i = 0;
            byte c;

            while (i < len)
            {
                esp.Read(rxdata, 1, ref numBytesRead);
                c = rxdata[0];
                if (c == 0xDB)
                {
                    esp.Read(rxdata, 1, ref numBytesRead);
                    c = rxdata[0];
                    if (c == 0xDC) { data[i++] = 0xC0; }
                    else if (c == 0xDD) { data[i++] = 0xDB; }
                    else { log.AppendText("Invalid SLIP escape!\r\n"); }
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

        private void command(byte cmd, byte[] cdata, int len, int chk)
        {
            CorrectReply = 0;
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
            write(data, i);

            if (read(data, 1) == 0)
            {
                log.AppendText("No response!\r\n");
                return;
            }
            if (data[0] != 0xC0) { log.AppendText("Invalid head of packet\r\n"); return; }
            read(data, 8);
            if (data[0] != 0x01 || (cmd != data[1])) { log.AppendText("Invalid response\r\n"); return; }
            len_ret = data[2] + data[3] * 256;
            read(data, len_ret);
            if (len_ret != 2 || data[0] != 0x00 || data[1] != 0x00) // Something bad happened
            {
                log.AppendText("Failure: got " + len_ret + "Bytes of data: ");
                StringBuilder hex = new StringBuilder((int)len * 3);
                for (x = 0; x < len_ret; x++)
                {
                    hex.AppendFormat(" {0:x2}", data[x]);
                }
                log.AppendText(hex.ToString() + "\r\n");
            }


            read(data, 1);
            if (data[0] == 0xC0)
            {
                CorrectReply = 1;
            }
            else
            {
                log.AppendText("Invalid end of packet\r\n");
            }
        }

        private void sync()
        {
            int x;
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
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
                command(ESP_SYNC, cmddata, 36, 0);
            }
        }


        private void flash_begin(int ImageLen, int Address)
        {
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
            int i = 0;
            esp.SetTimeouts(10000, 10000);
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
            command(ESP_FLASH_BEGIN, cmddata, i, 0);
            esp.SetTimeouts(250, 250);
        }

        private void flash_block(int DataLen, int Sequence)
        {
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
            int i = 0;
            int x = 0;
            esp.SetTimeouts(1000, 1000);
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
            command(ESP_FLASH_DATA, cmddata, i, (int)(checksum(DataLen)));
            esp.SetTimeouts(250, 250);



        }

        private void flash_finish(int reboot)
        {
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            esp.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
            int i = 0;
            esp.SetTimeouts(10000, 10000);
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
            command(ESP_FLASH_END, cmddata, i, 0);
            esp.SetTimeouts(250, 250);
        }

        private void flash_chunks(int offset, byte[] flashdata)
        {
            int seq = 0;
            int blocks = 0;
            int len = 0;
            flash_begin(flashdata.Length, offset);
            log.AppendText("Flash erased\r\n");
            blocks = (int)(Math.Ceiling((double)flashdata.Length / (double)(ESP_FLASH_BLOCK)));
            log.AppendText("File is split up in " + blocks + " chunks of " + ESP_FLASH_BLOCK + " Length\r\n");
            for (seq = 0; seq < blocks; seq++)
            {
                Application.DoEvents();
                log.AppendText("Sending chunk [" + (seq + 1) + "/" + blocks + "]\r\n");
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
                pb.Value = ((seq + 1) * 100) / blocks;
            }
        }

    }
}
