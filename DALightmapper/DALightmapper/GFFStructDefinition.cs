using System;
using System.Text;
using System.IO;

namespace Bioware.Structs
{
    class GFFStructDefinition
    {
        GFFSTRUCTTYPE _id;
        uint _fieldCount;
        uint _fieldOffset;
        uint _structSize;
        GFFFieldDefinition[] _fields;

        public GFFSTRUCTTYPE id
        {
            get { return _id; }
        }
        public uint fieldCount
        {
            get { return _fieldCount; }
        }
        public uint fieldOffset
        {
            get { return _fieldOffset; }
        }
        public uint structSize
        {
            get { return _structSize; }
        }
        public GFFFieldDefinition[] fields
        {
            get { return _fields; }
        }

        public GFFStructDefinition(BinaryReader file, long beginningOffset)
        {
            //Read in struct definition
            _id = (GFFSTRUCTTYPE)file.ReadInt32();

            _fieldCount = file.ReadUInt32();
            _fields = new GFFFieldDefinition[fieldCount];

            _fieldOffset = file.ReadUInt32();
            _structSize = file.ReadUInt32();

            //Read in field definitions
            long currentPosition = file.BaseStream.Position;
            file.BaseStream.Seek(fieldOffset + beginningOffset, SeekOrigin.Begin);

            for (int i = 0; i < fieldCount; i++)
                _fields[i] = new GFFFieldDefinition(file);

            file.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }

        override public String ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("STRUCT: ID: "+id.ToString());
            s.Append("  Field Count: " + fieldCount);
            s.Append("  Field Offset: " + fieldOffset);
            s.Append("  Struct Length: " + structSize);
            return s.ToString();
        }
    }
}
