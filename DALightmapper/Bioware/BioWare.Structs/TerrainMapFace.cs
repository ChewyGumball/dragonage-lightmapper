using System;
using System.IO;

using Bioware.Files;

namespace Bioware.Structs
{
    public class TerrainMapFace
    {
        private const int EDGE_LIST_INDEX = 0;
        private const int ID_INDEX = 1;

        int[] _edges;
        int _id;
        BiowareStruct _definition;

        public int this[int i]
        {
            get { return _edges[i]; }
            set { _edges[i] = value; }
        }

        public int[] edges
        {
            get { return _edges; }
        }
        public int id
        {
            get { return _id; }
        }

        public BiowareStruct definition
        {
            get { return _definition; }
        }

        public TerrainMapFace(BiowareStruct definition, BinaryReader file)
        {
            _definition = definition;
            readData(file);
        }

        public void readData(BinaryReader file)
        {
            long currentPosition = file.BaseStream.Position;
            int reference;

            //Get the reference to the edge index list
            file.BaseStream.Seek(currentPosition + definition.fields[EDGE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            //Go to the list and fill it
            file.BaseStream.Seek(definition.binaryFile.dataOffset + reference, SeekOrigin.Begin);
            int length = file.ReadInt32();
            _edges = new int[length];
            for (int i = 0; i < _edges.Length; i++)
            {
                _edges[i] = file.ReadInt32();
            }

            //Get the face id
            file.BaseStream.Seek(currentPosition + definition.fields[ID_INDEX].index, SeekOrigin.Begin);
            _id = file.ReadInt32();
        }
    }
}
