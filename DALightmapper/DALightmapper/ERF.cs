using System;
using System.Threading;
using System.Text;
using System.IO;

using Ben;
using DALightmapper;

namespace Bioware.Files
{
    class ERF : BiowareFile
    {
        uint _resourceCount;        //# of files in the erf
        String[] _resourceNames;    //The names of those files
        uint[] _resourceOffsets;    //The offsets to the beginning of those files
        uint[] _resourceLengths;    //The lengths of those files

        String _type;               //The type of erf from the header
        String _version;            //The version of erf from the header

        public String[] resourceNames
        {
            get { return _resourceNames; }
        }
        public uint[] resourceOffsets
        {
            get { return _resourceOffsets; }
        }
        public uint[] resourceLengths
        {
            get { return _resourceLengths; }
        }
        public uint resourceCount
        {
            get { return _resourceCount; }
        }

        public ERF(String filename)
            : base(filename)
        {
            OpenR();
            //Get the type, version and file count
            _type = IOUtilities.readECStringWithLength(file, 8);
            _version = IOUtilities.readECStringWithLength(file, 8);
            _resourceCount = file.ReadUInt16();

            //Make arrays for resource data
            _resourceNames = new String[_resourceCount];
            _resourceLengths = new uint[_resourceCount];
            _resourceOffsets = new uint[_resourceCount];
        }

        //Actuall reads in the table of contents
        public void readKeyData()
        {
            OpenR();
            //Seek to the beginning of the data portion
            file.BaseStream.Seek(32, SeekOrigin.Begin);
            //Read in resource data
            for (int i = 0; i < _resourceCount && !IO.abort; i++)
            {
                //Read in file name
                _resourceNames[i] = IOUtilities.readECStringWithLength(file, 64);
                _resourceOffsets[i] = file.ReadUInt32();
                _resourceLengths[i] = file.ReadUInt32();
            }
        }

        public int indexOf(String fileName)
        {
            return Array.BinarySearch(_resourceNames, fileName);
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
            int index = Array.BinarySearch(_resourceNames, fileName);
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
                    OpenR();
                    file.BaseStream.Seek(_resourceOffsets[index], SeekOrigin.Begin);
                    byte[] bytes = file.ReadBytes((int)_resourceLengths[index]);
                    BinaryWriter outFile = new BinaryWriter(new FileStream((directory + "\\" + fileName), FileMode.Create));
                    outFile.Write(bytes);
                    outFile.Flush();
                    outFile.Close();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
