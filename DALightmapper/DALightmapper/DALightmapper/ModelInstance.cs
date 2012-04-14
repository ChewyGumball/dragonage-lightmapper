using System;
using System.Collections.Generic;
using OpenTK;

namespace DALightmapper
{
    public class ModelInstance
    {
        public String name { get; private set; }
        public Model baseModel { get; private set; }
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public uint id { get; private set; }
        public int roomID { get; private set; }
        public String layoutName { get; private set; }
        public BoundingBox[] bounds { get; private set; }
        public List<Triangle> tris
        {
            get
            {
                List<Triangle> triangles = new List<Triangle>();
                for (int i = 0; i < baseModel.meshes.Length; i++)
                {
                    for (int j = 0; j < baseModel.meshes[i].tris.Length; j++)
                    {
                        triangles.Add(getTri(i, j));
                    }
                }
                return triangles;
            }
        }

        public ModelInstance (String n, Model baseM, Vector3 pos, Quaternion rot, uint modelId, int rID, string lName)
        {
            name = n;
            baseModel = baseM;
            position = pos;
            rotation = rot;
            id = modelId;
            roomID = rID;
            layoutName = lName;
            //Need to make bounding boxes
            bounds = new BoundingBox[baseModel.meshes.Length];
            for (int i = 0; i < bounds.Length; i++)
            {
                bounds[i] = new BoundingBox(baseModel.meshes[i].bounds, position, rotation);
            }
        }

        public int getNumMeshes()
        {
            return baseModel.meshes.Length;
        }

        private Triangle getTri(int meshIndex, int triIndex)
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
