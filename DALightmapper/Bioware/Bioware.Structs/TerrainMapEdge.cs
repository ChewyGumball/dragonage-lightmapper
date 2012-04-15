using System.IO;

using Bioware.Files;

namespace Bioware.Structs
{
    public class TerrainMapEdge
    {
        private const int ID_INDEX = 0;
        private const int START_VERTEX_INDEX = 1;

        int _id, _startVertexIndex;
        BiowareStruct _definition;

        public int id
        {
            get { return _id; }
        }
        public int startVertexIndex
        {
            get { return _startVertexIndex; }
            set { _startVertexIndex = value; }
        }
        public BiowareStruct definition
        {
            get { return _definition; }
        }

        public TerrainMapEdge(BiowareStruct definition, BinaryReader file)
        {
            _definition = definition;
            readData(file);
        }

        public void readData(BinaryReader file)
        {
            //Save the current Position for later
            long currentPosition = file.BaseStream.Position;

            //Get the id
            file.BaseStream.Seek(currentPosition + definition.fields[ID_INDEX].index, SeekOrigin.Begin);
            _id = file.ReadInt32();

            //Get the start vertex index
            file.BaseStream.Seek(currentPosition + definition.fields[START_VERTEX_INDEX].index, SeekOrigin.Begin);
            _startVertexIndex = file.ReadInt32();
        }
    }
}
