using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace DATool
{
    class DDSUploader : UploadableObject
    {
        DDS texture;

        public DDSUploader() { texture = new DDS(); }
        public DDSUploader(DDS file) { texture = file; }

        public void upload(ref List<VBO> vboList, ref Dictionary<String, int> textureList)
        {
            int vb, eb;
            GL.GenBuffers(1, out vb);
            GL.GenBuffers(1, out eb);

            int buffer = uploadDDS(texture);
            textureList.Add(texture.formatString, buffer);

            //                      Position     |     Normal      | Texture
            float[] verts = {   0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                                1.9f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                                1.9f, 1.9f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                                0.0f, 1.9f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f
                            };
            uint[] indices = {  
                                0, 1, 2,
                                0, 2, 3
                             };

            VBO quad = new VBO(vb, eb, buffer);
            vboList.Add(quad);

            GL.BindBuffer(BufferTarget.ArrayBuffer, quad.vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quad.elementBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * sizeof(float)), verts, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        public static int uploadDDS(DDS d)
        {
            int buffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, buffer);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Decal);

            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, d.format, d.width, d.height, 0, d.data.Length, d.data);
            /*
            for (int i = 0; i < texture.mipmapCount; i++)
            {
                GL.CompressedTexImage2D(TextureTarget.Texture2D, i + 1, d.format, d.mipmapWidth(i), d.mipmapHeight(i), 0, d.mipmaps[i].Length, d.mipmaps[i]);
            }
            */
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return buffer;
        }
    }
}
