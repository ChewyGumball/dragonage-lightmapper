using System.IO;
using System;

using OpenTK;

using Ben;

using Bioware.Structs;
using Bioware.Files;

namespace DALightmapper
{
    class BiowareModel
    {
        private ModelHierarchy _hierarchy;
        private Vector3 _position;
        private Quaternion _rotation;
        private String _modelFileName;
        private uint[] _textureIDs;
        private uint _modelID;

        Boolean _isTerrain;

        public ModelHierarchy hierarchy
        {
            get { return _hierarchy; }
            set { _hierarchy = value; }
        }
        public BiowareMesh mesh
        {
            get { return _hierarchy.mesh; }
        }
        public Vector3 position
        {
            get { return _position; }
        }
        public Quaternion rotation
        {
            get { return _rotation; }
        }
        public String meshFileName
        {
            get { return _hierarchy.mshName; }
        }
        public String modelFileName
        {
            get { return _modelFileName; }
        }
        public uint[] textureIDs
        {
            get { return _textureIDs; }
        }
        public uint modelID
        {
            get { return _modelID; }
        }
        public Boolean isTerrain
        {
            get { return _isTerrain; }
        }


        public BiowareModel(Vector3 pos, Quaternion rot, String fileName, uint ID, Boolean isTerrainModel)
        {
            _isTerrain = isTerrainModel;

            _position = pos;
            _rotation = rot;
            _modelFileName = fileName;
            _modelID = ID;
        }
    }
}
