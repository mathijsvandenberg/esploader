namespace ESPLoader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnFlash = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOpenFiles = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.f2 = new System.Windows.Forms.TextBox();
            this.f1 = new System.Windows.Forms.TextBox();
            this.lblFile2 = new System.Windows.Forms.Label();
            this.lblFile1 = new System.Windows.Forms.Label();
            this.pb = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // btnFlash
            // 
            this.btnFlash.Location = new System.Drawing.Point(102, 60);
            this.btnFlash.Name = "btnFlash";
            this.btnFlash.Size = new System.Drawing.Size(87, 42);
            this.btnFlash.TabIndex = 11;
            this.btnFlash.Text = "Flash";
            this.btnFlash.UseVisualStyleBackColor = true;
            this.btnFlash.Click += new System.EventHandler(this.btnFlash_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(102, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(87, 42);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOpenFiles
            // 
            this.btnOpenFiles.Location = new System.Drawing.Point(12, 108);
            this.btnOpenFiles.Name = "btnOpenFiles";
            this.btnOpenFiles.Size = new System.Drawing.Size(87, 42);
            this.btnOpenFiles.TabIndex = 9;
            this.btnOpenFiles.Text = "Open Files";
            this.btnOpenFiles.UseVisualStyleBackColor = true;
            this.btnOpenFiles.Click += new System.EventHandler(this.btnOpenFiles_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(12, 60);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(87, 42);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(87, 42);
            this.btnOpen.TabIndex = 7;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // log
            // 
            this.log.BackColor = System.Drawing.Color.Black;
            this.log.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.log.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.log.Location = new System.Drawing.Point(195, 12);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log.Size = new System.Drawing.Size(443, 287);
            this.log.TabIndex = 12;
            // 
            // f2
            // 
            this.f2.Location = new System.Drawing.Point(69, 243);
            this.f2.Name = "f2";
            this.f2.Size = new System.Drawing.Size(120, 20);
            this.f2.TabIndex = 17;
            // 
            // f1
            // 
            this.f1.Location = new System.Drawing.Point(69, 219);
            this.f1.Name = "f1";
            this.f1.Size = new System.Drawing.Size(120, 20);
            this.f1.TabIndex = 16;
            // 
            // lblFile2
            // 
            this.lblFile2.AutoSize = true;
            this.lblFile2.Location = new System.Drawing.Point(12, 246);
            this.lblFile2.Name = "lblFile2";
            this.lblFile2.Size = new System.Drawing.Size(51, 13);
            this.lblFile2.TabIndex = 15;
            this.lblFile2.Text = "0x40000:";
            // 
            // lblFile1
            // 
            this.lblFile1.AutoSize = true;
            this.lblFile1.Location = new System.Drawing.Point(12, 222);
            this.lblFile1.Name = "lblFile1";
            this.lblFile1.Size = new System.Drawing.Size(51, 13);
            this.lblFile1.TabIndex = 14;
            this.lblFile1.Text = "0x00000:";
            // 
            // pb
            // 
            this.pb.Location = new System.Drawing.Point(12, 272);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(177, 27);
            this.pb.Step = 1;
            this.pb.TabIndex = 13;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 306);
            this.Controls.Add(this.f2);
            this.Controls.Add(this.f1);
            this.Controls.Add(this.lblFile2);
            this.Controls.Add(this.lblFile1);
            this.Controls.Add(this.pb);
            this.Controls.Add(this.log);
            this.Controls.Add(this.btnFlash);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpenFiles);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnOpen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "ESPLoader 0.1 by mathijs";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFlash;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOpenFiles;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.TextBox f2;
        private System.Windows.Forms.TextBox f1;
        private System.Windows.Forms.Label lblFile2;
        private System.Windows.Forms.Label lblFile1;
        private System.Windows.Forms.ProgressBar pb;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}

