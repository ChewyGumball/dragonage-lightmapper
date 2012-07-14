using System;
using System.Collections.Generic;
using OpenTK;

using Geometry;

namespace Bioware.Structs
{
    public class ModelInstance
    {
        public String name { get; private set; }
        public Model baseModel { get; private set; }
        public Vector3 position { get; private set; }
        //public Quaternion rotation { get; private set; }
        public Matrix4 transform { get; private set; }
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

        public ModelInstance(String n, Model baseM, Matrix4 trans)
        {
            name = n;
            baseModel = baseM;
            position = trans.Row3.Xyz;
            transform = trans;
        }

        public ModelInstance (String n, Model baseM, Vector3 pos, Quaternion rot, uint modelId, int rID, string lName)
        {
            name = n;
            baseModel = baseM;
            position = pos;

            transform = new Matrix4(new Vector4(Vector3.Transform(Vector3.UnitX, rot), 0),
                                    new Vector4(Vector3.Transform(Vector3.UnitY, rot), 0),
                                    new Vector4(Vector3.Transform(Vector3.UnitZ, rot), 0),
                                    new Vector4(pos, 1));
            id = modelId;
            roomID = rID;
            layoutName = lName;
            //Need to make bounding boxes
            bounds = new BoundingBox[baseModel.meshes.Length];
            for (int i = 0; i < bounds.Length; i++)
            {
                bounds[i] = new BoundingBox(baseModel.meshes[i].bounds, transform);
            }
        }

        public int getNumMeshes()
        {
            return baseModel.meshes.Length;
        }

        private Triangle getTri(int meshIndex, int triIndex)
        {
            Triangle oldTri = baseModel.meshes[meshIndex].tris[triIndex];
            return new Triangle((Vector3.Transform(oldTri.x, transform)),
                                (Vector3.Transform(oldTri.y, transform)),
                                (Vector3.Transform(oldTri.z, transform)),
                                oldTri.u,
                                oldTri.v,
                                oldTri.w);
        }

    }
}
