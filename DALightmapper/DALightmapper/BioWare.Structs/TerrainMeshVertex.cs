using System.IO;

using OpenTK;

using Bioware.Files;

namespace Bioware.Structs
{
    class TerrainMeshVertex
    {
        private const int ID_INDEX = 0;
        private const int POSITION_INDEX = 1;
        private const int LEVEL_INDEX = 2;

        int _id, _level;
        Vector3 _position;
        BiowareStruct _definition;

        public int id
        {
            get { return _id; }
        }
        public Vector3 position
        {
            get { return _position; }
        }
        public BiowareStruct definition
        {
            get { return _definition; }
        }
        public TerrainMeshVertex(BiowareStruct definition, BinaryReader file)
        {
            _definition = definition;
            readData(file);
        }

        public void readData(BinaryReader file)
        {
            //Save the current position for later
            long currentPosition = file.BaseStream.Position;

            //Get the id
            file.BaseStream.Seek(currentPosition + definition.fields[ID_INDEX].index, SeekOrigin.Begin);
            _id = file.ReadInt32();

            //Get the position
            file.BaseStream.Seek(currentPosition + definition.fields[POSITION_INDEX].index, SeekOrigin.Begin);
            _position = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

            //get the mesh level
            file.BaseStream.Seek(currentPosition + definition.fields[LEVEL_INDEX].index, SeekOrigin.Begin);
            _level = file.ReadInt32();
        }
    }
}
