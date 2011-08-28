﻿namespace DALightmapper
{
    partial class SettingsWindow
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
            this.lb_filePaths = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_erfFiles = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_tempFiles = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_tempDirectory = new System.Windows.Forms.TextBox();
            this.btn_changeWorkingDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_workingDirectory = new System.Windows.Forms.TextBox();
            this.btn_removeERFFile = new System.Windows.Forms.Button();
            this.btn_addERFFile = new System.Windows.Forms.Button();
            this.btn_removeFilePath = new System.Windows.Forms.Button();
            this.btn_addFilePath = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rb_energyInSystem = new System.Windows.Forms.RadioButton();
            this.rb_numBounces = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.nmup_minEnergy = new System.Windows.Forms.NumericUpDown();
            this.nmup_numBounces = new System.Windows.Forms.NumericUpDown();
            this.cb_useTrueAttenuation = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbl_verbosity = new System.Windows.Forms.Label();
            this.cb_clearTempDir = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.trb_Verbosity = new System.Windows.Forms.TrackBar();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_minEnergy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_numBounces)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trb_Verbosity)).BeginInit();
            this.SuspendLayout();
            // 
            // lb_filePaths
            // 
            this.lb_filePaths.FormattingEnabled = true;
            this.lb_filePaths.HorizontalScrollbar = true;
            this.lb_filePaths.Location = new System.Drawing.Point(6, 34);
            this.lb_filePaths.Name = "lb_filePaths";
            this.lb_filePaths.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lb_filePaths.Size = new System.Drawing.Size(379, 56);
            this.lb_filePaths.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "File paths to search in:";
            // 
            // lb_erfFiles
            // 
            this.lb_erfFiles.FormattingEnabled = true;
            this.lb_erfFiles.HorizontalScrollbar = true;
            this.lb_erfFiles.Location = new System.Drawing.Point(6, 125);
            this.lb_erfFiles.Name = "lb_erfFiles";
            this.lb_erfFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lb_erfFiles.Size = new System.Drawing.Size(379, 56);
            this.lb_erfFiles.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "ERF files to use:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_tempFiles);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tb_tempDirectory);
            this.groupBox1.Controls.Add(this.btn_changeWorkingDir);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tb_workingDirectory);
            this.groupBox1.Controls.Add(this.btn_removeERFFile);
            this.groupBox1.Controls.Add(this.btn_addERFFile);
            this.groupBox1.Controls.Add(this.btn_removeFilePath);
            this.groupBox1.Controls.Add(this.btn_addFilePath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lb_filePaths);
            this.groupBox1.Controls.Add(this.lb_erfFiles);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(391, 322);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Directory Settings";
            // 
            // btn_tempFiles
            // 
            this.btn_tempFiles.Location = new System.Drawing.Point(310, 293);
            this.btn_tempFiles.Name = "btn_tempFiles";
            this.btn_tempFiles.Size = new System.Drawing.Size(75, 23);
            this.btn_tempFiles.TabIndex = 13;
            this.btn_tempFiles.Text = "Change";
            this.btn_tempFiles.UseVisualStyleBackColor = true;
            this.btn_tempFiles.Click += new System.EventHandler(this.btn_tempFiles_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 250);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Temporary Files Directory:";
            // 
            // tb_tempDirectory
            // 
            this.tb_tempDirectory.Location = new System.Drawing.Point(6, 269);
            this.tb_tempDirectory.Name = "tb_tempDirectory";
            this.tb_tempDirectory.Size = new System.Drawing.Size(379, 20);
            this.tb_tempDirectory.TabIndex = 11;
            // 
            // btn_changeWorkingDir
            // 
            this.btn_changeWorkingDir.Location = new System.Drawing.Point(310, 240);
            this.btn_changeWorkingDir.Name = "btn_changeWorkingDir";
            this.btn_changeWorkingDir.Size = new System.Drawing.Size(75, 23);
            this.btn_changeWorkingDir.TabIndex = 10;
            this.btn_changeWorkingDir.Text = "Change";
            this.btn_changeWorkingDir.UseVisualStyleBackColor = true;
            this.btn_changeWorkingDir.Click += new System.EventHandler(this.btn_changeWorkingDir_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 196);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Working Directory:";
            // 
            // tb_workingDirectory
            // 
            this.tb_workingDirectory.Location = new System.Drawing.Point(6, 215);
            this.tb_workingDirectory.Name = "tb_workingDirectory";
            this.tb_workingDirectory.Size = new System.Drawing.Size(379, 20);
            this.tb_workingDirectory.TabIndex = 8;
            // 
            // btn_removeERFFile
            // 
            this.btn_removeERFFile.Location = new System.Drawing.Point(310, 186);
            this.btn_removeERFFile.Name = "btn_removeERFFile";
            this.btn_removeERFFile.Size = new System.Drawing.Size(75, 23);
            this.btn_removeERFFile.TabIndex = 7;
            this.btn_removeERFFile.Text = "Remove";
            this.btn_removeERFFile.UseVisualStyleBackColor = true;
            this.btn_removeERFFile.Click += new System.EventHandler(this.btn_removeERFFile_Click);
            // 
            // btn_addERFFile
            // 
            this.btn_addERFFile.Location = new System.Drawing.Point(229, 186);
            this.btn_addERFFile.Name = "btn_addERFFile";
            this.btn_addERFFile.Size = new System.Drawing.Size(75, 23);
            this.btn_addERFFile.TabIndex = 6;
            this.btn_addERFFile.Text = "Add";
            this.btn_addERFFile.UseVisualStyleBackColor = true;
            this.btn_addERFFile.Click += new System.EventHandler(this.btn_addERFFile_Click);
            // 
            // btn_removeFilePath
            // 
            this.btn_removeFilePath.Location = new System.Drawing.Point(310, 96);
            this.btn_removeFilePath.Name = "btn_removeFilePath";
            this.btn_removeFilePath.Size = new System.Drawing.Size(75, 23);
            this.btn_removeFilePath.TabIndex = 5;
            this.btn_removeFilePath.Text = "Remove";
            this.btn_removeFilePath.UseVisualStyleBackColor = true;
            this.btn_removeFilePath.Click += new System.EventHandler(this.btn_removeFilePath_Click);
            // 
            // btn_addFilePath
            // 
            this.btn_addFilePath.Location = new System.Drawing.Point(229, 96);
            this.btn_addFilePath.Name = "btn_addFilePath";
            this.btn_addFilePath.Size = new System.Drawing.Size(75, 23);
            this.btn_addFilePath.TabIndex = 4;
            this.btn_addFilePath.Text = "Add";
            this.btn_addFilePath.UseVisualStyleBackColor = true;
            this.btn_addFilePath.Click += new System.EventHandler(this.btn_addFilePath_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rb_energyInSystem);
            this.groupBox2.Controls.Add(this.rb_numBounces);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.nmup_minEnergy);
            this.groupBox2.Controls.Add(this.nmup_numBounces);
            this.groupBox2.Controls.Add(this.cb_useTrueAttenuation);
            this.groupBox2.Location = new System.Drawing.Point(409, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(430, 223);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Lightmapper Settings";
            // 
            // rb_energyInSystem
            // 
            this.rb_energyInSystem.AutoSize = true;
            this.rb_energyInSystem.Checked = true;
            this.rb_energyInSystem.Location = new System.Drawing.Point(25, 90);
            this.rb_energyInSystem.Name = "rb_energyInSystem";
            this.rb_energyInSystem.Size = new System.Drawing.Size(148, 17);
            this.rb_energyInSystem.TabIndex = 7;
            this.rb_energyInSystem.TabStop = true;
            this.rb_energyInSystem.Text = "Energy in system is below:";
            this.rb_energyInSystem.UseVisualStyleBackColor = true;
            // 
            // rb_numBounces
            // 
            this.rb_numBounces.AutoSize = true;
            this.rb_numBounces.Location = new System.Drawing.Point(25, 66);
            this.rb_numBounces.Name = "rb_numBounces";
            this.rb_numBounces.Size = new System.Drawing.Size(121, 17);
            this.rb_numBounces.TabIndex = 6;
            this.rb_numBounces.Text = "Number of bounces:";
            this.rb_numBounces.UseVisualStyleBackColor = true;
            this.rb_numBounces.CheckedChanged += new System.EventHandler(this.rb_numBounces_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Stop light mapping after:";
            // 
            // nmup_minEnergy
            // 
            this.nmup_minEnergy.DecimalPlaces = 3;
            this.nmup_minEnergy.Location = new System.Drawing.Point(179, 90);
            this.nmup_minEnergy.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nmup_minEnergy.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nmup_minEnergy.Name = "nmup_minEnergy";
            this.nmup_minEnergy.Size = new System.Drawing.Size(80, 20);
            this.nmup_minEnergy.TabIndex = 4;
            this.nmup_minEnergy.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nmup_minEnergy.ValueChanged += new System.EventHandler(this.nmup_minEnergy_ValueChanged);
            // 
            // nmup_numBounces
            // 
            this.nmup_numBounces.Location = new System.Drawing.Point(179, 66);
            this.nmup_numBounces.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nmup_numBounces.Name = "nmup_numBounces";
            this.nmup_numBounces.ReadOnly = true;
            this.nmup_numBounces.Size = new System.Drawing.Size(52, 20);
            this.nmup_numBounces.TabIndex = 1;
            this.nmup_numBounces.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nmup_numBounces.ValueChanged += new System.EventHandler(this.nmup_numBounces_ValueChanged);
            // 
            // cb_useTrueAttenuation
            // 
            this.cb_useTrueAttenuation.AutoSize = true;
            this.cb_useTrueAttenuation.Location = new System.Drawing.Point(12, 23);
            this.cb_useTrueAttenuation.Name = "cb_useTrueAttenuation";
            this.cb_useTrueAttenuation.Size = new System.Drawing.Size(122, 17);
            this.cb_useTrueAttenuation.TabIndex = 0;
            this.cb_useTrueAttenuation.Text = "Use true attenuation";
            this.cb_useTrueAttenuation.UseVisualStyleBackColor = true;
            this.cb_useTrueAttenuation.CheckedChanged += new System.EventHandler(this.cb_useTrueAttenuation_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lbl_verbosity);
            this.groupBox3.Controls.Add(this.cb_clearTempDir);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.trb_Verbosity);
            this.groupBox3.Location = new System.Drawing.Point(409, 242);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(391, 91);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Program Settings";
            // 
            // lbl_verbosity
            // 
            this.lbl_verbosity.AutoSize = true;
            this.lbl_verbosity.Location = new System.Drawing.Point(36, 70);
            this.lbl_verbosity.Name = "lbl_verbosity";
            this.lbl_verbosity.Size = new System.Drawing.Size(0, 13);
            this.lbl_verbosity.TabIndex = 3;
            // 
            // cb_clearTempDir
            // 
            this.cb_clearTempDir.AutoSize = true;
            this.cb_clearTempDir.Checked = true;
            this.cb_clearTempDir.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_clearTempDir.Location = new System.Drawing.Point(149, 20);
            this.cb_clearTempDir.Name = "cb_clearTempDir";
            this.cb_clearTempDir.Size = new System.Drawing.Size(222, 17);
            this.cb_clearTempDir.TabIndex = 2;
            this.cb_clearTempDir.Text = "Clear temp directory when program closes";
            this.cb_clearTempDir.UseVisualStyleBackColor = true;
            this.cb_clearTempDir.CheckedChanged += new System.EventHandler(this.cb_clearTempDir_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Message Verbosity:";
            // 
            // trb_Verbosity
            // 
            this.trb_Verbosity.LargeChange = 2;
            this.trb_Verbosity.Location = new System.Drawing.Point(9, 36);
            this.trb_Verbosity.Maximum = 4;
            this.trb_Verbosity.Name = "trb_Verbosity";
            this.trb_Verbosity.Size = new System.Drawing.Size(99, 45);
            this.trb_Verbosity.TabIndex = 0;
            this.trb_Verbosity.Scroll += new System.EventHandler(this.trb_Verbosity_Scroll);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Encapsulated Resource Files|*.erf";
            this.openFileDialog1.Multiselect = true;
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 342);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SettingsWindow";
            this.Text = "SettingsWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsWindow_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_minEnergy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_numBounces)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trb_Verbosity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lb_filePaths;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lb_erfFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_removeERFFile;
        private System.Windows.Forms.Button btn_addERFFile;
        private System.Windows.Forms.Button btn_removeFilePath;
        private System.Windows.Forms.Button btn_addFilePath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_workingDirectory;
        private System.Windows.Forms.Button btn_changeWorkingDir;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TrackBar trb_Verbosity;
        private System.Windows.Forms.CheckBox cb_clearTempDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nmup_numBounces;
        private System.Windows.Forms.CheckBox cb_useTrueAttenuation;
        private System.Windows.Forms.RadioButton rb_energyInSystem;
        private System.Windows.Forms.RadioButton rb_numBounces;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nmup_minEnergy;
        private System.Windows.Forms.Label lbl_verbosity;
        private System.Windows.Forms.Button btn_tempFiles;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_tempDirectory;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}