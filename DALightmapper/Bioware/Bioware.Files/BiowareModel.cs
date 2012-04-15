using System.IO;
using System;

using OpenTK;

using Ben;

using Bioware.Structs;
using Bioware.Files;

namespace DALightmapper
{
    public class BiowareModel
    {
        public BiowareMesh mesh { get; private set; }
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public String meshFileName { get; private set; }
        public String modelFileName { get; private set; }
        public uint modelID { get; private set; }
        public int roomID { get; private set; }
        public Boolean isTerrain { get; private set; }


        public BiowareModel(Vector3 pos, Quaternion rot, String fileName, uint ID, int rID, Boolean isTerrainModel)
        {
            isTerrain = isTerrainModel;

            position = pos;
            rotation = rot;
            modelFileName = fileName;
            modelID = ID;
            roomID = rID;
        }
    }
}
