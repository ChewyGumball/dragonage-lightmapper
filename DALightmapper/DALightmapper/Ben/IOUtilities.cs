using System;
using System.Text;
using System.IO;

namespace Ben
{
    public class IOUtilities
    {
        //Used to abbreviate file sizes
        private static readonly float kilobyte = 1024;
        private static readonly float megabyte = 1024 * kilobyte;
        private static readonly float gigabyte = 1024 * megabyte;
        private static readonly float terabyte = 1024 * gigabyte;

        //----------------------------------------------------------------------//
        //  Reads in a string headed with the length in characters //
        //----------------------------------------------------------------------//
        public static String readECString(BinaryReader file, long offset)
        {
            //Seek to the string
            file.BaseStream.Seek(offset, SeekOrigin.Begin);
            return readECString(file);
        }
        public static String readECString(BinaryReader file)
        {
            //The length is specified in # of chars, not # of bytes
            //  There are 2 bytes per char so multiply by 2
            int length = file.ReadInt32() * 2;
            return readECStringWithLength(file,length);
        }

        public static String readECStringWithLength(BinaryReader file, int length)
        {
            return Encoding.Unicode.GetString(file.ReadBytes(length)).Trim('\0').Trim();
        }

        //--------------------------------------------------------------------//
        //  Converts 2 bytes representing a big endian 16bit float into a 32  //
        //                          bit float                                 //
        //--------------------------------------------------------------------//
        public static float readFloat16(BinaryReader file)
        {
            byte first = file.ReadByte();
            byte second = file.ReadByte();
            //little endian so gotta swap bytes
            return toFloat16(second, first);
        }
        public static float toFloat16(byte first, byte second)
        {
            int result = 0;
            int exponent;
            //Get the sign bit
            result |= first & 128;
            //Make room for the exponent
            result = result << 1;
            //Get the exponent
            exponent = (first & 127) >> 2;
            //Get it to excess 127 from excess 15
            exponent += 112;
            //Or the exponent into the result
            result |= exponent;
            //Make room for the significand
            result = result << 2;
            //Get the last 2 bits of the first byte (significand portion of first byte)
            result |= 3 & first;
            //Make room for the second byte
            result = result << 8;
            //Or it in
            result |= second;
            //move everything into the right position
            result = result << 13;

            return BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
        }

        //--------------------------------------------------------------------//
        //  Convert numbers to strings representing the number's bit pattern  //
        //--------------------------------------------------------------------//
        public static String toBits(int number)
        {
            String bits = "";
            uint mask = 0x80000000;
            for (int i = 0; i < 32; i++, mask = mask >> 1)
            {
                if ((number & mask) != 0)
                    bits += "1";
                else
                    bits += "0";
            }
            return bits;
        }
        public static String toBits(short number)
        {
            String bits = "";
            uint mask = 0x8000;
            for (int i = 0; i < 16; i++, mask = mask >> 1)
            {
                if ((number & mask) != 0)
                    bits += "1";
                else
                    bits += "0";
            }
            return bits;
        }
        public static String toBits(float number)
        {
            String bits = "";
            uint mask;
            Byte[] bytes = BitConverter.GetBytes(number);
            for (int i = 0; i < bytes.Length; i++)
            {
                mask = 0x80;
                for (int j = 0; j < 8; j++, mask = mask >> 1)
                {
                    if ((bytes[i] & mask) != 0)
                        bits += "1";
                    else
                        bits += "0";
                }
            }
            return bits;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        //Abreviates filesizes
        public static string ToByteString(long bytes)
        {
            if (bytes > terabyte) return (bytes / terabyte).ToString("0.00 TB");
            else if (bytes > gigabyte) return (bytes / gigabyte).ToString("0.00 GB");
            else if (bytes > megabyte) return (bytes / megabyte).ToString("0.00 MB");
            else if (bytes > kilobyte) return (bytes / kilobyte).ToString("0.00 KB");
            else return bytes + " Bytes";
        }
    }
}
