using System;
using System.IO;
using Bioware.Structs;
using Ben;

namespace Bioware.Files
{
    class GFF : BiowareFile
    {
        GFFHeader _header;
        BiowareStruct[] _structs;
        long beginingOffset;
        long _dataBlockOffset;

        public GFFHeader header
        {
            get { return _header; }
        }
        public BiowareStruct[] structs
        {
            get { return _structs; }
        }

        public long dataOffset
        {
            get { return _dataBlockOffset; }
        }

        //Reads a GFF file from the file with path filename starting at offset
        public GFF(String filename, long offset)
            : base(filename)
        {
            //Save the filename for opening later
            beginingOffset = offset;

            OpenR();
            //Get the header of the gff
            _header = new GFFHeader(file);
            _dataBlockOffset = _header.dataOffset + beginingOffset;

            //Get the struct definitions
            _structs = new BiowareStruct[_header.structCount];

            for (int i = 0; i < _header.structCount; i++)
            {
                _structs[i] = new BiowareStruct(this, new GFFStructDefinition(file,beginingOffset));
            }
            Close();
        }

        public override void OpenR()
        {
            base.OpenR();
            file.BaseStream.Seek(beginingOffset, SeekOrigin.Begin);
        }
    }
}
