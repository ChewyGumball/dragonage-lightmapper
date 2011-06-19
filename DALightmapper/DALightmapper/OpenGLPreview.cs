using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using Bioware.Files;

namespace DALightmapper
{
    
    enum Showing {UV, Model, Lightmap};
    public partial class OpenGLPreview : Form
    {

        int currentMeshIndex;
        Mesh[] meshes;

        Showing currentlyShowing = Showing.UV;


        public OpenGLPreview()
        {
            InitializeComponent();
            setButtons(false);
        }

        private void setButtons(Boolean b)
        {
            btn_show3D.Enabled = b;
            btn_showUV.Enabled = b;
            btn_showLightmap.Enabled = b;
            btn_next.Enabled = b;
            btn_prev.Enabled = b;
        }

        private void setMeshNum(int i)
        {
            currentMeshIndex = i;
            lbl_meshNum.Text = (currentMeshIndex + 1) +"/" + meshes.Length;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            tb_path.Text = openFileDialog1.FileName;
        }

        private void btn_choose_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            String filePath = tb_path.Text;
            String extention = Path.GetExtension(filePath);
            List<Mesh> renderableMeshes = new List<Mesh>();

            //Try and find the model file
            if (extention == ".mmh")
            {
                GFF tempGFF = new GFF(filePath, 0);
                ModelHierarchy mh = new ModelHierarchy(tempGFF);
                renderableMeshes.AddRange(mh.mesh.toModel().meshes);
            }
            else if (extention == ".msh")
            {
                GFF tempGFF = new GFF(filePath, 0);
                ModelMesh mm = new ModelMesh(tempGFF);
                renderableMeshes.AddRange(mm.toModel().meshes);
            }
            //If its not the right type of file then print an error
            else
            {
                lbl_progressStatus.Text = "This is not a valid model (.mmh or .msh) file!";
            }
            meshes = renderableMeshes.ToArray();

            // Enable the buttons if there are actually meshes
            if (meshes.Length > 0)
            {
                setMeshNum(0);
                setButtons(true);
            }
            else
            {
                lbl_meshNum.Text = "";
                setButtons(false);

            }
        }

        //Draws the UV map of the current mesh
        private void displayUV()
        {
            //TODO
        }

        //Draws a 3d view of the current mesh 
        private void display3D()
        {

            //TODO
        }

        //Draws the Lightmap UV of the current mesh
        private void displayLightmap()
        {
            //TODO
        }

        private void btn_showUV_Click(object sender, EventArgs e)
        {
            currentlyShowing = Showing.UV;
            refreshView();
        }

        private void btn_show3D_Click(object sender, EventArgs e)
        {
            currentlyShowing = Showing.Model;
            refreshView();
        }

        private void btn_showLightmap_Click(object sender, EventArgs e)
        {
            currentlyShowing = Showing.Lightmap;
            refreshView();
        }

        private void btn_prev_Click(object sender, EventArgs e)
        {
            if (currentMeshIndex == 0)
                currentMeshIndex = meshes.Length;

            setMeshNum(currentMeshIndex - 1);

            refreshView();
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            if (currentMeshIndex == meshes.Length - 1)
                currentMeshIndex = -1;

            setMeshNum(currentMeshIndex + 1);

            refreshView();
        }

        private void refreshView()
        {
            switch (currentlyShowing)
            {
                case Showing.UV:
                    displayUV(); break;
                case Showing.Model:
                    display3D(); break;
                case Showing.Lightmap:
                    displayLightmap(); break;
                default:
                    lbl_progressStatus.Text = "Unknown currently showing status...?" + currentlyShowing; break;

            }
        }
    }
}
