﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ben;

using Bioware.Files;

namespace DALightmapper
{
    public partial class MainWindow : Form
    {
        int currentJob;

        OpenGLPreview oglPreviewWindow;
        SettingsWindow settingsWindow;

        Thread runThread;

        public MainWindow()
        {


            InitializeComponent();
            Settings.stream.attachTextBox(tb_Status);
            Settings.stream.attachProgressBar(pg_Status);
            Settings.initializeSettings();
            oglPreviewWindow = new OpenGLPreview();
            settingsWindow = new SettingsWindow();

            String a = "fhi_floorstoneedge_0.mmh";
            String b = "fhi_floorstone_0.mmh";
            String c = "fhi_floorstone3x3_0.mmh";
            String d = "fhi_floorstoneedge_0.phy";

            Settings.stream.AppendFormatLine("{0} to {1} = {2}", a, b, a.CompareTo(b));
            Settings.stream.AppendFormatLine("{0} to {1} = {2}", b, c, b.CompareTo(c));
            Settings.stream.AppendFormatLine("{0} to {1} = {2}", c, d, c.CompareTo(d));
            Settings.stream.AppendFormatLine("{0} to {1} = {2}", d, a, d.CompareTo(a));

            
            try
            {
                if (!Directory.Exists(Settings.workingDirectory))
                {
                    Directory.CreateDirectory(Settings.workingDirectory);
                }
                if (!Directory.Exists(Settings.tempDirectory + "\\lightmaps"))
                {
                    Directory.CreateDirectory(Settings.tempDirectory + "\\lightmaps");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Settings.stream.AppendFormatLine("The working directory (\"{0}\") could not be accessed due to permissions.", Settings.workingDirectory);
                Settings.stream.indent++;
                Settings.stream.AppendLine("Please change the permissions of the parent directory.");
                Settings.stream.indent--;
            }
            catch (Exception e)
            {
                Settings.stream.AppendFormatLine("There was an error accessing the working directory (\"{0}\"), {1}", Settings.workingDirectory, e.Message);
            }

            //Plug into thread finish handler for Light Mapping
            Lightmapper.FinishedLightMapping += new Lightmapper.FinishedLightMappingEventHandler(doneLightMapping);
        
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (lb_Files.Items.Count > 0)
            {
                btn_Start.Enabled = false;
                btn_Stop.Enabled = true;
                btn_Add.Enabled = false;
                btn_Remove.Enabled = false;

                currentJob = 0;
                proccessNextJob();
            }
            else
            {
                Settings.stream.AppendLine("There are no jobs to start.");
            }

        }

        //Processes the next job
        private void proccessNextJob()
        {
            if (currentJob > 0 && Settings.cleanUpTempFiles && IO.numTempFiles > 0)
            {
                Settings.stream.AppendLine("Closing temp files first.");
                IO.cleanUpTempFiles();
            }

            Settings.stream.AppendLine();
            if (currentJob >= lb_Files.Items.Count)
            {
                Settings.stream.AppendLine("Finished all jobs.");
                btn_Start.Enabled = true;
                btn_Stop.Enabled = false;
                btn_Add.Enabled = true;
                btn_Remove.Enabled = true; ;
            }
            else
            {
                Settings.stream.AppendFormatLine("Starting job {0}:",currentJob + 1);
                //Run lightmapping!
                int jobIndex = currentJob;
                ThreadStart job = delegate {
                    try
                    {
                        Lightmapper.runLightmaps(lb_Files.Items[jobIndex].ToString());
                    }
                    catch (LightmappingAbortedException)
                    { }
                };
                runThread = new Thread(job);
                runThread.Start();
                currentJob++;
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            Settings.stream.clear();
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            currentJob = lb_Files.Items.Count + 1;
            Lightmapper.abort = true;
            btn_Start.Enabled = true;
            btn_Stop.Enabled = false;
            btn_Add.Enabled = true;
            btn_Remove.Enabled = true;
            Settings.stream.SetProgressBarMaximum(1);
            Settings.stream.AppendLine();
        }
        
        //Forces doneLightMapping to run on the UI thread
        private delegate void doneLightMappingDelegate(FinishedLightMappingEventArgs e);
        //Saves the generated lightmaps and proceeds to next job
        private void doneLightMapping(FinishedLightMappingEventArgs e)
        {
            //Ensure this is being run on the UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new doneLightMappingDelegate(doneLightMapping), e);
            }
            else
            {
                Settings.stream.AppendLine(e.message);

                if (e.successful)
                {
                    //Save lightmaps properly
                }

                Lightmapper.abort = false;
                Settings.stream.AppendLine("Procceding to next job.");
                //process next job
                proccessNextJob();
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            String[] paths = openFileDialog1.FileNames;
            for (int i = 0; i < paths.Length; i++)
            {
                if (lb_Files.Items.Contains(paths[i]))
                {
                    Settings.stream.AppendFormatLine(Verbosity.Low, "Job already exists: {0}", paths[i]);
                }
                else
                {
                    Settings.stream.AppendFormatLine(Verbosity.Low, "Adding job to list: {0}", paths[i]);
                    lb_Files.Items.Add(paths[i]);
                }
            }
        }

        private void btn_Remove_Click(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lb_Files.SelectedIndices;
            while (indices.Count != 0)
            {
                Settings.stream.AppendFormatLine(Verbosity.Low, "Removing job from list: {0}", lb_Files.Items[indices[0]]);
                lb_Files.Items.RemoveAt(indices[0]);
            }
        }

        private void lb_Files_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                //Find the beginning of the selection
                int selection = lb_Files.SelectedIndex;
                //Remove the selected entries
                btn_Remove_Click(sender, e);

                //If there are things left to select
                if (lb_Files.Items.Count > 0)
                {
                    //Restore the selection to the nearest entry
                    if (lb_Files.Items.Count < selection)
                    {
                        selection = lb_Files.Items.Count;
                    }

                    lb_Files.SelectedIndex = selection;
                }
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oglPreviewWindow.Show();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Save the settings
            Settings.saveSettings();
            //Clean up temp files
            if (Settings.cleanUpTempFiles)
            {
                try
                {
                    IO.cleanUpTempFiles();
                    Directory.Delete(Settings.tempDirectory, true);
                }
                catch (Exception ex)
                {
                    Settings.stream.AppendFormatLine("Could not clean up temporary directory: {0}", ex.Message);
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsWindow.Show();
        }
    }
}