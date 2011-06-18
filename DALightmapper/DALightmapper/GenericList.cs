using System;
using System.Collections.Generic;
using System.IO;
using Bioware.Files;
using Ben;

namespace Bioware.Structs
{
    class GenericList
    {
        GFFFieldFlags[] _flags;
        uint[] _offsets;

        public uint this[int i]
        {
            get { return _offsets[i]; }
        }

        public GFFFieldFlags[] type
        {
            get { return _flags; }
        }

        public int length
        {
            get { return _offsets.Length; }
        }

        public GenericList(BinaryReader reader)
        {
            //This is the number of places in the list
            //  But some may be null so can't be relied upon
            //  as only non null entries are usefull (null is not a valid struct type)
            int length = reader.ReadInt32();

            List<GFFFieldFlags> flags = new List<GFFFieldFlags>();
            GFFFieldFlags tempFlag;
            uint tempOffset;
            List<uint> offsets = new List<uint>();
            for (int i = 0; i < length; i++)
            {
                tempFlag = new GFFFieldFlags(reader);
                tempOffset = reader.ReadUInt32();
                if (tempFlag.id != GFFFIELDTYPE.GenericList)
                {
                    flags.Add(tempFlag);
                    offsets.Add(tempOffset);
                }
            }

            length = flags.Count;
            _flags = flags.ToArray();
            _offsets = offsets.ToArray();
        }
    }
}
