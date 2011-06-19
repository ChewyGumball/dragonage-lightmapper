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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lbl_progressStatus = new System.Windows.Forms.Label();
            this.btn_prev = new System.Windows.Forms.Button();
            this.btn_next = new System.Windows.Forms.Button();
            this.lbl_meshNum = new System.Windows.Forms.Label();
            this.btn_showLightmap = new System.Windows.Forms.Button();
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
            this.btn_showUV.Location = new System.Drawing.Point(3, 521);
            this.btn_showUV.Name = "btn_showUV";
            this.btn_showUV.Size = new System.Drawing.Size(75, 23);
            this.btn_showUV.TabIndex = 3;
            this.btn_showUV.Text = "UV";
            this.btn_showUV.UseVisualStyleBackColor = true;
            this.btn_showUV.Click += new System.EventHandler(this.btn_showUV_Click);
            // 
            // btn_show3D
            // 
            this.btn_show3D.Location = new System.Drawing.Point(84, 521);
            this.btn_show3D.Name = "btn_show3D";
            this.btn_show3D.Size = new System.Drawing.Size(75, 23);
            this.btn_show3D.TabIndex = 4;
            this.btn_show3D.Text = "3D";
            this.btn_show3D.UseVisualStyleBackColor = true;
            this.btn_show3D.Click += new System.EventHandler(this.btn_show3D_Click);
            // 
            // tb_path
            // 
            this.tb_path.Location = new System.Drawing.Point(3, 570);
            this.tb_path.Name = "tb_path";
            this.tb_path.Size = new System.Drawing.Size(431, 20);
            this.tb_path.TabIndex = 5;
            // 
            // btn_load
            // 
            this.btn_load.Location = new System.Drawing.Point(165, 521);
            this.btn_load.Name = "btn_load";
            this.btn_load.Size = new System.Drawing.Size(84, 23);
            this.btn_load.TabIndex = 6;
            this.btn_load.Text = "Load";
            this.btn_load.UseVisualStyleBackColor = true;
            this.btn_load.Click += new System.EventHandler(this.btn_load_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "\"Model Hierarchy|*.mmh|Mesh|*.msh\"";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(255, 521);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(260, 23);
            this.progressBar1.TabIndex = 7;
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
            this.btn_prev.Location = new System.Drawing.Point(165, 546);
            this.btn_prev.Name = "btn_prev";
            this.btn_prev.Size = new System.Drawing.Size(26, 23);
            this.btn_prev.TabIndex = 9;
            this.btn_prev.Text = "<";
            this.btn_prev.UseVisualStyleBackColor = true;
            this.btn_prev.Click += new System.EventHandler(this.btn_prev_Click);
            // 
            // btn_next
            // 
            this.btn_next.Location = new System.Drawing.Point(223, 546);
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
            this.btn_showLightmap.Location = new System.Drawing.Point(3, 546);
            this.btn_showLightmap.Name = "btn_showLightmap";
            this.btn_showLightmap.Size = new System.Drawing.Size(75, 23);
            this.btn_showLightmap.TabIndex = 12;
            this.btn_showLightmap.Text = "Lightmap";
            this.btn_showLightmap.UseVisualStyleBackColor = true;
            this.btn_showLightmap.Click += new System.EventHandler(this.btn_showLightmap_Click);
            // 
            // OpenGLPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 595);
            this.Controls.Add(this.btn_showLightmap);
            this.Controls.Add(this.lbl_meshNum);
            this.Controls.Add(this.btn_next);
            this.Controls.Add(this.btn_prev);
            this.Controls.Add(this.lbl_progressStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btn_load);
            this.Controls.Add(this.tb_path);
            this.Controls.Add(this.btn_show3D);
            this.Controls.Add(this.btn_showUV);
            this.Controls.Add(this.btn_choose);
            this.Controls.Add(this.glControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "OpenGLPreview";
            this.Text = "OpenGLPreview";
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbl_progressStatus;
        private System.Windows.Forms.Button btn_prev;
        private System.Windows.Forms.Button btn_next;
        private System.Windows.Forms.Label lbl_meshNum;
        private System.Windows.Forms.Button btn_showLightmap;
    }
}