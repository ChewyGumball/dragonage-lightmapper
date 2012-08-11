using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Drawing;

using OpenTK;

namespace Geometry
{
    public class Targa
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


        public String filename { get; set; }

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
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    this[i, j] = new Pixel(0,0,0);
                }
            }
        }
        public Targa(String file, Pixel[] pix, short w, short h, byte bpp)
        {
            pixels = pix;
            width = w;
            height = h;
            bitsPerPixel = bpp;
            filename = file;
        }
        public Targa(Pixel[] pix, short w, short h, byte bpp)
            : this("UNNAMED PIXEL", pix, w, h, bpp)
        { }

        public void grow(int[,] filter)
        {
            int filterWidth = filter.GetLength(0);
            int filterHeight = filter.GetLength(1);

            int halfWidth = filterWidth / 2;
            int halfHeight = filterHeight / 2;

            Pixel[] newPixels = new Pixel[pixels.Length]; 
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Pixel current = this[x, y];

                    if (current.r < 10 && current.g < 10 && current.b < 10)
                    {
                        int weight = 0;
                        int newR = 0;
                        int newG = 0;
                        int newB = 0;

                        for (int filterX = 0; filterX < filterWidth; filterX++)
                        {
                            for (int filterY = 0; filterY < filterHeight; filterY++)
                            {
                                int curX = x - halfWidth + filterX;
                                int curY = y - halfHeight + filterY;
                                if ((curX >= 0 && curX < width) && (curY >= 0 && curY < height))
                                {
                                    Pixel next = this[curX, curY];
                                    if (next.a > 0 || next.g > 0 || next.b > 0)
                                    {
                                        newR += filter[filterX, filterY] * next.r;
                                        newG += filter[filterX, filterY] * next.g;
                                        newB += filter[filterX, filterY] * next.b;

                                        weight += filter[filterX, filterY];
                                    }
                                }
                            }
                        }
                        if (weight != 0)
                        {
                            newPixels[y * width + x] = new Pixel((byte)(newB / weight), (byte)(newG / weight), (byte)(newR / weight));
                        }
                        else
                        {
                            newPixels[y * width + x] = this[x, y];
                        }
                    }
                    else
                    {
                        newPixels[y * width + x] = this[x, y];
                    }
                }
            }

            pixels = newPixels;
        }

        public void applyFilter(int[,] filter)
        {
            int filterWidth = filter.GetLength(0);
            int filterHeight = filter.GetLength(1);

            int halfWidth = filterWidth / 2;
            int halfHeight = filterHeight / 2;

            Pixel[] newPixels = new Pixel[pixels.Length]; 

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int weight = 0;
                    int newR = 0;
                    int newG = 0;
                    int newB = 0;
                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int curX = x - halfWidth + filterX;
                            int curY = y - halfHeight + filterY;
                            if ((curX >= 0 && curX < width) && (curY >= 0 && curY < height))
                            {
                                newR += filter[filterX, filterY] * this[curX, curY].r;
                                newG += filter[filterX, filterY] * this[curX, curY].g;
                                newB += filter[filterX, filterY] * this[curX, curY].b;
                                weight += filter[filterX, filterY];
                            } 
                        }
                    }
                    if (weight != 0)
                    {
                        newPixels[y * width + x] = new Pixel((byte)(newB / weight), (byte)(newG / weight), (byte)(newR / weight));
                    }
                    else
                    {
                        newPixels[y * width + x] = this[x, y];
                    }
                }
            }

            pixels = newPixels;
        }

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
