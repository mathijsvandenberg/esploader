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

        COMPort comport = new COMPort();
        FTDIPort ftdiport = new FTDIPort();
        InterfacePort port = new InterfacePort();
        ESP8266ProgrammingTool esp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // Fix this, Should be in Interface port class


            if (cboPorts.Text.StartsWith("[COM]"))
            {
                port = new COMPort();
            }
            if (cboPorts.Text.StartsWith("[FTDI]"))
            {
                port = new FTDIPort();
            }

            if (port.OpenPort(cboPorts.Text,75000) == 0)
            {
                log.AppendText("Port open success!\r\n");
                esp = new ESP8266ProgrammingTool(port);
                timer1.Enabled = true;
            }
            else
            {
                log.AppendText("Error opening port '" + cboPorts.Text + "'\r\n");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (port.Close() == 0)
            {
                log.AppendText("Closing port!\r\n");

            }
            else
            {
                log.AppendText("Error closing port!\r\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            esp.Reset();

            log.AppendText("Reset ESP8266\r\n");
            //esp.SetRTS(false);
            //Thread.Sleep(250);
            //esp.SetRTS(true);
        }

        private void btnFlash_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Thread.Sleep(500);
            log.AppendText("Connecting to target..\r\n");
            //port.SetBaudRate(ESP_ROM_BAUD);

            if (esp.sync() == -1)
            {
                log.AppendText("Could not sync target..\r\n");
                return;
            }

            log.AppendText("Synced..\r\n");
            Thread.Sleep(500);
            //esp.Flash(0x00000,file1);
            log.AppendText("Flash part 1 done\r\n");
            //esp.Flash(0x40000,file2);
            log.AppendText("Flash part 2 done\r\n");

            //esp.flash_finish(0);
            log.AppendText("Finished\r\n");
            //esp.SetBaudRate(ESP_USER_BAUD);
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
                //file1 = File.ReadAllBytes(file);
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
                //file2 = File.ReadAllBytes(file);
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
            //esp.GetRxBytesAvailable(ref RxQueue);
            //if (RxQueue > 0)
            //{
                //Array.Clear(data, 0, 1024);
                //esp.Read(data, RxQueue, ref numBytesRead);
                //var str = System.Text.Encoding.Default.GetString(data);
                //Console.WriteLine(numBytesRead);
                //log.AppendText(str);

           // }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void aboutESPLoaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ESPLoader 0.2 by:\r\n\r\nMathijs van den Berg <mathijsvandenberg3@gmail.com>\r\nJonathan Georgino <ESPTools@jonathangeorgino.com>\r\n\r\nhttps://github.com/mathijsvandenberg/esploader/");

        }

        private void cboPorts_DropDown(object sender, EventArgs e)
        {
            cboPorts.Items.Clear();
            //list available COM ports (COMPort.cs)
            foreach (string s in comport.GetPortNames())
            {
                cboPorts.Items.Add("[COM] " + s);
            }

            //list available FTDI ports (FTDIPort.cs)
            foreach (string s in ftdiport.GetPortNames())
            {
                cboPorts.Items.Add("[FTDI] " + s);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



 

    }
}
