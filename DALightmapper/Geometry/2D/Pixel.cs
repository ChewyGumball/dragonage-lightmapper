using System;
using System.Diagnostics;
using OpenTK;

namespace Geometry
{
    [DebuggerDisplay("Colour = {r},{g},{b},({a})")]
    public class Pixel
    {
        public byte r, g, b, a;

        public Pixel(byte blue, byte green, byte red)
            : this(blue, green, red, 0)
        { }

        public Pixel(Vector3 v)
            : this((byte)Math.Min(255f,v.Z), (byte)Math.Min(255f,v.Y), (byte)Math.Min(255f,v.X))
        { }

        public Pixel(Vector4 v)
            : this((byte)Math.Min(255f,v.Z), (byte)Math.Min(255f,v.Y), (byte)Math.Min(255f,v.X), (byte)Math.Min(255f,v.W))
        { }

        public Pixel(byte blue, byte green, byte red, byte alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }

        public static Pixel operator +(Pixel a, Pixel b)
        {
            int newr = Math.Min(255, Math.Max(a.r + b.r, 0));
            int newg = Math.Min(255, Math.Max(a.g + b.g, 0));
            int newb = Math.Min(255, Math.Max(a.b + b.b, 0));
            int newa = Math.Min(255, Math.Max(a.a + b.a, 0));

            return new Pixel((byte)newb, (byte)newg, (byte)newr, (byte)newa);
        }

        public static Pixel operator *(Pixel a, int b)
        {
            return b * a;
        }

        public static Pixel operator *(int a, Pixel b)
        {
            int newr = Math.Min(255, Math.Max(a * b.r, 0));
            int newg = Math.Min(255, Math.Max(a * b.g, 0));
            int newb = Math.Min(255, Math.Max(a * b.b, 0));
            int newa = Math.Min(255, Math.Max(a * b.a, 0));

            return new Pixel((byte)newb, (byte)newg, (byte)newr, (byte)newa);
        }

        public static Pixel operator /(Pixel a, int b)
        {
            return b / a;
        }

        public static Pixel operator /(int a, Pixel b)
        {
            int newr = Math.Min(255, Math.Max(b.r / a, 0));
            int newg = Math.Min(255, Math.Max(b.g / a, 0));
            int newb = Math.Min(255, Math.Max(b.b / a, 0));
            int newa = Math.Min(255, Math.Max(b.a / a, 0));

            return new Pixel((byte)newb, (byte)newg, (byte)newr, (byte)newa);
        }
    }

}
