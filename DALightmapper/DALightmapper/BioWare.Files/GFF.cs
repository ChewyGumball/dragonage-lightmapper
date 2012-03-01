using System;
using System.IO;
using Bioware.Structs;
using DALightmapper;
using Ben;

namespace Bioware.Files
{
    public class GFF : FindableFile
    {
        private long beginningOffset;

        public String path { get; private set; }
        public GFFHeader header { get; private set; }
        public BiowareStruct[] structs { get; private set; }

        public long dataOffset { get; private set; }

        public GFF() { }

        //Reads a GFF file from the file with path filename starting at offset
        public GFF(String filename, long offset)
        {
            createFromPathWithOffset(filename, offset, 0);
        }

        public GFF(String filename) : this(filename, 0) { }


        public void createFromPathWithOffset(String filename, long offset, int length)
        {
            path = filename;
            //Save the filename for opening later
            beginningOffset = offset;

            BinaryReader file = openReader();
            //Get the header of the gff
            header = new GFFHeader(file);
            dataOffset = header.dataOffset + beginningOffset;

            //Get the struct definitions
            structs = new BiowareStruct[header.structCount];

            for (int i = 0; i < header.structCount; i++)
            {
                structs[i] = new BiowareStruct(this, new GFFStructDefinition(file, beginningOffset));
            }
            file.Close();
        }
        public void createFromPath(String filename)
        {
            createFromPathWithOffset(filename, 0, 0);
        }

        public BinaryReader openReader()
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
