namespace DALightmapper
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
            this.nmup_numPhotons = new System.Windows.Forms.NumericUpDown();
            this.cb_useTrueAttenuation = new System.Windows.Forms.CheckBox();
            this.lbl_verbosity = new System.Windows.Forms.Label();
            this.cb_clearTempDir = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.trb_Verbosity = new System.Windows.Forms.TrackBar();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.nmup_Cores = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_Save = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.nmup_gatherRadius = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_numPhotons)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trb_Verbosity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_Cores)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_gatherRadius)).BeginInit();
            this.SuspendLayout();
            // 
            // lb_filePaths
            // 
            this.lb_filePaths.FormattingEnabled = true;
            this.lb_filePaths.HorizontalScrollbar = true;
            this.lb_filePaths.Location = new System.Drawing.Point(12, 23);
            this.lb_filePaths.Name = "lb_filePaths";
            this.lb_filePaths.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lb_filePaths.Size = new System.Drawing.Size(314, 108);
            this.lb_filePaths.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "File paths to search in:";
            // 
            // lb_erfFiles
            // 
            this.lb_erfFiles.FormattingEnabled = true;
            this.lb_erfFiles.HorizontalScrollbar = true;
            this.lb_erfFiles.Location = new System.Drawing.Point(12, 153);
            this.lb_erfFiles.Name = "lb_erfFiles";
            this.lb_erfFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lb_erfFiles.Size = new System.Drawing.Size(314, 108);
            this.lb_erfFiles.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "ERF files to use:";
            // 
            // btn_tempFiles
            // 
            this.btn_tempFiles.Location = new System.Drawing.Point(726, 104);
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
            this.label5.Location = new System.Drawing.Point(422, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Temporary Files Directory:";
            // 
            // tb_tempDirectory
            // 
            this.tb_tempDirectory.Location = new System.Drawing.Point(422, 80);
            this.tb_tempDirectory.Name = "tb_tempDirectory";
            this.tb_tempDirectory.Size = new System.Drawing.Size(379, 20);
            this.tb_tempDirectory.TabIndex = 11;
            // 
            // btn_changeWorkingDir
            // 
            this.btn_changeWorkingDir.Location = new System.Drawing.Point(726, 51);
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
            this.label3.Location = new System.Drawing.Point(422, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Working Directory:";
            // 
            // tb_workingDirectory
            // 
            this.tb_workingDirectory.Location = new System.Drawing.Point(422, 26);
            this.tb_workingDirectory.Name = "tb_workingDirectory";
            this.tb_workingDirectory.Size = new System.Drawing.Size(379, 20);
            this.tb_workingDirectory.TabIndex = 8;
            // 
            // btn_removeERFFile
            // 
            this.btn_removeERFFile.Location = new System.Drawing.Point(334, 182);
            this.btn_removeERFFile.Name = "btn_removeERFFile";
            this.btn_removeERFFile.Size = new System.Drawing.Size(75, 23);
            this.btn_removeERFFile.TabIndex = 7;
            this.btn_removeERFFile.Text = "Remove";
            this.btn_removeERFFile.UseVisualStyleBackColor = true;
            this.btn_removeERFFile.Click += new System.EventHandler(this.btn_removeERFFile_Click);
            // 
            // btn_addERFFile
            // 
            this.btn_addERFFile.Location = new System.Drawing.Point(334, 153);
            this.btn_addERFFile.Name = "btn_addERFFile";
            this.btn_addERFFile.Size = new System.Drawing.Size(75, 23);
            this.btn_addERFFile.TabIndex = 6;
            this.btn_addERFFile.Text = "Add";
            this.btn_addERFFile.UseVisualStyleBackColor = true;
            this.btn_addERFFile.Click += new System.EventHandler(this.btn_addERFFile_Click);
            // 
            // btn_removeFilePath
            // 
            this.btn_removeFilePath.Location = new System.Drawing.Point(334, 52);
            this.btn_removeFilePath.Name = "btn_removeFilePath";
            this.btn_removeFilePath.Size = new System.Drawing.Size(75, 23);
            this.btn_removeFilePath.TabIndex = 5;
            this.btn_removeFilePath.Text = "Remove";
            this.btn_removeFilePath.UseVisualStyleBackColor = true;
            this.btn_removeFilePath.Click += new System.EventHandler(this.btn_removeFilePath_Click);
            // 
            // btn_addFilePath
            // 
            this.btn_addFilePath.Location = new System.Drawing.Point(334, 23);
            this.btn_addFilePath.Name = "btn_addFilePath";
            this.btn_addFilePath.Size = new System.Drawing.Size(75, 23);
            this.btn_addFilePath.TabIndex = 4;
            this.btn_addFilePath.Text = "Add";
            this.btn_addFilePath.UseVisualStyleBackColor = true;
            this.btn_addFilePath.Click += new System.EventHandler(this.btn_addFilePath_Click);
            // 
            // nmup_numPhotons
            // 
            this.nmup_numPhotons.Location = new System.Drawing.Point(703, 181);
            this.nmup_numPhotons.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.nmup_numPhotons.Name = "nmup_numPhotons";
            this.nmup_numPhotons.Size = new System.Drawing.Size(79, 20);
            this.nmup_numPhotons.TabIndex = 1;
            this.nmup_numPhotons.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nmup_numPhotons.ValueChanged += new System.EventHandler(this.nmup_numBounces_ValueChanged);
            // 
            // cb_useTrueAttenuation
            // 
            this.cb_useTrueAttenuation.AutoSize = true;
            this.cb_useTrueAttenuation.Location = new System.Drawing.Point(593, 159);
            this.cb_useTrueAttenuation.Name = "cb_useTrueAttenuation";
            this.cb_useTrueAttenuation.Size = new System.Drawing.Size(122, 17);
            this.cb_useTrueAttenuation.TabIndex = 0;
            this.cb_useTrueAttenuation.Text = "Use true attenuation";
            this.cb_useTrueAttenuation.UseVisualStyleBackColor = true;
            this.cb_useTrueAttenuation.CheckedChanged += new System.EventHandler(this.cb_useTrueAttenuation_CheckedChanged);
            // 
            // lbl_verbosity
            // 
            this.lbl_verbosity.AutoSize = true;
            this.lbl_verbosity.Location = new System.Drawing.Point(515, 165);
            this.lbl_verbosity.Name = "lbl_verbosity";
            this.lbl_verbosity.Size = new System.Drawing.Size(0, 13);
            this.lbl_verbosity.TabIndex = 3;
            // 
            // cb_clearTempDir
            // 
            this.cb_clearTempDir.AutoSize = true;
            this.cb_clearTempDir.Checked = true;
            this.cb_clearTempDir.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_clearTempDir.Location = new System.Drawing.Point(593, 136);
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
            this.label4.Location = new System.Drawing.Point(488, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Message Verbosity:";
            // 
            // trb_Verbosity
            // 
            this.trb_Verbosity.LargeChange = 2;
            this.trb_Verbosity.Location = new System.Drawing.Point(488, 131);
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
            // nmup_Cores
            // 
            this.nmup_Cores.Location = new System.Drawing.Point(677, 204);
            this.nmup_Cores.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmup_Cores.Name = "nmup_Cores";
            this.nmup_Cores.Size = new System.Drawing.Size(38, 20);
            this.nmup_Cores.TabIndex = 14;
            this.nmup_Cores.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmup_Cores.ValueChanged += new System.EventHandler(this.nmup_Cores_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(590, 206);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Maximum Cores:";
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(765, 248);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 23);
            this.btn_Save.TabIndex = 16;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(590, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "# of photons per light:";
            // 
            // nmup_gatherRadius
            // 
            this.nmup_gatherRadius.DecimalPlaces = 2;
            this.nmup_gatherRadius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nmup_gatherRadius.Location = new System.Drawing.Point(677, 227);
            this.nmup_gatherRadius.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nmup_gatherRadius.Name = "nmup_gatherRadius";
            this.nmup_gatherRadius.Size = new System.Drawing.Size(52, 20);
            this.nmup_gatherRadius.TabIndex = 18;
            this.nmup_gatherRadius.ValueChanged += new System.EventHandler(this.nmup_gatherRadius_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(590, 229);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Gather Radius:";
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 273);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.nmup_gatherRadius);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.nmup_Cores);
            this.Controls.Add(this.cb_clearTempDir);
            this.Controls.Add(this.lbl_verbosity);
            this.Controls.Add(this.nmup_numPhotons);
            this.Controls.Add(this.btn_tempFiles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_useTrueAttenuation);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lb_erfFiles);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lb_filePaths);
            this.Controls.Add(this.tb_tempDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.trb_Verbosity);
            this.Controls.Add(this.btn_addFilePath);
            this.Controls.Add(this.btn_changeWorkingDir);
            this.Controls.Add(this.btn_removeFilePath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btn_addERFFile);
            this.Controls.Add(this.tb_workingDirectory);
            this.Controls.Add(this.btn_removeERFFile);
            this.Name = "SettingsWindow";
            this.Text = "SettingsWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nmup_numPhotons)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trb_Verbosity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_Cores)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmup_gatherRadius)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lb_filePaths;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lb_erfFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_removeERFFile;
        private System.Windows.Forms.Button btn_addERFFile;
        private System.Windows.Forms.Button btn_removeFilePath;
        private System.Windows.Forms.Button btn_addFilePath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_workingDirectory;
        private System.Windows.Forms.Button btn_changeWorkingDir;
        private System.Windows.Forms.TrackBar trb_Verbosity;
        private System.Windows.Forms.CheckBox cb_clearTempDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nmup_numPhotons;
        private System.Windows.Forms.CheckBox cb_useTrueAttenuation;
        private System.Windows.Forms.Label lbl_verbosity;
        private System.Windows.Forms.Button btn_tempFiles;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_tempDirectory;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.NumericUpDown nmup_Cores;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nmup_gatherRadius;
        private System.Windows.Forms.Label label8;
    }
}