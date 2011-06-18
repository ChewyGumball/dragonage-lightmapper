using System;
using OpenTK;

namespace DALightmapper
{
    class ModelInstance
    {
        String _name;
        Model _baseModel;
        Vector3 _position;
        Quaternion _rotation;
        uint _id;

        public String name
        {
            get { return _name; }
        }
        public uint ID
        {
            get { return _id; }
        }

        public ModelInstance (String name, Model baseM, Vector3 pos, Quaternion rot, uint modelId)
        {
            _name = name;
            _baseModel = baseM;
            _position = pos;
            _rotation = rot;
            _id = modelId;
        }

        public int getNumMeshes()
        {
            return _baseModel.meshes.Length;
        }

        public Patch getPatch(int meshIndex, int patchIndex)
        { 
            //Do stuff
            //Mesh has base patch, need to transform by position and rotation
            return new Patch();
        }

        public Triangle getTri(int meshIndex, int triIndex)
        {
            Triangle oldTri = _baseModel.meshes[meshIndex].tris[triIndex];
            return new Triangle((Vector3.Transform(oldTri.x, _rotation) + _position),
                                (Vector3.Transform(oldTri.y, _rotation) + _position),
                                (Vector3.Transform(oldTri.z, _rotation) + _position),
                                oldTri.u,
                                oldTri.v,
                                oldTri.w);
        }

    }
}
