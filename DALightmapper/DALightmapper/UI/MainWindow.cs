using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Ben;

using Bioware.Files;
using Bioware.IO;

namespace DALightmapper
{
    public partial class MainWindow : Form
    {
        OpenGLPreview oglPreviewWindow;
        SettingsWindow settingsWindow;

        Queue<String> jobs;

        public MainWindow()
        {


            InitializeComponent();
            Settings.stream.attachTextBox(tb_Status);
            Settings.stream.attachProgressBar(pg_Status);
            Settings.initializeSettings(Environment.GetCommandLineArgs());

            foreach (String s in Settings.scenes)
            {
                lb_Files.Items.Add(s);
            }

            oglPreviewWindow = new OpenGLPreview();
            settingsWindow = new SettingsWindow();
            Settings.stream.WriteLine("Command line arguments:");
            foreach (String s in Environment.GetCommandLineArgs())
            {
                Settings.stream.WriteText(s + " ");
            }
            Settings.stream.WriteLine();

            try
            {
                if (!Directory.Exists(Settings.tempDirectory))
                {
                    Directory.CreateDirectory(Settings.tempDirectory);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Settings.stream.WriteLine("The temp directory (\"{0}\") could not be accessed due to permissions.", Settings.tempDirectory);
                Settings.stream.indent++;
                Settings.stream.WriteLine("Please change the permissions of the parent directory.");
                Settings.stream.indent--;
            }
            catch (Exception e)
            {
                Settings.stream.WriteLine("There was an error accessing the temp directory (\"{0}\"), {1}", Settings.tempDirectory, e.Message);
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            jobs = new Queue<String>();
            foreach (String s in lb_Files.Items)
            {
                jobs.Enqueue(s);
            }

            if (jobs.Count > 0)
            {
                Thread jobThread = new Thread(proccessJobs);
                jobThread.Start();
            }
            else
            {
                Settings.stream.WriteLine("There are no jobs to start.");
            }

        }

        //Processes the next job
        private void proccessJobs()
        {
            toggleButtons(false);
            while (jobs.Count > 0)
            {
                Settings.stream.WriteLine();
                String currentJob = jobs.Dequeue();

                Settings.stream.WriteLine("Starting next job ({0}):", currentJob);

                String extention = Path.GetExtension(currentJob);
                if (extention == ".gff" || extention == ".msh" || extention == ".mmh" || extention == ".tmsh")
                {
                    GFF file = ResourceManager.findFile<GFF>(currentJob);
                    if (file != null)
                    {
                        Settings.stream.WriteLine("This is a GFF file, printing struct definitions:");
                        Settings.stream.WriteText(ResourceManager.getGFFLayout(file));
                    }
                    else
                    {
                        Settings.stream.WriteLine("Couldn't find {0}, skipping this job.", currentJob);
                    }
                }
                else
                {
                    //Run lightmapping!
                    try
                    {
                        Scene currentScene;
                        switch (Path.GetExtension(currentJob))
                        {
                            case ".xml": currentScene = new XMLScene(currentJob); break;
                            case ".lvl": currentScene = new LevelScene(currentJob); break;

                            default: throw new LightmappingAbortedException("The scene \"" + currentJob + "\" is not a valid file format. Valid file formats are .xml and .lvl.");
                        }

                        List<LightMap> maps = Lightmapper.runLightmaps(currentScene.lightmapModels, currentScene.lights);

                        currentScene.exportLightmaps(maps);
                    }
                    catch (LightmappingAbortedException e)
                    {
                        Settings.stream.WriteLine("M:" + e.Message);
                    }
                }
            }
            Settings.stream.WriteLine("Finished all jobs.");

            toggleButtons(true);
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            Settings.stream.clear();
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            Lightmapper.abort = true;
            Settings.stream.SetProgressBarMaximum(1);
            Settings.stream.WriteLine();
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            String[] paths = openFileDialog1.FileNames;
            for (int i = 0; i < paths.Length; i++)
            {
                if (lb_Files.Items.Contains(paths[i]))
                {
                    Settings.stream.WriteLine(Verbosity.Low, "Job already exists: {0}", paths[i]);
                }
                else
                {
                    Settings.stream.WriteLine(Verbosity.Low, "Adding job to list: {0}", paths[i]);
                    lb_Files.Items.Add(paths[i]);
                }
            }
        }

        private void btn_Remove_Click(object sender, EventArgs e)
        {
            ListBox.SelectedIndexCollection indices = lb_Files.SelectedIndices;
            while (indices.Count != 0)
            {
                Settings.stream.WriteLine(Verbosity.Low, "Removing job from list: {0}", lb_Files.Items[indices[0]]);
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

        private void toggleButtons(bool toggle)
        {
            if (btn_Start.InvokeRequired)
            {
                Invoke((Action)delegate() { toggleButtons(toggle); });
            }
            else
            {

                btn_Start.Enabled = toggle;
                btn_Stop.Enabled = !toggle;
                btn_Add.Enabled = toggle;
                btn_Remove.Enabled = toggle;
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
                    foreach (String s in Directory.GetDirectories(Settings.tempDirectory))
                    {
                        Directory.Delete(s, true);
                    }
                    foreach (String s in Directory.GetFiles(Settings.tempDirectory))
                    {
                        File.Delete(s);
                    }
                }
                catch (Exception ex)
                {
                    Settings.stream.WriteLine("Could not clean up temporary directory: {0}", ex.Message);
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsWindow.Show();
        }
    }
}
