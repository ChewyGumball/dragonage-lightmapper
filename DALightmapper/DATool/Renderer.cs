﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using DALightmapper;
using Bioware.Files;
using Bioware.Structs;
using Ben;

namespace DATool
{
    enum DisplayState { Run, Stop };
    class Renderer
    {
        public Camera camera { get; private set; }
        public int frameRate { get; set; }

        GLControl control;
        Thread renderThread;
        DisplayState renderState = DisplayState.Stop;
        UploadableObject nextUpload;
        Matrix4 perspective;
        Matrix4 orthographic;

        bool rotateCamera;
        bool updateBuffers;

        List<VBO> nextVertexBufferObjects;
        Dictionary<String, int> nextTextureBuffers;
        Matrix4 nextProjectionMatrix;
        Matrix4 nextModelViewMatrix;

        int shaderProgram;
        float rotationAngle;

        readonly String vertexShaderPath = System.AppDomain.CurrentDomain.BaseDirectory + "//Shaders//shader.vert";
        readonly String fragmentShaderPath = System.AppDomain.CurrentDomain.BaseDirectory + "//Shaders//shader.frag";

        public delegate void glControlThreadSwitcher(OpenTK.Platform.IWindowInfo window);

        public Renderer(GLControl c)
        {
            camera = new Camera();
            camera.translate(new Vector3(0, 0, 50));
            //camera.rotateUp(-(float)(Math.PI / 2));

            frameRate = 60;
            control = c;
            renderThread = null;

            perspective = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2), (float)control.Width / (float)control.Height, 1, 100);
            float min = Math.Min(control.Width, control.Height);
            orthographic = Matrix4.CreateOrthographicOffCenter(0, 2, 0, 2, -1, 1);

            rotateCamera = false;
            updateBuffers = false;

            nextVertexBufferObjects = new List<VBO>();
            nextTextureBuffers = new Dictionary<String, int>();
            nextProjectionMatrix = new Matrix4();
            nextModelViewMatrix = new Matrix4();

            shaderProgram = 0;
            rotationAngle = (float)(Math.PI / 180);

            nextUpload = new ModelUploader();

            initializeControl();
        }

        public void displayDDS(DDS d)
        {
            nextUpload = new DDSUploader(d);
            rotateCamera = false;
            nextProjectionMatrix = orthographic;
            nextModelViewMatrix = Matrix4.Identity;
            updateBuffers = true;
        }
        public void displayTarga(Targa t) { }
        public void displayModel(ModelMesh m)
        {
            nextUpload = new ModelUploader(m);
            rotateCamera = true;
            nextProjectionMatrix = perspective;
            nextModelViewMatrix = camera.matrix;
            updateBuffers = true;
        }

        public void start()
        {
            renderState = DisplayState.Run;
            renderThread = new Thread(startRender);
            renderThread.Start();
        }
        public void pause()
        {
            renderState = DisplayState.Stop;
            renderThread.Join();
        }
        public void resume()
        {
            renderState = DisplayState.Run;
            renderThread = new Thread(render);
            renderThread.Start();
        }
        public void stop()
        {
            renderState = DisplayState.Stop;
            renderThread.Join();
        }

        private void startRender()
        {
            makeCurrent();

            GL.ClearColor(Color.Black);
            StreamReader vertShader = new StreamReader(vertexShaderPath);
            StreamReader fragShader = new StreamReader(fragmentShaderPath);

            String vertShaderString = vertShader.ReadToEnd();
            String fragShaderString = fragShader.ReadToEnd();

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShader, vertShaderString);
            GL.ShaderSource(fragmentShader, fragShaderString);

            GL.CompileShader(vertexShader);
            Console.WriteLine(GL.GetShaderInfoLog(vertexShader));
            GL.CompileShader(fragmentShader);
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShader));

            shaderProgram = GL.CreateProgram();

            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.LinkProgram(shaderProgram);
            Console.WriteLine(GL.GetProgramInfoLog(shaderProgram));

            render();
        }

        private void render()
        {
            makeCurrent();

            List<VBO> vertexBufferObjects = new List<VBO>();
            Dictionary<String, int> textureBuffers = new Dictionary<String, int>();

            while (renderState == DisplayState.Run)
            {
                DateTime start = new DateTime();

                if (updateBuffers)
                {
                    Console.WriteLine("Updating buffers");
                    //Free the memory of the old buffers
                    clearBuffers(vertexBufferObjects, textureBuffers);

                    nextVertexBufferObjects = new List<VBO>();
                    nextTextureBuffers = new Dictionary<String, int>();
                    nextUpload.upload(ref nextVertexBufferObjects, ref nextTextureBuffers);

                    //Switch the buffers
                    vertexBufferObjects = nextVertexBufferObjects;
                    textureBuffers = nextTextureBuffers;

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref nextProjectionMatrix);

                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref nextModelViewMatrix);

                    updateBuffers = false;
                }

                if (rotateCamera)
                {
                    camera.rotateRight(rotationAngle);
                    Matrix4 camMatrix = camera.matrix;
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref camMatrix);
                }

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(shaderProgram);

                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.EnableClientState(ArrayCap.TextureCoordArray);

                foreach (VBO vbo in vertexBufferObjects)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.vertexBuffer);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.elementBuffer);

                    GL.Uniform1(GL.GetUniformLocation(shaderProgram, "diffuseTexture"), 0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, vbo.textureBuffer);


                    GL.VertexPointer(3, VertexPointerType.Float, vbo.vertexElementCount * sizeof(float), 0);
                    GL.NormalPointer(NormalPointerType.Float, vbo.vertexElementCount * sizeof(float), 3);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, vbo.vertexElementCount * sizeof(float), 6);

                    GL.DrawElements(BeginMode.Triangles, vbo.indexElementCount, DrawElementsType.UnsignedInt, 0);

                }

                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.DisableClientState(ArrayCap.NormalArray);
                GL.DisableClientState(ArrayCap.VertexArray);

                control.SwapBuffers();

                double millisecondsPerFrame = 1000 / frameRate;
                double timeTaken = (DateTime.Now - start).TotalMilliseconds % millisecondsPerFrame;
                Thread.Sleep((int)(millisecondsPerFrame - timeTaken));
            }

            if (renderState == DisplayState.Stop)
            {
                clearBuffers(vertexBufferObjects, textureBuffers);
            }
        }
        
        private void clearBuffers(List<VBO> vbos, Dictionary<String, int> textures)
        {
            //Clean up previous models memory
            foreach (VBO vbo in vbos)
            {
                int vb = vbo.vertexBuffer;
                int eb = vbo.elementBuffer;
                GL.DeleteBuffers(1, ref vb);
                GL.DeleteBuffers(1, ref eb);
            }

            //Clean up textures, they may be shared between chunks so we can't use the vbo textureBuffer value as there may be duplicates
            foreach (int texture in textures.Values)
            {
                int buffer = texture;
                GL.DeleteBuffers(1, ref buffer);
            }
        }
        private void makeCurrent()
        {
            if (!control.Context.IsCurrent)
            {
                control.Invoke(new glControlThreadSwitcher(control.Context.MakeCurrent), new object[] { null });
                control.MakeCurrent();
            }
        }
        private void initializeControl()
        {
            makeCurrent();

            int w = control.Width;
            int h = control.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }
    }
}