using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Drawing;

using OpenTK;

namespace Ben
{
    struct Pixel
    {
        public byte r, g, b, a;

        public Pixel(byte blue, byte green, byte red)
            : this(blue, green, red, 255)
        { }

        public Pixel(Vector3 v)
            : this((byte)v.X, (byte)v.Y, (byte)v.Z)
        { }

        public Pixel(Vector4 v)
            : this((byte)v.X, (byte)v.Y, (byte)v.Z, (byte)v.W)
        { }

        public Pixel(byte blue, byte green, byte red, byte alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }
    }

    class Targa
    {
        #region types
        private const byte noData = 0;
        private const byte uncompressedMapped = 1;
        private const byte uncompressedRGB = 2;
        private const byte uncompressedBW = 0;
        private const byte runlengthMapped = 9;
        private const byte runlengthRGB = 10;
        private const byte compressedBW = 11;
        private const byte compressedMapped = 32;
        private const byte compressedMapped4Pass = 33;
        #endregion

        byte compression = uncompressedRGB;
        short xOrigin = 0;
        short yOrigin = 0;
        public short width
        {
            get;
            set;
        }
        public short height
        {
            get;
            set;
        }
        byte bitsPerPixel = 24;

        Pixel[] pixels;


        String filename { get; set; }

        public Pixel this[int i, int j]
        {
            get { return pixels[j * width + i]; }
            set { pixels[j * width + i] = value; }
        }

        public Targa(String file)
        {
            filename = file;
            readFromFile();
        }
        public Targa(String file, short w, short h)
        {
            filename = file;
            pixels = new Pixel[w * h];
            width = w;
            height = h;
        }
        public Targa(Pixel[] pix, short w, short h, byte bpp, String file)
        {
            pixels = pix;
            width = w;
            height = h;
            bitsPerPixel = bpp;
            filename = file;
        }
        public Targa(Pixel[] pix, short w, short h, byte bpp)
            : this(pix, w, h, bpp, "")
        { }


        public void readFromFile()
        {
            FileStream fs;
            BinaryReader file;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                file = new BinaryReader(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file " + filename + ": " + e.Message);
                return;
            }

            file.ReadByte(); //idlength
            file.ReadByte(); //coloumaptype

            compression = file.ReadByte();

            file.ReadInt16(); //colourmaporigin
            file.ReadInt16(); //colourmaplength
            file.ReadByte();  //colourmapdepth

            xOrigin = file.ReadInt16();
            yOrigin = file.ReadInt16();
            width = file.ReadInt16();
            height = file.ReadInt16();
            bitsPerPixel = file.ReadByte();

            file.ReadByte(); //imagedescriptor

            pixels = new Pixel[width * height];

            switch (compression)
            {
                case uncompressedRGB:
                    readUncompressedRGB(file);
                    break;
                default:
                    Console.WriteLine("This targa format is not currently supported.");
                    break;
            }
        }

        private void readUncompressedRGB(BinaryReader file)
        {
            switch (bitsPerPixel)
            {
                case 24:
                    for (int i = 0; i < width * height; i++)
                    {
                        pixels[i] = new Pixel(file.ReadByte(), file.ReadByte(), file.ReadByte());
                    }
                    break;
                case 32:
                    for (int i = 0; i < width * height; i++)
                    {
                        pixels[i] = new Pixel(file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte());
                    }
                    break;
                default:
                    Console.WriteLine("Only 24 or 32 bits per pixel is supported, " + filename + " has " + bitsPerPixel + ".");
                    break;
            }
        }

        public void writeToFile()
        {
            byte zero = 0;
            short szero = 0;
            FileStream fs;
            BinaryWriter file;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                file = new BinaryWriter(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writting file " + filename + ": " + e.Message);
                return;
            }

            file.Write(zero); //idlength
            file.Write(zero); //colourmaptype

            file.Write(compression);

            file.Write(szero); //colourmaporigin
            file.Write(szero); //colourmaplength
            file.Write(zero);  //colourmapdepth

            file.Write(xOrigin);
            file.Write(yOrigin);
            file.Write(width);
            file.Write(height);
            file.Write(bitsPerPixel);

            file.Write(zero); //imagedescriptor

            switch (compression)
            {
                case uncompressedRGB:
                    writeUncompressedRGB(file);
                    break;
                default:
                    Console.WriteLine("This targa format is not currently supported.");
                    break;
            }
            file.Close();
            fs.Close();
        }

        public Bitmap getTextureData()
        {
            Bitmap b = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = height - 1; j > -1; j--)
                {
                    
                    b.SetPixel(i, height - j - 1, Color.FromArgb(this[i,j].a, this[i, j].r, this[i, j].g, this[i, j].b));
                }
            }
            return b;
        }

        private void writeUncompressedRGB(BinaryWriter file)
        {
            switch(bitsPerPixel)
            {
                case 24:
                    for (int i = 0; i < width * height; i++)
                    {
                        file.Write(pixels[i].b);
                        file.Write(pixels[i].g);
                        file.Write(pixels[i].r);
                    }
                    break;
                case 32:
                    for (int i = 0; i < width * height; i++)
                    {
                        file.Write(pixels[i].b);
                        file.Write(pixels[i].g);
                        file.Write(pixels[i].r);
                        file.Write(pixels[i].a);
                    }
                    break;
                default:
                    Console.WriteLine("Unable to write targa: only 24 or 32 bits per pixel is supported, this one has {0}.",bitsPerPixel);
                    break;
            }
        }
    }
}
