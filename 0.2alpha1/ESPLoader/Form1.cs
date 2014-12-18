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
        ESP8266ProgrammingTool esp = new ESP8266ProgrammingTool();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            esp.StatusChange += StatusChange;
            ((Control)this.tabTCPServer).Enabled = false;

        }



        void StatusChange(object sender, EventArgs e)
        {
            log.AppendText(esp.CurrentStatus + "\r\n");
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
                esp.SetInterface(ref port);

                log.AppendText("Port open success!\r\n");
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
        }

        private void btnFlash_Click(object sender, EventArgs e)
        {
            esp.Flash();
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
                esp.AddBinaryFile(file, 0x00000);
                f1.Text = file;
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
                esp.AddBinaryFile(file, 0x40000);
                f2.Text = file;
            }
            catch (IOException)
            {
                log.AppendText("Cannot open file " + file + "\r\n");
                f2.Text = "";
            }

            log.AppendText("Done Opening files\r\n");
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }



 

    }
}
