using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace DATool
{
    class DDSUploader : OverlayUploader
    {
        private DDS texture;

        public DDSUploader() { texture = new DDS(); }
        public DDSUploader(DDS file) { texture = file; }

        public override int uploadTexture()
        {
            int buffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, buffer);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);

            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, texture.format, texture.width, texture.height, 0, texture.data.Length, texture.data);
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
