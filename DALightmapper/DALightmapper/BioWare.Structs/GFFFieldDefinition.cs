using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Bioware.Structs
{
    //Enum for bit descriptions as per the GFF wiki
    enum Bit16 : ushort { one = 0x8000, two = 0x4000, three = 0x2000, four = 0x1000, five = 0x0800, six = 0x0400, seven = 0x0200, eight = 0x0100, nine = 0x0080, ten = 0x0040, eleven = 0x0020, twelve = 0x0010, thirteen = 0x0008, fourteen = 0x0004, fifteen = 0x0002, sixteen = 0x0001 };

    class GFFFieldDefinition
    {
        GFFID _label;   //ID of the field
        GFFFieldFlags _flags;   //The flags and type of this field
        uint _index;    //Offset from beginning of struct to data

        public GFFID label
        {
            get { return _label; }
        }
        public GFFFieldFlags flags
        {
            get { return _flags; }
        }
        public GFFFIELDTYPE id
        {
            get { return flags.id; }
        }
        public bool isList
        {
            get { return flags.isList; }
        }
        public bool isStruct
        {
            get { return flags.isStruct; }
        }
        public bool isReference
        {
            get { return flags.isReference; }
        }
        public uint index
        {
            get { return _index; }
        }

        public GFFFieldDefinition(BinaryReader file)
        {
            _label = (GFFID)file.ReadInt32();
            _flags = new GFFFieldFlags(file);
            _index = file.ReadUInt32();

        }

        override public String ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("FIELD: Type: " + label.ToString());
            if (isStruct)
            {
                s.Append("  Struct: " + (int)id);
            }
            else
            {
                s.Append("  ID: " + id.ToString());
            }

            if (isList)
            {
                s.Append("  LIST");
            }
            if (isReference)
            {
                s.Append("  REFERENCE");
            }

            s.Append("  Offset: " + index);

            return s.ToString();
        }
    }
}
