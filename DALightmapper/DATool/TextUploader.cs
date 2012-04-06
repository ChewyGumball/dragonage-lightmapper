using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace DATool
{
    class TextUploader : OverlayUploader
    {
        private PointF origin;
        private String text;
        private int width;
        private int height;

        public TextUploader(String s, int w = 512, int h = 512) : this(s, new PointF(0, 0), w, h) { }

        public TextUploader(String s, PointF o, int w, int h)
        {
            text = s;
            width = w;
            height = h;
            origin = o;
        }

        public override int uploadTexture()
        {
            int buffer = GL.GenTexture();

            Bitmap bitmap = new Bitmap(width, height);
            Graphics gfx = Graphics.FromImage(bitmap);
            gfx.Clear(Color.Transparent);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            gfx.DrawString(text, new Font(FontFamily.GenericSansSerif, 11), Brushes.White, new RectangleF(new PointF(0, 0), new SizeF(width, height)));

            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, buffer);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            return buffer;
        }
    }
}
