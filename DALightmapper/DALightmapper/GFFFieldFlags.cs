using System;
using System.IO;

namespace Bioware.Structs
{
    class GFFFieldFlags
    {
        GFFFIELDTYPE _id;   //Type of field
        bool _isList;   //Whether its a pointer to a list
        bool _isStruct; //Whether its a struct or not
        bool _isReference;  //Whether its a reference or not

        public GFFFIELDTYPE id
        {
            get { return _id; }
        }
        public bool isList
        {
            get { return _isList; }
        }
        public bool isStruct
        {
            get { return _isStruct; }
        }
        public bool isReference
        {
            get { return _isReference; }
        }

        public GFFFieldFlags(BinaryReader reader)
        {
            _id = (GFFFIELDTYPE)reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();

            _isList = (flags & (ushort)Bit16.one) != 0;
            _isStruct = (flags & (ushort)Bit16.two) != 0;
            _isReference = (flags & (ushort)Bit16.three) != 0;
        }
       
    }
}
