using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Ben
{
    public class TextBitmap
    {
        public String text { get; private set; }
        Bitmap bitmap;
        Graphics gfx;
        Matrix4 stuff = Matrix4.CreateOrthographic(300, 300, -4, 100);
        public int textureIndex;

        public TextBitmap(String s, int width, int height)
        {
            text = s;
            textureIndex = GL.GenTexture();
            bitmap = new Bitmap(width, height);
            gfx = Graphics.FromImage(bitmap);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            gfx.Clear(Color.Transparent);
            gfx.DrawString(text, new Font(FontFamily.GenericMonospace, 14), Brushes.White, new PointF(0, 0));

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        public void draw()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, bitmap.Width, bitmap.Height, 0, -1, 1);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 1f); GL.Vertex2(0f, 0f);
            GL.TexCoord2(1f, 1f); GL.Vertex2(1f, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0f, 1f);
            GL.End();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

    }
}
