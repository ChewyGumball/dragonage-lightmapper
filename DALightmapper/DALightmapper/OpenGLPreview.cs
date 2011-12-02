using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Bioware.Files;

using Ben;

namespace DALightmapper
{
    
    enum Showing {UV, Model, Lightmap, Texture, Level};
    public partial class OpenGLPreview : Form
    {
        int currentMeshIndex;
        Mesh[] meshes;
        Targa texture;
        Level level;
        Showing currentlyShowing = Showing.UV;

        TextBitmap modelName;

        bool mouseDown = false;
        Point mouseOrigin;
        Vector3 cameraPos = new Vector3(0, 30, 30);
        Vector3 origin = new Vector3();
        Vector3 up = new Vector3(0, 1, 0);
        Vector3 colour = Vector3.Normalize(new Vector3(0, -10, -10));

        public OpenGLPreview()
        {
            InitializeComponent();
            setButtons(false);
        }

        private void setButtons(bool b)
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
            modelName = new TextBitmap(meshes[currentMeshIndex].getName(), meshes[currentMeshIndex].getName().Length, 35);
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
            else if (extention == ".tga")
            {
                texture = new Targa(filePath);
                currentlyShowing = Showing.Texture;
                refreshView();
            }
            else if (extention == ".lvl")
            {
                level = new Level(filePath);
                level.readObjectsAsync();
                refreshView();
            }
            //If its not the right type of file then print an error
            else
            {
                lbl_progressStatus.Text = "This is not a valid model (.mmh or .msh) or texture (.tga) file!";
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
            int width = glControl1.Width;
            int height = glControl1.Height;

            Mesh tris = meshes[currentMeshIndex];

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, width, height);

            GL.ClearColor(Color.Black);
            GL.Color3(Color.White);
            GL.LineWidth(0.5f);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Begin(BeginMode.Triangles);
            for (int i = 0; i < tris.getNumTris(); i++)
            {
                GL.Vertex2(tris[i].u.X * width, tris[i].u.Y * height);
                GL.Vertex2(tris[i].v.X * width, tris[i].v.Y * height);
                GL.Vertex2(tris[i].w.X * width, tris[i].w.Y * height);
            }
            GL.End();
            glControl1.SwapBuffers();
        }

        //Draws a 3d view of the current mesh 
        private void display3D()
        {
            int width = glControl1.Width;
            int height = glControl1.Height;

            GL.Viewport(0, 0, Width, Height);

            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perpective);

            setCamera();

            GL.Viewport(0, 0, width, height);

            GL.ClearColor(Color.Black);
            GL.Color3(Color.White);
            GL.LineWidth(0.5f);

            redraw();
        }

        //Draws the Lightmap UV of the current mesh
        private void displayLightmap()
        {
            int width = glControl1.Width;
            int height = glControl1.Height;

            Mesh tris = meshes[currentMeshIndex];

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, width, height);


            GL.ClearColor(Color.Black);
            GL.Color3(Color.White);
            GL.LineWidth(0.5f);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Begin(BeginMode.Triangles);
            for (int i = 0; i < tris.getNumTris(); i++)
            {
                if (tris[i].isLightmapped)
                {
                    GL.Vertex2(tris[i].a.X * width, tris[i].a.Y * height);
                    GL.Vertex2(tris[i].b.X * width, tris[i].b.Y * height);
                    GL.Vertex2(tris[i].c.X * width, tris[i].c.Y * height);
                }
            }
            GL.End();
            glControl1.SwapBuffers();
        }

        private void displayTexture()
        {
            int textureIndex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.Texture2D);

            Bitmap textureData = texture.getTextureData();
            BitmapData data = textureData.LockBits(new Rectangle(0, 0, texture.width, texture.height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, texture.width, texture.height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);

            textureData.UnlockBits(data);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-0.6f, -0.4f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(0.6f, -0.4f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(0.6f, 0.4f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-0.6f, 0.4f);
            GL.End();

            glControl1.SwapBuffers();
            GL.DeleteTexture(textureIndex);
        }

        private void displayLevel() 
        {
            //set camera to first light position
            //render geometry
            //render patches
        }

        private void setCamera()
        {
            Matrix4 lookat = Matrix4.LookAt(cameraPos, origin, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
        }

        private void redraw()
        {
            Mesh tris = meshes[currentMeshIndex];
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(BeginMode.Triangles);
            for (int i = 0; i < tris.getNumTris(); i++)
            {
                double cosine = Math.Abs(Vector3.Dot(colour, tris[i].normal));
                GL.Color3(cosine, cosine, cosine);
                GL.Vertex3(tris[i].x.X, tris[i].x.Y, tris[i].x.Z);
                GL.Vertex3(tris[i].y.X, tris[i].y.Y, tris[i].y.Z);
                GL.Vertex3(tris[i].z.X, tris[i].z.Y, tris[i].z.Z);
            }
            GL.End();
            GL.Disable(EnableCap.DepthTest);
            //modelName.draw(0,0);
            glControl1.SwapBuffers();
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
                case Showing.Texture:
                    displayTexture(); break;
                case Showing.Level:
                    displayLevel(); break;
                default:
                    lbl_progressStatus.Text = "Unknown currently showing status...?" + currentlyShowing; break;

            }
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            mouseOrigin = e.Location;
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && currentlyShowing != Showing.Texture)
            {
                origin += new Vector3(-(e.X - mouseOrigin.X) / 10, -(e.Y - mouseOrigin.Y) / 10, 0);
                mouseOrigin = e.Location;
                setCamera();
                redraw();
            }
        }

        private void OpenGLPreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
        }

        private void OpenGLPreview_Load(object sender, EventArgs e)
        {

        }
    }
}
