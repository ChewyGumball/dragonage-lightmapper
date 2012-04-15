using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using OpenTK.Graphics.OpenGL;

using Bioware.IO;

namespace DATool
{
    class DDS : FindableFile
    {
        #region PIXEL FORMAT FLAGS

        private readonly static uint ALPHAPIXELS_FLAG = 0x1;
        private readonly static uint ALPHA_FLAG = 0x2;
        private readonly static uint FOURCC_FLAG = 0x4;
        private readonly static uint RGB_FLAG = 0x40;
        private readonly static uint YUV_FLAG = 0x200;
        private readonly static uint LUMINANCE_FLAG = 0x20000;

        #endregion

        #region HEADER FLAGS

        private readonly static uint MIPMAPS = 0x20000;

        #endregion

        private long beginningOffset;

        public String path { get; private set; }

        public byte[] data { get; private set; }

        public int width { get; private set; }
        public int height { get; private set; }

        public int mipmapCount { get; private set; }
        public byte[][] mipmaps { get; private set; }

        public String formatString { get; private set; }
        public PixelInternalFormat format { get; private set; }

        public int mipmapWidth(int i)
        {
            return width / (int)Math.Pow(2, i + 1); ;
        }
        public int mipmapHeight(int i)
        {
            return height / (int)Math.Pow(2, i + 1); 
        }

        public DDS()
        {
            path = "";
            data = new byte[0];
            width = 0;
            height = 0;
            mipmapCount = 0;
            mipmaps = new byte[0][];
        }

        public DDS(String filename, long offset, int length) { createFromPathWithOffset(filename, offset, length); }
        public DDS(String filename) : this(filename, 0, -1) { }

        public void createFromPathWithOffset(String filename, long offset, int length)
        {
            path = filename;
            beginningOffset = offset;

            BinaryReader file = openReader();

            //format:
            //  magic number
            //  header
            //  data

            //Magic number, should be "DDS"
            String magicNumber = new String(file.ReadChars(4));

            //Header
            uint headerSize = file.ReadUInt32();
            uint flags = file.ReadUInt32();
            height = (int)file.ReadUInt32();
            width = (int)file.ReadUInt32();
            uint pitchOrLinearSize = file.ReadUInt32();
            uint depth = file.ReadUInt32();

            //Number of mipmaps
            mipmapCount = (int)file.ReadUInt32();
            if ((flags & MIPMAPS) == 0)
            {
                mipmapCount = 0;
            }

            mipmaps = new byte[mipmapCount][];

            //unused 11 uints worth of space in the header
            file.BaseStream.Seek(11 * 4, SeekOrigin.Current);

            uint pixelFormatSize = file.ReadUInt32();
            uint pixelFormatFlags = file.ReadUInt32();

            //The texture is not compressed if the FOURCC is not set
            if ((pixelFormatFlags & FOURCC_FLAG) == 0)
            {
                throw new NotImplementedException("Uncompressed textures have not been implemented!");
            }

            //Format
            formatString = new String(file.ReadChars(4));
            switch (formatString)
            {
                case "DXT1": format = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext; break;
                case "DXT3": format = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext; break;
                case "DXT5": format = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext; break;
                default: throw new NotImplementedException(formatString + " has not been implemented!"); 
            }

            //Uncompressed format values
            uint bitCount = file.ReadUInt32();
            uint redMask = file.ReadUInt32();
            uint greenMask = file.ReadUInt32();
            uint blueMask = file.ReadUInt32();
            uint alphaMask = file.ReadUInt32();

            //Useless values
            uint caps = file.ReadUInt32();
            uint caps2 = file.ReadUInt32();
            uint caps3 = file.ReadUInt32();
            uint caps4 = file.ReadUInt32();
            uint reserved = file.ReadUInt32();

            //All DXT formats except DXT1 have block size 16
            int blockSize = 16;
            if (format == PixelInternalFormat.CompressedRgbaS3tcDxt1Ext)
            {
                blockSize = 8;
            }

            //Read in base level texture
            int size = Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * blockSize;
            data = file.ReadBytes(size);

            //Read in the mipmaps
            for (int i = 0; i < mipmapCount; i++)
            {
                int mipmapSize = Math.Max(1, (mipmapWidth(i) + 3) / 4) * Math.Max(1, (mipmapHeight(i) + 3) / 4) * blockSize;
                mipmaps[i] = file.ReadBytes(mipmapSize);
            }

            file.Close();
        }
        public void createFromPath(String filename) { createFromPathWithOffset(filename, 0, -1); }

        private BinaryReader openReader()
        {
            try
            {
                BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.BaseStream.Seek(beginningOffset, SeekOrigin.Begin);
                return file;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    Console.WriteLine("{0} \n Inner: {1}.", e.StackTrace, e.InnerException.Message);
                else
                    Console.WriteLine("{0}", e.StackTrace);

                throw e;
            }
        }
    }
}
