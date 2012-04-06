using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace DATool
{
    abstract class OverlayUploader : UploadableObject
    {
        public void upload(ref List<VBO> vboList, ref Dictionary<string, int> textureList)
        {
            int vb, eb;
            GL.GenBuffers(1, out vb);
            GL.GenBuffers(1, out eb);

            int buffer = uploadTexture();
            textureList.Add("Overlay " + buffer, buffer);

            //                      Position     |     Normal      | Texture
            float[] verts = {   0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
                                2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                                2.0f, 2.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                                0.0f, 2.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f
                            };
            uint[] indices = {  
                                0, 1, 2,
                                0, 2, 3
                             };

            VBO quad = new VBO(vb, eb, buffer, 8, indices.Length);
            vboList.Add(quad);

            GL.BindBuffer(BufferTarget.ArrayBuffer, quad.vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quad.elementBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * sizeof(float)), verts, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public abstract int uploadTexture();
    }
}
