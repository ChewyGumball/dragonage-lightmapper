﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Ben;
using Bioware.Files;
namespace DALightmapper
{
    public partial class SettingsWindow : Form
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Settings.initializeSettings();
            //Make sure working directory is working!

            trb_Verbosity.Value = (int)Settings.verboseStatus;
            lbl_verbosity.Text = Settings.verboseStatus.ToString();

            cb_useTrueAttenuation.Checked = Settings.useTrueAttenuation;
            cb_clearTempDir.Checked = Settings.cleanUpTempFiles;

            foreach (String s in Settings.filePaths)
            {
                lb_filePaths.Items.Add(s);
            }
            foreach (ERF erf in Settings.erfFiles)
            {
                lb_erfFiles.Items.Add(erf.path);
            }

            tb_workingDirectory.Text = Settings.workingDirectory;
            tb_tempDirectory.Text = Settings.tempDirectory;

            nmup_numPhotons.Value = Settings.numPhotonsPerLight;
            nmup_Cores.Maximum = Environment.ProcessorCount;
            nmup_Cores.Minimum = 1;
            nmup_Cores.Value = Settings.maxThreads;

        }

        private void nmup_numBounces_ValueChanged(object sender, EventArgs e)
        {
            Settings.numPhotonsPerLight = (int)nmup_numPhotons.Value;
        }
        
        private void cb_useTrueAttenuation_CheckedChanged(object sender, EventArgs e)
        {
            Settings.useTrueAttenuation = cb_useTrueAttenuation.Checked;
        }

        private void trb_Verbosity_Scroll(object sender, EventArgs e)
        {
            Settings.verboseStatus = (Verbosity)trb_Verbosity.Value;
            Settings.stream.verbosity = Settings.verboseStatus;
            lbl_verbosity.Text = Settings.verboseStatus.ToString();
        }

        private void btn_changeWorkingDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            tb_workingDirectory.Text = folderBrowserDialog1.SelectedPath;
            Settings.workingDirectory = tb_workingDirectory.Text;
        }

        private void btn_tempFiles_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            tb_tempDirectory.Text = folderBrowserDialog1.SelectedPath;
            Settings.tempDirectory = tb_tempDirectory.Text;
        }

        private void btn_addFilePath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            String path = folderBrowserDialog1.SelectedPath;
            Settings.filePaths.Add(path);
            lb_filePaths.Items.Add(path);
        }

        private void btn_addERFFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            String[] paths = openFileDialog1.FileNames;
            foreach (String s in paths)
            {
                Settings.erfFiles.Add(new ERF(s));
                lb_erfFiles.Items.Add(s);
            }
        }

        private void btn_removeFilePath_Click(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lb_filePaths.SelectedIndices;
            while (indices.Count != 0)
            {
                lb_filePaths.Items.RemoveAt(indices[0]);
            }
        }

        private void btn_removeERFFile_Click(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lb_erfFiles.SelectedIndices;
            while (indices.Count != 0)
            {
                lb_erfFiles.Items.RemoveAt(indices[0]);
            }
        }

        private void cb_clearTempDir_CheckedChanged(object sender, EventArgs e)
        {
            Settings.cleanUpTempFiles = cb_clearTempDir.Checked;
        }

        private void SettingsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Settings.saveSettings();
            Hide();
        }

        private void nmup_Cores_ValueChanged(object sender, EventArgs e)
        {
            Settings.maxThreads = (int)nmup_Cores.Value;
            ThreadPool.SetMaxThreads(Settings.maxThreads, Settings.maxThreads);
            Settings.stream.AppendFormatLine("MaxThreads = {0}", Settings.maxThreads);
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            Settings.saveSettings();
        }
    }
}
