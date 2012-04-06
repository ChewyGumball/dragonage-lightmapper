using System;
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
        List<UploadableObject> nextModels;
        List<OverlayUploader> nextOverlays;

        Matrix4 perspective;
        Matrix4 orthographic;

        bool rotateModel;
        bool updateCamera;
        bool updateBuffers;
        bool clearDisplayList;
        bool overlay;
        bool updateOverlayList;
        bool clearOverlayList;

        Matrix4 nextProjectionMatrix;

        int shaderProgram;
        float rotationAngle;

        int textTextureIndex;

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
            orthographic = Matrix4.CreateOrthographicOffCenter(0, 2, 0, 2, -1, 1);

            rotateModel = false;
            updateBuffers = false;
            updateCamera = false;
            clearDisplayList = false;

            overlay = false;
            updateOverlayList = false;
            clearOverlayList = false;
            
            nextProjectionMatrix = Matrix4.Identity;

            textTextureIndex = 0;
            shaderProgram = 0;
            rotationAngle = (float)(Math.PI / 180);

            nextModels = new List<UploadableObject>();
            nextOverlays = new List<OverlayUploader>();

            initializeControl();
        }

        public void overlayText(String s)
        {
            lock (nextOverlays)
            {
                nextOverlays.Add(new TextUploader(s, control.Width, control.Height));
            }
            updateOverlayList = true;
        }

        public void showOverlays() { overlay = true; }
        public void hideOverlays() { overlay = false; }
        public void clearOverlays() 
        {
            lock (nextOverlays)
            {
                nextOverlays.Clear();
            }
            clearOverlayList = true; 
        }

        public void displayDDS(DDS d)
        {
            lock (nextModels)
            {
                nextModels.Clear();
                nextModels.Add(new DDSUploader(d));
            }
            rotateModel = false;
            nextProjectionMatrix = orthographic;
            updateCamera = false;
            clearDisplayList = true;
            updateBuffers = true;
        }
        public void displayTarga(Targa t) { }
        public void displayModel(ModelMesh m)
        {
            lock (nextModels)
            {
                nextModels.Clear();
                nextModels.Add(new ModelUploader(m));
            }
            rotateModel = true;
            nextProjectionMatrix = perspective;
            updateCamera = true;
            clearDisplayList = true;
            updateBuffers = true;
        }
        public void clearDisplay() { clearDisplayList = true;}

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

            GL.ClearColor(Color.Blue);
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

            List<VBO> overlayObjects = new List<VBO>();
            Dictionary<String, int> overlayTextureBuffers = new Dictionary<String, int>();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Matrix4 modelTransform = Matrix4.Identity;

            while (renderState == DisplayState.Run)
            {
                DateTime start = new DateTime();
                if (clearDisplayList)
                {
                    //Free the memory of the old buffers
                    clearBuffers(vertexBufferObjects, textureBuffers);

                    vertexBufferObjects = new List<VBO>();
                    textureBuffers = new Dictionary<String, int>();
                    clearDisplayList = false;
                }

                if (clearOverlayList)
                {
                    clearBuffers(overlayObjects, overlayTextureBuffers);
                    overlayObjects = new List<VBO>();
                    overlayTextureBuffers = new Dictionary<String, int>();
                    clearOverlayList = false;
                }

                if (updateBuffers)
                {
                    //Update the buffers
                    uploadBuffers(ref vertexBufferObjects, ref textureBuffers, nextModels);
                    nextModels.Clear();

                    //Set up the projection matrix
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref nextProjectionMatrix);

                    //reset the model transform
                    modelTransform = Matrix4.Identity;

                    updateBuffers = false;
                }

                if (updateOverlayList)
                {
                    uploadBuffers(ref overlayObjects, ref overlayTextureBuffers, nextOverlays);
                    nextOverlays.Clear();

                    updateOverlayList = false;
                }

                if (updateCamera)
                {
                    //If we are rotating the model, update the model transform
                    if (rotateModel)
                    {
                        modelTransform *= Matrix4.CreateRotationZ(-rotationAngle);
                    }

                    //Add in the camera transform
                    Matrix4 camMatrix = modelTransform;
                    lock (camera)
                    {
                        camMatrix = camMatrix * camera.matrix;
                    }

                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref camMatrix);
                }

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(shaderProgram);

                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.EnableClientState(ArrayCap.TextureCoordArray);

                //Draw each model
                foreach (VBO vbo in vertexBufferObjects)
                {
                    drawVBO(vbo);
                }

                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.DisableClientState(ArrayCap.NormalArray);
                GL.DisableClientState(ArrayCap.VertexArray);

                if (overlay)
                {
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadMatrix(ref orthographic);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PushMatrix();
                    GL.LoadIdentity();

                    GL.UseProgram(shaderProgram);

                    GL.EnableClientState(ArrayCap.VertexArray);
                    GL.EnableClientState(ArrayCap.NormalArray);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);

                    foreach (VBO vbo in overlayObjects)
                    {
                        drawVBO(vbo);
                    }

                    GL.DisableClientState(ArrayCap.TextureCoordArray);
                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.VertexArray);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PopMatrix();

                }

                control.SwapBuffers();

                //Sleep till next frame start
                double millisecondsPerFrame = 1000 / frameRate;
                double timeTaken = (DateTime.Now - start).TotalMilliseconds % millisecondsPerFrame;
                Thread.Sleep((int)(millisecondsPerFrame - timeTaken));
            }

            if (renderState == DisplayState.Stop)
            {
                clearBuffers(vertexBufferObjects, textureBuffers);
                clearBuffers(overlayObjects, overlayTextureBuffers);
            }
        }

        private void drawVBO(VBO vbo)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.elementBuffer);
            GL.BindTexture(TextureTarget.Texture2D, vbo.textureBuffer);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "diffuseTexture"), 0);

            GL.VertexPointer(3, VertexPointerType.Float, vbo.vertexSize * sizeof(float), 0);
            GL.NormalPointer(NormalPointerType.Float, vbo.vertexSize * sizeof(float), 3 * sizeof(float));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, vbo.vertexSize * sizeof(float), 6 * sizeof(float));

            GL.DrawElements(BeginMode.Triangles, vbo.indexElementCount, DrawElementsType.UnsignedInt, 0);
        }
        private void uploadBuffers<T>(ref List<VBO> vertexBufferObjects, ref Dictionary<String, int> textureBuffers, List<T> uploads) where T : UploadableObject
        {
            lock (uploads)
            {
                foreach (UploadableObject obj in uploads)
                {
                    //Upload the next model and its textures
                    obj.upload(ref vertexBufferObjects, ref textureBuffers);
                }
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
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }
    }
}
