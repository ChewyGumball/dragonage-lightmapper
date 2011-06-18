using System;
using System.IO;
using System.Collections.Generic;
using Bioware.Files;

namespace Bioware.Structs
{
    class BiowareStruct
    {
        GFFStructDefinition _definition;
        GFF _binaryFile;

        public GFF binaryFile
        {
            get { return _binaryFile; }
        }
        public GFFSTRUCTTYPE type
        {
            get { return definition.id; }
        }
        public uint structSize
        {
            get { return definition.structSize; }
        }
        public GFFStructDefinition definition
        {
            get { return _definition; }
        }
        public GFFFieldDefinition[] fields
        {
            get { return definition.fields; }
        }
        public long offset
        {
            get { return _definition.fieldOffset; }
        }

        public BiowareStruct (GFF f, GFFStructDefinition def)
        {
            _binaryFile = f;
            _definition = def;
        }
    }
}
