using System;
using System.Threading;
using System.Text;
using System.IO;

using Bioware.IO;

namespace Bioware.Files
{
    public class ERF
    {
        String _type;               //The type of erf from the header
        String _version;            //The version of erf from the header

        public String path { get; private set; }
        public String[] resourceNames { get; private set; }
        public uint[] resourceOffsets { get; private set; }
        public uint[] resourceLengths { get; private set; }
        public uint resourceCount { get; private set;}

        public ERF(String filename)
        {
            path = filename;
            BinaryReader file = openReader(); 
            //Get the type, version and file count
            _type = IOUtilities.readECStringWithLength(file, 8);
            _version = IOUtilities.readECStringWithLength(file, 8);
            resourceCount = file.ReadUInt16();

            //Make arrays for resource data
            resourceNames = new String[resourceCount];
            resourceLengths = new uint[resourceCount];
            resourceOffsets = new uint[resourceCount];

            //Seek to the beginning of the data portion
            file.BaseStream.Seek(32, SeekOrigin.Begin);
            //Read in resource data
            for (int i = 0; i < resourceCount; i++)
            {
                //Read in file name
                resourceNames[i] = IOUtilities.readECStringWithLength(file, 64).ToLower();
                resourceOffsets[i] = file.ReadUInt32();
                resourceLengths[i] = file.ReadUInt32();
            }

            file.Close();
        }

        public int indexOf(String fileName)
        { 
            return Array.IndexOf(resourceNames, fileName.ToLower());
        }

        //Tests to see if the input filename is present in this erf
        public Boolean isInERF(String fileName)
        {
            return indexOf(fileName) >= 0;
        }
        //Creates a copy of the file contained in this erf with input filename at the input directory, returns true of the copy was successful, false if not
        public Boolean separateFile(String fileName, String directory)
        {
            //Find the index of the file
            int index = indexOf(fileName);
            //If the file isn't in this erf print an error!
            if (index < 0)
            {
                return false;
            }
            else
            {
                //Try to separate it
                try
                {
                    BinaryReader file = openReader(); 
                    file.BaseStream.Seek(resourceOffsets[index], SeekOrigin.Begin);
                    byte[] bytes = file.ReadBytes((int)resourceLengths[index]);
                    BinaryWriter outFile = new BinaryWriter(new FileStream((directory + "\\" + fileName), FileMode.Create));
                    outFile.Write(bytes);
                    outFile.Flush();
                    outFile.Close();
                    file.Close();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private BinaryReader openReader()
        {
            try
            {
                return new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
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
