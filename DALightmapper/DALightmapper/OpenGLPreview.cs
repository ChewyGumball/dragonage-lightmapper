using System;
using System.IO;
using System.Threading;
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
    
    enum Showing {Nothing, UV, Model, Lightmap, Texture, Level};
    public partial class OpenGLPreview : Form
    {
        int currentMeshIndex;
        String file;
        String drawString;
        Mesh[] meshes;
        Targa texture;
        Level level;
        Showing currentlyShowing = Showing.Nothing;

        int textureIndex;
        Bitmap bitmap;

        bool mouseDown = false;
        Point mouseOrigin;
        Vector3 cameraPos = new Vector3(0, 30, 30);
        Vector3 origin = new Vector3();
        Vector3 up = new Vector3(0, -1, 0);
        Vector3 colour = Vector3.Normalize(new Vector3(0, -10, -10));
        Camera camera = new Camera();

        public OpenGLPreview()
        {
            InitializeComponent();
            setButtons(false);
            camera = new Camera();
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
            lbl_meshNum.Text = (currentMeshIndex + 1) +"/" + meshes.Length;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            tb_path.Text = openFileDialog1.FileName;
        }

        
        //Refreshes the view with the appropriate settings and draws the text bitmap on the screen
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

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 camMatrix = camera.matrix;
            GL.LoadMatrix(ref camMatrix);

            GL.Viewport(0, 0, width, height);

            GL.ClearColor(Color.Black);
            GL.Color3(Color.White);
            GL.LineWidth(0.5f);

            redraw3D();
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
        //Draws a Texure 
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

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1, 1, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(0.1f, 0.9f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(0.9f, 0.9f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(0.9f, 0.1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(0.1f, 0.1f);
            GL.End();

            redrawText();

            glControl1.SwapBuffers();
            GL.DeleteTexture(textureIndex);
        }
        //Draws all the meshes in a level
        private void displayLevel()
        {
            int width = glControl1.Width;
            int height = glControl1.Height;

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 camMatrix = camera.matrix;
            GL.LoadMatrix(ref camMatrix);
            //GL.Translate(new Vector3(0, 10, 0));

            GL.Viewport(0, 0, width, height);

            redraw();
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
            Matrix4 cameraMatrix = camera.matrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref cameraMatrix);

            if (currentlyShowing == Showing.Model)
                redraw3D();
            else if (currentlyShowing == Showing.Level)
                redrawLevel();

            redrawText();

            glControl1.SwapBuffers();
        }

        private void redrawText()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, 1, 1, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0f, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex2(1f, 0f);
            GL.TexCoord2(1f, 1f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0f, 1f); GL.Vertex2(0f, 1f);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();

        }
        private void redrawLevel()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Enable(EnableCap.DepthTest);
            GL.Begin(BeginMode.Triangles);
            //render geometry
            foreach (ModelInstance m in level.lightmapModels)
            {
                foreach (Triangle t in m.tris)
                {
                    GL.Vertex3(t.x.X, t.x.Y, t.x.Z);
                    GL.Vertex3(t.y.X, t.y.Y, t.y.Z);
                    GL.Vertex3(t.z.X, t.z.Y, t.z.Z);
                }
            }
            GL.End();
            GL.Disable(EnableCap.DepthTest);
        }
        private void redraw3D()
        {
            Mesh tris = meshes[currentMeshIndex];
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Texture2D);
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
        }

        //Clears the text bitmap then writes the input string on it
        private void updateBitmap(String s)
        {
            Graphics gfx = Graphics.FromImage(bitmap);
            gfx.Clear(Color.Transparent);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gfx.DrawString(s, new Font(FontFamily.GenericSansSerif, 11), Brushes.White, new PointF(0, 0));

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }


        //-- Input Handlers --//
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

            drawString = "File: " + file + "\nModel: " + meshes[currentMeshIndex].getName();
            updateBitmap(drawString);

            refreshView();
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            if (currentMeshIndex == meshes.Length - 1)
                currentMeshIndex = -1;

            setMeshNum(currentMeshIndex + 1);

            drawString = "File: " + file + "\nModel: " + meshes[currentMeshIndex].getName();
            updateBitmap(drawString);

            refreshView();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            mouseOrigin = e.Location;
            //glControl1.Focus();
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && (currentlyShowing == Showing.Model || currentlyShowing == Showing.Level))
            {
                float side = 1000f;
                float diffX = (e.X - mouseOrigin.X);
                float diffY = (e.Y - mouseOrigin.Y);

                float angleX = (float)Math.Acos(((2 * side * side) - (diffX * diffX)) / (2 * side * side));
                float angleY = (float)Math.Acos(((2 * side * side) - (diffY * diffY)) / (2 * side * side));

                if (diffX < 0)
                    angleX *= -1;
                if (diffY < 0)
                    angleY *= -1;
                camera.rotateRight(angleX);
                camera.rotateUp(angleY);
                mouseOrigin = e.Location;
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

        private void glControl1_MouseLeave(object sender, EventArgs e)
        {
            mouseDown = false;
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    camera.localTranslate(new Vector3(0.5f, 0, 0));
                    redraw();
                    break;
                case Keys.S:
                    camera.localTranslate(new Vector3(0, 0, 0.5f));
                    redraw();
                    break;
                case Keys.A:
                    camera.localTranslate(new Vector3(-0.5f, 0, 0));
                    redraw();
                    break;
                case Keys.W:
                    camera.localTranslate(new Vector3(0, 0, -0.5f));
                    redraw();
                    break;
            }
        }

        private void btn_choose_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            setButtons(false);
            String filePath = tb_path.Text;
            file = filePath;
            String extention = Path.GetExtension(filePath);
            List<Mesh> renderableMeshes = new List<Mesh>();
            drawString = "File: " + file;

            updateBitmap(drawString);

            //Try and find the model file
            if (extention == ".mmh")
            {
                GFF tempGFF = new GFF(filePath, 0);
                ModelHierarchy mh = new ModelHierarchy(tempGFF);
                currentlyShowing = Showing.Model;
                meshes = mh.mesh.toModel().meshes;
                setButtons(true);
                if (meshes.Length > 0)
                {
                    setMeshNum(0);
                }
            }
            else if (extention == ".msh")
            {
                GFF tempGFF = new GFF(filePath, 0);
                ModelMesh mm = new ModelMesh(tempGFF);
                currentlyShowing = Showing.Model;
                meshes = mm.toModel().meshes;
                setButtons(true);
                if (meshes.Length > 0)
                {
                    setMeshNum(0);
                }
            }
            else if (extention == ".tga")
            {
                texture = new Targa(filePath);
                currentlyShowing = Showing.Texture;
            }
            else if (extention == ".lvl")
            {
                level = new Level(filePath);
                currentlyShowing = Showing.Level;
                level.readObjects();
            }
            //If its not the right type of file then print an error
            else
            {
                lbl_progressStatus.Text = "This is not a valid model (.mmh or .msh), texture (.tga), or level (.lvl) file!";
            }
            refreshView();
        }

        //-- Setup for the gl context --//
        private void glControl1_Load(object sender, EventArgs e)
        {
            textureIndex = GL.GenTexture();
            bitmap = new Bitmap(glControl1.Width, glControl1.Height);
            updateBitmap("Nothing to see here.");
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            redrawText();
            glControl1.SwapBuffers();
        }
    }
}
