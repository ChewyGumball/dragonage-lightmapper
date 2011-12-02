using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Ben
{
    class TextBitmap
    {
        String text;
        Bitmap bitmap;
        Graphics gfx;
        Matrix4 stuff = Matrix4.CreateOrthographic(300, 300, -4, 100);
        public int textureIndex;

        public TextBitmap(String s, int width, int height)
        {
            text = s;
            textureIndex = GL.GenTexture();
            bitmap = new Bitmap(width * 60, height * 60);
            gfx = Graphics.FromImage(bitmap);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            gfx.DrawString(text, new Font(FontFamily.GenericMonospace, 14), Brushes.White, new PointF(0, 0));

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width * 60, height * 60), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width * 60, height * 60, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        public void draw(int x, int y)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(-100, 100, -100, 100, 0, 100);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);
            GL.Begin(BeginMode.Quads);
	
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0, 0, 0);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(20,0, 0);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(20, 60, 0);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0, 60, 0);

            GL.End();
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
        }

    }
}
