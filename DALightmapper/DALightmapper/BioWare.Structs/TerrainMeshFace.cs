using System.IO;

using Bioware.Files;

namespace Bioware.Structs
{
    public class TerrainMeshFace
    {

        private const int ID_INDEX = 0;
        private const int EDGE_LIST_INDEX = 1;
        private const int MAP_ID_INDEX = 2;

        int[] _edges;
        int _id;
        int _mapId;
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
        public int mapId
        {
            get { return _mapId; }
            set { _mapId = value; }
        }
        public BiowareStruct definition
        {
            get { return _definition; }
        }
        public TerrainMeshFace(BiowareStruct definition,BinaryReader file)
        {
            _definition = definition;
            readData(file);
        }

        public void readData(BinaryReader file)
        {
            //Save the current position for later
            long startPosition = file.BaseStream.Position;
            int reference;

            //Get the face id
            file.BaseStream.Seek(startPosition + definition.fields[ID_INDEX].index, SeekOrigin.Begin);
            _id = file.ReadInt32();

            //Get the edge ids
            file.BaseStream.Seek(startPosition + definition.fields[EDGE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            //Seek to the edge list and fill it
            file.BaseStream.Seek(definition.binaryFile.dataOffset + reference, SeekOrigin.Begin);
            _edges = new int[file.ReadInt32()];
            for (int i = 0; i < _edges.Length; i++)
            {
                _edges[i] = file.ReadInt32();
            }

            //Get the map face id
            file.BaseStream.Seek(startPosition + definition.fields[MAP_ID_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            //Seek to the map id list (only 1 thing needed so skip the length integer at the beginning)
            file.BaseStream.Seek(definition.binaryFile.dataOffset + reference + 4, SeekOrigin.Begin);
            _mapId = file.ReadInt32();
        }
    }
}
