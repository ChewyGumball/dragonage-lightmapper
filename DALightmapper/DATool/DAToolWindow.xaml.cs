using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Threading;

using OpenTK.Graphics.OpenGL;
using OpenTK;

using Bioware.Files;
using Bioware.IO;
using Bioware.Structs;

namespace DATool
{
    /// <summary>
    /// Interaction logic for DAToolWindow.xaml
    /// </summary>
    public partial class DAToolWindow : Window
    {
        Renderer renderer;
        bool mouseOverControl = false;
        System.Drawing.Point mouseOrigin;

        public DAToolWindow()
        {
            InitializeComponent();
            glControl.Width = (int)windowsFormsHost1.Width;
            glControl.Height = (int)windowsFormsHost1.Height;
            //glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(mouseWheel);
            glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(mouseMove);
            glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(mouseEnter);
            glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(mouseLeave);
            glControl.KeyDown += new System.Windows.Forms.KeyEventHandler(keyPress);
            renderer = new Renderer(glControl);
            renderer.camera.translate(new Vector3(-5, 0, 0));
            renderer.camera.rotateUp(((float)(Math.PI / 2)));
            renderer.camera.rotateRight(-((float)(Math.PI / 2)));
            renderer.start();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (modelListBox.SelectedIndex == modelListBox.Items.Count)
            {
                modelListBox.SelectedIndex = 0;
            }
            else
            {
                modelListBox.SelectedIndex += 1;
            }
        }

        // method adds a folder into the location box, 
        // then adds all the erfs in all subfolders into the list of erfs
        private void viewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog();
            browser.ShowNewFolderButton = false;
            System.Windows.Forms.DialogResult result = browser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //User clicked OK, add the path into the location box, try to add all the erfs in there too
                if (!ResourceManager.filePaths.Contains(browser.SelectedPath))
                {
                    ResourceManager.addFilePath(browser.SelectedPath);
                }
                locationListBox.Items.Add(browser.SelectedPath);

                foreach (String s in Directory.GetDirectories(browser.SelectedPath, "*", SearchOption.AllDirectories))
                {
                    if (!ResourceManager.filePaths.Contains(s))
                    {
                        //Add all the subdirectories
                        ResourceManager.addFilePath(s);
                    }
                }

                foreach (String t in Directory.GetFiles(browser.SelectedPath, "*.erf", SearchOption.AllDirectories))
                {
                    ResourceManager.addERF(t);
                }

            }
        }
        private void viewERFButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog();
            browser.CheckFileExists = true;
            browser.Multiselect = true;
            browser.Filter = "ERF files (.erf)|*.erf";
            System.Windows.Forms.DialogResult result = browser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String s in browser.FileNames)
                {
                    if (ResourceManager.getERF(s) == null)
                    {
                        locationListBox.Items.Add(s);
                        ResourceManager.addERF(s);
                    }
                }
            }
        }

        private bool isDisplayableExtension(String t)
        {
            return t == ".mmh" || t == ".msh" || t == ".dds";
        }

        private void locationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (locationListBox.SelectedItem != null)
            {
                String location = (String)locationListBox.SelectedItem;
                modelListBox.Items.Clear();
                if (Directory.Exists(location))
                {
                    if (ResourceManager.filePaths.Contains(location))
                    {
                        //Folder has been added, try to see if the erfs have been added, if not, throw error message. Then add all the contents of erf to modellist
                        foreach (String s in Directory.GetFiles(location, "*", SearchOption.AllDirectories))
                        {
                            if (Path.GetExtension(s) == ".erf")
                            {
                                ERF myErf = ResourceManager.getERF(s);
                                if (myErf != null)
                                {
                                    foreach (String t in myErf.resourceNames)
                                    {
                                        if (isDisplayableExtension(Path.GetExtension(t)))
                                        {
                                            modelListBox.Items.Add(t);
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("the given erf doens't exist in the erf list, the file path given is: {0}", s);
                                }
                            }
                            else if (isDisplayableExtension(Path.GetExtension(s)))
                            {
                                modelListBox.Items.Add(s);
                            }
                        }
                    }
                    else
                    {
                        // If it appears in the location box, should be added already
                        Console.WriteLine("What're you doing here? This folder should be added already");
                    }
                }
                else
                {
                    // Not a direcotry, must be an ERF file
                    ERF selectedERF = null;
                    foreach (ERF erf in ResourceManager.erfFiles)
                    {
                        if (erf.path == location)
                        {
                            selectedERF = erf;
                            break;
                        }
                    }
                    if (selectedERF == null)
                    {
                        Console.WriteLine("SHIT");
                    }
                    else
                    {
                        modelListBox.Items.Clear();
                        foreach (String s in selectedERF.resourceNames)
                        {
                            if (isDisplayableExtension(Path.GetExtension(s)))
                            {
                                modelListBox.Items.Add(s);
                            }
                        }
                    }
                }
            }
        }
        private void modelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modelListBox.SelectedItem != null)
            {
                String itemName = (String)modelListBox.SelectedItem;

                if (Path.GetExtension(itemName) == ".msh")
                {
                    GFF tempGFF = ResourceManager.findFile<GFF>(itemName);
                    if (tempGFF != null)
                    {
                        renderer.clearOverlays();
                        renderer.overlayText(tempGFF.path);
                        renderer.showOverlays();
                        renderer.displayModel(new ModelMesh(tempGFF));

                    }
                    else
                    {
                        Console.WriteLine("Couldn't find {0}", itemName);
                    }
                }
                else if (Path.GetExtension(itemName) == ".mmh")
                {
                    GFF tempGFF = ResourceManager.findFile<GFF>(itemName);
                    if (tempGFF != null)
                    {
                        ModelHierarchy mh = new ModelHierarchy(tempGFF);
                        if (mh.loadedMesh)
                        {
                            renderer.clearOverlays();
                            renderer.overlayText(mh.mmhName);
                            renderer.showOverlays();
                            renderer.displayModel(mh.mesh);
                        }
                        else
                        {
                            Console.WriteLine("Couldn't load mesh {0} for {1}", mh.mshName, mh.mmhName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find {0}", itemName);
                    }
                }
                else if (Path.GetExtension(itemName) == ".dds")
                {
                    try
                    {
                        DDS texture = ResourceManager.findFile<DDS>(itemName);
                        if (texture != null)
                        {
                            renderer.displayDDS(texture);
                        }
                        else
                        {
                            Console.WriteLine("Couldn't find {0}", itemName);
                        }
                    }
                    catch (NotImplementedException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (Path.GetExtension(itemName) == ".mao")
                {
                    MaterialObject mao = ResourceManager.findFile<MaterialObject>(itemName);
                }
            }
        }

        private void mouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Console.WriteLine("{0} changed.", e.Delta / 120);
            lock (renderer.camera)
            {
                renderer.camera.translate(new Vector3(0, 0, -e.Delta / 120));
            }
        }

        private void mouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (mouseOverControl)
            {
                float side = 500f;  //Vary this to change rotation speed, higher is slower
                float diffX = (e.X - mouseOrigin.X);
                float diffY = (e.Y - mouseOrigin.Y);

                //Use cosine law on isosceles triangle, assume delta is the far side from the vertex we calculate the angle for
                float angleX = (float)Math.Acos(((2 * side * side) - (diffX * diffX)) / (2 * side * side));
                float angleY = (float)Math.Acos(((2 * side * side) - (diffY * diffY)) / (2 * side * side));

                if (diffX < 0)
                    angleX *= -1;
                if (diffY < 0)
                    angleY *= -1;

                lock (renderer.camera)
                {
                    renderer.camera.rotateRight(angleX);
                    renderer.camera.rotateUp(angleY);
                }
                mouseOrigin = e.Location;
            }
        }

        private void mouseEnter(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseOrigin = e.Location;
            mouseOverControl = true;
        }
        private void mouseLeave(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseOverControl = false;
        }
        private void keyPress(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.D:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(0.5f, 0, 0));
                    }
                    break;
                case System.Windows.Forms.Keys.S:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(0, 0, 0.5f));
                    }
                    break;
                case System.Windows.Forms.Keys.A:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(-0.5f, 0, 0));
                    }
                    break;
                case System.Windows.Forms.Keys.W:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(0, 0, -0.5f));
                    }
                    break;
                case System.Windows.Forms.Keys.R:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(0, 0.5f, 0));
                    }
                    break;
                case System.Windows.Forms.Keys.F:
                    lock (renderer.camera)
                    {
                        renderer.camera.localTranslate(new Vector3(0, -0.5f, 0));
                    }
                    break;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            renderer.stop();
        }
    }
}
