using System;
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
        IO io;
        StatusStream stream;
        int currentJob;

        OpenGLPreview oglPreviewWindow;
        SettingsWindow settingsWindow;

        public MainWindow()
        {



            InitializeComponent(); 
            stream = new StatusStream(tb_Status);
            oglPreviewWindow = new OpenGLPreview();
            settingsWindow = new SettingsWindow();

            //Create the IO object
            io = new IO(stream);

            try
            {

                if (!Directory.Exists(Settings.workingDirectory))
                {
                    Directory.CreateDirectory(Settings.workingDirectory);
                }
                if (!Directory.Exists(Settings.tempDirectory))
                {
                    Directory.CreateDirectory(Settings.tempDirectory);
                }
            }
            catch (UnauthorizedAccessException)
            {
                stream.AppendLine("The working directory (\"" + Settings.workingDirectory + "\") could not be accessed due to permissions.\n\t Please change the permissions of the parent directory.", Verbosity.Warnings);
            }
            catch (Exception e)
            {
                stream.AppendLine("There was an error accessing the working directory (\"" + Settings.workingDirectory + "\"), " + e.Message, Verbosity.Warnings);
            }

            //Plug into thread finish handler for Light Mapping
            Lightmapper.FinishedLightMapping += new Lightmapper.FinishedLightMappingEventHandler(doneLightMapping);
        
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            btn_Start.Enabled = false;
            btn_Add.Enabled = false;
            btn_Remove.Enabled = false;

            currentJob = 0;
            proccessNextJob();

        }

        //Processes the next job
        private void proccessNextJob()
        {
            if (currentJob > 0 && Settings.cleanUpTempFiles && io.numTempFiles > 0)
            {
                stream.AppendText("Cleaning up temp files first.\n", Verbosity.Warnings);
                io.cleanUpTempFiles();
                stream.AppendText("\n", Verbosity.Warnings);
            }

            if (currentJob >= lb_Files.Items.Count)
            {
                stream.AppendText("Finished all jobs.\n", Verbosity.Warnings);
                btn_Start.Enabled = true;
                btn_Stop.Enabled = false;
                btn_Add.Enabled = true;
                btn_Remove.Enabled = true; ;
            }
            else
            {
                stream.AppendText("Starting job " + (currentJob + 1) + ":\n", Verbosity.Warnings);
                //Read in data
                io.readLevelAsync(lb_Files.Items[currentJob++].ToString(), new FinishedReadingEventHandler(doneReading));
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            stream.clear();
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            currentJob = lb_Files.Items.Count + 1;
            Lightmapper.abort = true;
            btn_Start.Enabled = true;
            btn_Stop.Enabled = false;
            btn_Add.Enabled = true;
            btn_Remove.Enabled = true;
        }

        //Forces doneReading to run on the UI thread
        private delegate void doneReadingDelegate(FinishedReadingEventArgs e);
        //starts the lightmapping process after file io has completed
        private void doneReading(FinishedReadingEventArgs e)
        {
            //Ensure this is being run on the UI thread
            if (this.InvokeRequired)
                this.BeginInvoke(new doneReadingDelegate(doneReading), e);
            else
            {
                stream.AppendText(e.message + " \n", Verbosity.Warnings);

                //If the reading was successful proceed with creating lightmaps
                if (e.successful)
                {
                    //Enable the stop button so the user can stop the lightmapping process
                    btn_Stop.Enabled = true;
                    ThreadStart starter = delegate { Lightmapper.runLightmaps(e.level); };
                    new Thread(starter).Start();
                }
                else
                    doneLightMapping(new FinishedLightMappingEventArgs("Did not render light maps, IO was aborted.\n"));

            }
        }

        //Forces doneLightMapping to run on the UI thread
        private delegate void doneLightMappingDelegate(FinishedLightMappingEventArgs e);
        //Saves the generated lightmaps and proceeds to next job
        private void doneLightMapping(FinishedLightMappingEventArgs e)
        {
            //Ensure this is being run on the UI thread
            if (this.InvokeRequired)
                this.BeginInvoke(new doneLightMappingDelegate(doneLightMapping), e);
            else
            {
                stream.AppendText(e.message + " \n", Verbosity.Warnings);

                //Save lightmaps properly

                stream.AppendText("Procceding to next job.\n", Verbosity.Warnings);
                stream.AppendText("\n", Verbosity.Warnings);

                //process next job
                proccessNextJob();
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            String[] paths = openFileDialog1.FileNames;
            for (int i = 0; i < paths.Length; i++)
                if (lb_Files.Items.Contains(paths[i]))
                {
                    stream.AppendText("Job already exists: " + paths[i] + "\n", Verbosity.Low);
                }
                else
                {
                    stream.AppendText("Adding job to list: " + paths[i] + "\n", Verbosity.Low);
                    lb_Files.Items.Add(paths[i]);
                }
        }

        private void btn_Remove_Click(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lb_Files.SelectedIndices;
            while (indices.Count != 0)
            {
                stream.AppendText("Removing job from list: " + lb_Files.Items[indices[0]] + "\n", Verbosity.Low);
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
                io.cleanUpTempFiles();
                Directory.Delete(Settings.tempDirectory, true);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsWindow.Show();
        }
    }
}
