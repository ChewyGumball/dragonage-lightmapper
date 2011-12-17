namespace DALightmapper
{
    partial class OpenGLPreview
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
            this.glControl1 = new OpenTK.GLControl();
            this.btn_choose = new System.Windows.Forms.Button();
            this.btn_showUV = new System.Windows.Forms.Button();
            this.btn_show3D = new System.Windows.Forms.Button();
            this.tb_path = new System.Windows.Forms.TextBox();
            this.btn_load = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lbl_progressStatus = new System.Windows.Forms.Label();
            this.btn_prev = new System.Windows.Forms.Button();
            this.btn_next = new System.Windows.Forms.Button();
            this.lbl_meshNum = new System.Windows.Forms.Label();
            this.btn_showLightmap = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_showAll = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(3, 3);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(512, 512);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl1_KeyDown);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseLeave += new System.EventHandler(this.glControl1_MouseLeave);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
            // 
            // btn_choose
            // 
            this.btn_choose.Location = new System.Drawing.Point(440, 568);
            this.btn_choose.Name = "btn_choose";
            this.btn_choose.Size = new System.Drawing.Size(75, 23);
            this.btn_choose.TabIndex = 2;
            this.btn_choose.Text = "Choose";
            this.btn_choose.UseVisualStyleBackColor = true;
            this.btn_choose.Click += new System.EventHandler(this.btn_choose_Click);
            // 
            // btn_showUV
            // 
            this.btn_showUV.Location = new System.Drawing.Point(92, 14);
            this.btn_showUV.Name = "btn_showUV";
            this.btn_showUV.Size = new System.Drawing.Size(43, 23);
            this.btn_showUV.TabIndex = 3;
            this.btn_showUV.Text = "UV";
            this.btn_showUV.UseVisualStyleBackColor = true;
            this.btn_showUV.Click += new System.EventHandler(this.btn_showUV_Click);
            // 
            // btn_show3D
            // 
            this.btn_show3D.Location = new System.Drawing.Point(141, 14);
            this.btn_show3D.Name = "btn_show3D";
            this.btn_show3D.Size = new System.Drawing.Size(43, 23);
            this.btn_show3D.TabIndex = 4;
            this.btn_show3D.Text = "3D";
            this.btn_show3D.UseVisualStyleBackColor = true;
            this.btn_show3D.Click += new System.EventHandler(this.btn_show3D_Click);
            // 
            // tb_path
            // 
            this.tb_path.Location = new System.Drawing.Point(55, 570);
            this.tb_path.Name = "tb_path";
            this.tb_path.Size = new System.Drawing.Size(379, 20);
            this.tb_path.TabIndex = 5;
            // 
            // btn_load
            // 
            this.btn_load.Location = new System.Drawing.Point(3, 568);
            this.btn_load.Name = "btn_load";
            this.btn_load.Size = new System.Drawing.Size(46, 23);
            this.btn_load.TabIndex = 6;
            this.btn_load.Text = "Load";
            this.btn_load.UseVisualStyleBackColor = true;
            this.btn_load.Click += new System.EventHandler(this.btn_load_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Model Hierarchy|*.mmh|Mesh|*.msh|Targa|*.tga|Level|*.lvl";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // lbl_progressStatus
            // 
            this.lbl_progressStatus.AutoSize = true;
            this.lbl_progressStatus.Location = new System.Drawing.Point(255, 551);
            this.lbl_progressStatus.Name = "lbl_progressStatus";
            this.lbl_progressStatus.Size = new System.Drawing.Size(0, 13);
            this.lbl_progressStatus.TabIndex = 8;
            // 
            // btn_prev
            // 
            this.btn_prev.Location = new System.Drawing.Point(236, 14);
            this.btn_prev.Name = "btn_prev";
            this.btn_prev.Size = new System.Drawing.Size(26, 23);
            this.btn_prev.TabIndex = 9;
            this.btn_prev.Text = "<";
            this.btn_prev.UseVisualStyleBackColor = true;
            this.btn_prev.Click += new System.EventHandler(this.btn_prev_Click);
            // 
            // btn_next
            // 
            this.btn_next.Location = new System.Drawing.Point(268, 14);
            this.btn_next.Name = "btn_next";
            this.btn_next.Size = new System.Drawing.Size(26, 23);
            this.btn_next.TabIndex = 10;
            this.btn_next.Text = ">";
            this.btn_next.UseVisualStyleBackColor = true;
            this.btn_next.Click += new System.EventHandler(this.btn_next_Click);
            // 
            // lbl_meshNum
            // 
            this.lbl_meshNum.AutoSize = true;
            this.lbl_meshNum.Location = new System.Drawing.Point(195, 552);
            this.lbl_meshNum.Name = "lbl_meshNum";
            this.lbl_meshNum.Size = new System.Drawing.Size(0, 13);
            this.lbl_meshNum.TabIndex = 11;
            // 
            // btn_showLightmap
            // 
            this.btn_showLightmap.Location = new System.Drawing.Point(11, 14);
            this.btn_showLightmap.Name = "btn_showLightmap";
            this.btn_showLightmap.Size = new System.Drawing.Size(75, 23);
            this.btn_showLightmap.TabIndex = 12;
            this.btn_showLightmap.Text = "Lightmap";
            this.btn_showLightmap.UseVisualStyleBackColor = true;
            this.btn_showLightmap.Click += new System.EventHandler(this.btn_showLightmap_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_showAll);
            this.groupBox1.Controls.Add(this.btn_showLightmap);
            this.groupBox1.Controls.Add(this.btn_showUV);
            this.groupBox1.Controls.Add(this.btn_prev);
            this.groupBox1.Controls.Add(this.btn_next);
            this.groupBox1.Controls.Add(this.btn_show3D);
            this.groupBox1.Location = new System.Drawing.Point(212, 521);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(303, 43);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Model";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(3, 521);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(168, 43);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Level";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(87, 14);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Patches";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 14);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Wireframe";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btn_showAll
            // 
            this.btn_showAll.Location = new System.Drawing.Point(191, 14);
            this.btn_showAll.Name = "btn_showAll";
            this.btn_showAll.Size = new System.Drawing.Size(39, 23);
            this.btn_showAll.TabIndex = 13;
            this.btn_showAll.Text = "All";
            this.btn_showAll.UseVisualStyleBackColor = true;
            this.btn_showAll.Click += new System.EventHandler(this.btn_showAll_Click);
            // 
            // OpenGLPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 595);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbl_meshNum);
            this.Controls.Add(this.lbl_progressStatus);
            this.Controls.Add(this.btn_load);
            this.Controls.Add(this.tb_path);
            this.Controls.Add(this.btn_choose);
            this.Controls.Add(this.glControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "OpenGLPreview";
            this.Text = "OpenGLPreview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenGLPreview_FormClosing);
            this.Load += new System.EventHandler(this.OpenGLPreview_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Button btn_choose;
        private System.Windows.Forms.Button btn_showUV;
        private System.Windows.Forms.Button btn_show3D;
        private System.Windows.Forms.TextBox tb_path;
        private System.Windows.Forms.Button btn_load;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lbl_progressStatus;
        private System.Windows.Forms.Button btn_prev;
        private System.Windows.Forms.Button btn_next;
        private System.Windows.Forms.Label lbl_meshNum;
        private System.Windows.Forms.Button btn_showLightmap;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_showAll;
    }
}