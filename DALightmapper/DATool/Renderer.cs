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

        bool updateBuffers;

        List<VBO> nextVertexBufferObjects;
        Dictionary<String, int> nextTextureBuffers;
        float nextRotationAngle;
        
        int shaderProgram;

        readonly String vertexShaderPath = System.AppDomain.CurrentDomain.BaseDirectory + "//Shaders//shader.vert";
        readonly String fragmentShaderPath = System.AppDomain.CurrentDomain.BaseDirectory + "//Shaders//shader.frag";
        
        public delegate void glControlThreadSwitcher(OpenTK.Platform.IWindowInfo window);

        public Renderer(GLControl c)
        {
            nextRotationAngle = 0;
            nextVertexBufferObjects = new List<VBO>();
            nextTextureBuffers = new Dictionary<String, int>();

            renderThread = null;
            
            camera = new Camera();
            frameRate = 60;

            uploadModel(new ModelMesh());

            control = c;
            camera.translate(new Vector3(0, 0, 50));
            camera.rotateUp(-(float)(Math.PI / 2));
            shaderProgram = 0;

            initializeControl();
        }

        public void displayDDS(DDS d) {

            nextRotationAngle = 0;
            int vb, eb;
            GL.GenBuffers(1, out vb);
            GL.GenBuffers(1, out eb);
            int tb = uploadDDS(d);
            nextTextureBuffers.Add(d.formatString, tb);

                                    //Position     |     Normal      | Texture
            float[] verts = {   00.0f, 00.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                                10.0f, 00.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                                10.0f, 10.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                                00.0f, 10.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f
                            };
            uint[] indices = {  0, 2, 1,
                                0, 3, 2
                            };
            VBO quad = new VBO(vb, eb, tb);
            nextVertexBufferObjects.Add(quad);

            GL.BindBuffer(BufferTarget.ArrayBuffer, quad.vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quad.elementBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * sizeof(float)), verts, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            updateBuffers = true;
        }
        public void displayTarga(Targa t) { }
        public void displayModel(ModelMesh m)
        {
            uploadModel(m);
            nextRotationAngle = (float)(Math.PI / 180);
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
            
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2), (float)control.Width / (float)control.Height, 1, 1000);
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 camMatrix = camera.matrix;
            Matrix4 otherMatrix = Matrix4.LookAt(new Vector3(0, 0, 50), new Vector3(), new Vector3(1, 0, 0));
            GL.LoadMatrix(ref camMatrix);

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
            Dictionary<String, int> textureBuffers = new Dictionary<String,int>();
            float rotationAngle = 0;


            while (renderState == DisplayState.Run)
            {
                DateTime start = new DateTime();

                camera.rotateRight(rotationAngle);
                Matrix4 camMatrix = camera.matrix;
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref camMatrix);

                if (updateBuffers)
                {
                    rotationAngle = nextRotationAngle;

                    //Free the memory of the old buffers
                    clearBuffers(vertexBufferObjects, textureBuffers);

                    //Switch the buffers
                    vertexBufferObjects = nextVertexBufferObjects;
                    textureBuffers = nextTextureBuffers;

                    nextVertexBufferObjects = new List<VBO>();
                    nextTextureBuffers = new Dictionary<String, int>();

                    updateBuffers = false;
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
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.Uniform1(GL.GetUniformLocation(shaderProgram, "diffuseTexture"), 0);
                    GL.BindTexture(TextureTarget.Texture2D, vbo.textureBuffer);
                    GL.BindBuffer(BufferTarget.TextureBuffer, vbo.textureBuffer);

                    //GL.GetUniformLocation(shaderProgram, "diffuseTexture");

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

        private void uploadModel(ModelMesh model)
        {
            //Make VBOs of each chunk
            foreach (MeshChunk m in model.chunks)
            {
                int vb, eb, tb = 0;
                GL.GenBuffers(1, out vb);
                GL.GenBuffers(1, out eb);
                //find texture buffer
                if (m.materialObjectName != "")
                {
                    MaterialObject mao = IO.findFile<MaterialObject>(m.materialObjectName);
                    if (mao != null)
                    {
                        if (mao.textures.ContainsKey(TextureType.Diffuse))
                        {
                            //if we havent already added it to the dictionary
                            if (!nextTextureBuffers.ContainsKey(mao.textures[TextureType.Diffuse]))
                            {
                                int buffer = 0;
                                //find the file and then make a buffer out of it then add it to the dictionary
                                DDS dds = IO.findFile<DDS>(mao.textures[TextureType.Diffuse]);
                                if (dds != null)
                                {
                                    buffer = uploadDDS(dds);
                                }
                                else
                                {
                                    Console.WriteLine("Couldn't find texture {0}", mao.textures[TextureType.Diffuse]);
                                }

                                nextTextureBuffers.Add(mao.textures[TextureType.Diffuse], buffer);
                            }
                            tb = nextTextureBuffers[mao.textures[TextureType.Diffuse]];
                        }
                        else
                        {
                            Console.WriteLine("Material object {0} doesn't have a diffuse texture", m.materialObjectName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find material object {0}", m.materialObjectName);
                    }
                }

                VBO curVBO = new VBO(vb, eb, tb);
                nextVertexBufferObjects.Add(curVBO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, curVBO.vertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, curVBO.elementBuffer);

                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m.verts.Length * sizeof(float)), m.verts, BufferUsageHint.StaticDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(m.indices.Length * sizeof(uint)), m.indices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }
        }  
        private int uploadDDS(DDS dds)
        {
            int buffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, buffer);
            //GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)dds.size, dds.data, BufferUsageHint.StaticDraw);
            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, dds.format, dds.width, dds.height, 0, dds.data.Length, dds.data);
            for (int i = 0; i < dds.mipmapCount; i++)
            {
                GL.CompressedTexImage2D(TextureTarget.Texture2D, i + 1, dds.format, dds.mipmapWidth(i), dds.mipmapHeight(i), 0, dds.mipmaps[i].Length, dds.mipmaps[i]);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return buffer;
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
                control.Invoke(new glControlThreadSwitcher(control.Context.MakeCurrent), new object[] {null});
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
