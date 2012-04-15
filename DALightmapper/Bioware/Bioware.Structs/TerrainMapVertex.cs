using System.IO;

using OpenTK;

using Bioware.Files;

namespace Bioware.Structs
{
    public class TerrainMapVertex
    {
        private const int ID_INDEX = 0;
        private const int POSITION_INDEX = 1;

        int _id;
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
        public TerrainMapVertex(BiowareStruct definition, BinaryReader file)
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
        }
    }
}
