using System;
using OpenTK;

namespace DALightmapper
{
    class ModelInstance
    {
        public String name { get; private set; }
        public Model baseModel { get; private set; }
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public uint id { get; private set; }

        public ModelInstance (String n, Model baseM, Vector3 pos, Quaternion rot, uint modelId)
        {
            name = n;
            baseModel = baseM;
            position = pos;
            rotation = rot;
            id = modelId;
        }

        public int getNumMeshes()
        {
            return baseModel.meshes.Length;
        }

        public Patch getPatch(int meshIndex, int patchIndex)
        { 
            //Do stuff
            //Mesh has base patch, need to transform by position and rotation
            return new Patch();
        }

        public Triangle getTri(int meshIndex, int triIndex)
        {
            Triangle oldTri = baseModel.meshes[meshIndex].tris[triIndex];
            return new Triangle((Vector3.Transform(oldTri.x, rotation) + position),
                                (Vector3.Transform(oldTri.y, rotation) + position),
                                (Vector3.Transform(oldTri.z, rotation) + position),
                                oldTri.u,
                                oldTri.v,
                                oldTri.w);
        }

    }
}
