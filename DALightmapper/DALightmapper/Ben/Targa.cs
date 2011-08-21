using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using OpenTK;

namespace Ben
{
    struct Pixel
    {
        public byte r, g, b;

        public Pixel(byte blue, byte green, byte red)
        {
            r = red;
            g = green;
            b = blue;
        }

        public Pixel(Vector3 v)
        {
            r = (byte)v.X;
            g = (byte)v.Y;
            b = (byte)v.Z;
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

        byte compression;
        short xOrigin = 0;
        short yOrigin = 0;
        short width;
        short height;
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
            if (bitsPerPixel != 24)
            {
                Console.WriteLine("Only 24 bits per pixel is supported, " + filename + " has " + bitsPerPixel + ".");
            }
            else
            {
                for (int i = 0; i < width * height; i++)
                {
                    pixels[i] = new Pixel(file.ReadByte(), file.ReadByte(), file.ReadByte());
                }
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
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
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
        }

        private void writeUncompressedRGB(BinaryWriter file)
        {
            for (int i = 0; i < width * height; i++)
            {
                file.Write(pixels[i].b);
                file.Write(pixels[i].g);
                file.Write(pixels[i].r);
            }
        }
    }
}
