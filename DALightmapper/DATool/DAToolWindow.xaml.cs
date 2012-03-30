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
using DALightmapper;
using Bioware.Files;
using Bioware.Structs;
using Ben;

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

        private void viewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog();
            browser.ShowNewFolderButton = false;
            System.Windows.Forms.DialogResult result = browser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (!Settings.filePaths.Contains(browser.SelectedPath))
                {
                    Settings.filePaths.Add(browser.SelectedPath);
                    locationListBox.Items.Add(browser.SelectedPath);
                }
                foreach (String s in Directory.GetDirectories(browser.SelectedPath, "*", SearchOption.AllDirectories))
                {
                    if (!Settings.filePaths.Contains(s))
                    {
                        Settings.filePaths.Add(s);
                        locationListBox.Items.Add(s);
                    }
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
                    bool alreadyAdded = false;
                    foreach (ERF erf in Settings.erfFiles)
                    {
                        if (erf.path == s)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }
                    if (!alreadyAdded)
                    {
                        ERF newErf = new ERF(s);
                        newErf.readKeyData();
                        Settings.erfFiles.Add(newErf);
                        locationListBox.Items.Add(s);
                    }
                }
            }
        }

        private void locationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (locationListBox.SelectedItem != null)
            {
                String location = (String)locationListBox.SelectedItem;
                if (Settings.filePaths.Contains(location))
                {
                    Directory.GetDirectories(location);
                }
                else
                {
                    ERF selectedERF = null;
                    foreach (ERF erf in Settings.erfFiles)
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
                            if (Path.GetExtension(s) == ".mmh" || Path.GetExtension(s) == ".msh" || Path.GetExtension(s) == ".dds" || Path.GetExtension(s) == ".mao")
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
                    GFF tempGFF = IO.findFile<GFF>(itemName);
                    if (tempGFF != null)
                    {
                        renderer.displayModel(new ModelMesh(tempGFF));
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find {0}", itemName);
                    }
                }
                else if (Path.GetExtension(itemName) == ".mmh")
                {
                    GFF tempGFF = IO.findFile<GFF>(itemName);
                    if (tempGFF != null)
                    {
                        ModelHierarchy mh = new ModelHierarchy(tempGFF);
                        if (mh.loadedMesh)
                        {
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
                        DDS texture = IO.findFile<DDS>(itemName);
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
                    MaterialObject mao = IO.findFile<MaterialObject>(itemName);
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
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            renderer.stop();
        }
    }
}
