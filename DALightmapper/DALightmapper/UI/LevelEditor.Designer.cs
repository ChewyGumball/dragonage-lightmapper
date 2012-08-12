namespace DALightmapper.UI
{
    partial class LevelEditor
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.glControl1 = new OpenTK.GLControl();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.cb_Patches = new System.Windows.Forms.CheckBox();
            this.cb_Shaded = new System.Windows.Forms.CheckBox();
            this.cb_Wireframe = new System.Windows.Forms.CheckBox();
            this.lb_Level = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.LightTab = new System.Windows.Forms.TabPage();
            this.lb_LightType = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.lstb_Lights = new System.Windows.Forms.ListBox();
            this.ObjectTab = new System.Windows.Forms.TabPage();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_InstanceName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstb_Objects = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.LightTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.ObjectTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.glControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1091, 526);
            this.splitContainer1.SplitterDistance = 785;
            this.splitContainer1.TabIndex = 0;
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(786, 526);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.button1);
            this.splitContainer2.Panel1.Controls.Add(this.cb_Patches);
            this.splitContainer2.Panel1.Controls.Add(this.cb_Shaded);
            this.splitContainer2.Panel1.Controls.Add(this.cb_Wireframe);
            this.splitContainer2.Panel1.Controls.Add(this.lb_Level);
            this.splitContainer2.Panel1.Controls.Add(this.label5);
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(302, 526);
            this.splitContainer2.SplitterDistance = 68;
            this.splitContainer2.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(3, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(82, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Save Settings";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // cb_Patches
            // 
            this.cb_Patches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_Patches.AutoSize = true;
            this.cb_Patches.Location = new System.Drawing.Point(234, 47);
            this.cb_Patches.Name = "cb_Patches";
            this.cb_Patches.Size = new System.Drawing.Size(65, 17);
            this.cb_Patches.TabIndex = 4;
            this.cb_Patches.Text = "Patches";
            this.cb_Patches.UseVisualStyleBackColor = true;
            // 
            // cb_Shaded
            // 
            this.cb_Shaded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_Shaded.AutoSize = true;
            this.cb_Shaded.Location = new System.Drawing.Point(170, 47);
            this.cb_Shaded.Name = "cb_Shaded";
            this.cb_Shaded.Size = new System.Drawing.Size(63, 17);
            this.cb_Shaded.TabIndex = 3;
            this.cb_Shaded.Text = "Shaded";
            this.cb_Shaded.UseVisualStyleBackColor = true;
            // 
            // cb_Wireframe
            // 
            this.cb_Wireframe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_Wireframe.AutoSize = true;
            this.cb_Wireframe.Location = new System.Drawing.Point(95, 47);
            this.cb_Wireframe.Name = "cb_Wireframe";
            this.cb_Wireframe.Size = new System.Drawing.Size(74, 17);
            this.cb_Wireframe.TabIndex = 2;
            this.cb_Wireframe.Text = "Wireframe";
            this.cb_Wireframe.UseVisualStyleBackColor = true;
            // 
            // lb_Level
            // 
            this.lb_Level.AutoSize = true;
            this.lb_Level.Location = new System.Drawing.Point(10, 21);
            this.lb_Level.Name = "lb_Level";
            this.lb_Level.Size = new System.Drawing.Size(72, 13);
            this.lb_Level.TabIndex = 1;
            this.lb_Level.Text = "sdfgerhetrhrth";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Level:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.LightTab);
            this.tabControl1.Controls.Add(this.ObjectTab);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(299, 451);
            this.tabControl1.TabIndex = 0;
            // 
            // LightTab
            // 
            this.LightTab.Controls.Add(this.lb_LightType);
            this.LightTab.Controls.Add(this.label2);
            this.LightTab.Controls.Add(this.label1);
            this.LightTab.Controls.Add(this.numericUpDown1);
            this.LightTab.Controls.Add(this.lstb_Lights);
            this.LightTab.Location = new System.Drawing.Point(4, 22);
            this.LightTab.Name = "LightTab";
            this.LightTab.Padding = new System.Windows.Forms.Padding(3);
            this.LightTab.Size = new System.Drawing.Size(291, 413);
            this.LightTab.TabIndex = 0;
            this.LightTab.Text = "Lights";
            this.LightTab.UseVisualStyleBackColor = true;
            // 
            // lb_LightType
            // 
            this.lb_LightType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lb_LightType.AutoSize = true;
            this.lb_LightType.Location = new System.Drawing.Point(62, 392);
            this.lb_LightType.Name = "lb_LightType";
            this.lb_LightType.Size = new System.Drawing.Size(45, 13);
            this.lb_LightType.TabIndex = 4;
            this.lb_LightType.Text = "Ambient";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 392);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Light Type:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(114, 392);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Photons:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown1.Location = new System.Drawing.Point(166, 390);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 1;
            // 
            // lstb_Lights
            // 
            this.lstb_Lights.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstb_Lights.FormattingEnabled = true;
            this.lstb_Lights.Location = new System.Drawing.Point(6, 6);
            this.lstb_Lights.Name = "lstb_Lights";
            this.lstb_Lights.Size = new System.Drawing.Size(280, 381);
            this.lstb_Lights.TabIndex = 0;
            // 
            // ObjectTab
            // 
            this.ObjectTab.Controls.Add(this.numericUpDown2);
            this.ObjectTab.Controls.Add(this.label4);
            this.ObjectTab.Controls.Add(this.lb_InstanceName);
            this.ObjectTab.Controls.Add(this.label3);
            this.ObjectTab.Controls.Add(this.lstb_Objects);
            this.ObjectTab.Location = new System.Drawing.Point(4, 22);
            this.ObjectTab.Name = "ObjectTab";
            this.ObjectTab.Padding = new System.Windows.Forms.Padding(3);
            this.ObjectTab.Size = new System.Drawing.Size(291, 425);
            this.ObjectTab.TabIndex = 1;
            this.ObjectTab.Text = "Objects";
            this.ObjectTab.UseVisualStyleBackColor = true;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDown2.Location = new System.Drawing.Point(92, 401);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown2.TabIndex = 4;
            this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown2.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 403);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Light Map Size:";
            // 
            // lb_InstanceName
            // 
            this.lb_InstanceName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lb_InstanceName.AutoSize = true;
            this.lb_InstanceName.Location = new System.Drawing.Point(89, 386);
            this.lb_InstanceName.Name = "lb_InstanceName";
            this.lb_InstanceName.Size = new System.Drawing.Size(117, 13);
            this.lb_InstanceName.TabIndex = 2;
            this.lb_InstanceName.Text = "ffiuhiwuhe98hf928hkhjf";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 386);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Instance Name:";
            // 
            // lstb_Objects
            // 
            this.lstb_Objects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstb_Objects.FormattingEnabled = true;
            this.lstb_Objects.Location = new System.Drawing.Point(6, 6);
            this.lstb_Objects.Name = "lstb_Objects";
            this.lstb_Objects.Size = new System.Drawing.Size(279, 368);
            this.lstb_Objects.TabIndex = 0;
            // 
            // LevelEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1091, 526);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LevelEditor";
            this.Text = "LevelEditor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.LightTab.ResumeLayout(false);
            this.LightTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ObjectTab.ResumeLayout(false);
            this.ObjectTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox cb_Patches;
        private System.Windows.Forms.CheckBox cb_Shaded;
        private System.Windows.Forms.CheckBox cb_Wireframe;
        private System.Windows.Forms.Label lb_Level;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage LightTab;
        private System.Windows.Forms.Label lb_LightType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.ListBox lstb_Lights;
        private System.Windows.Forms.TabPage ObjectTab;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lb_InstanceName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lstb_Objects;
    }
}