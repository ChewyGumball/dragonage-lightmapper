﻿using System;
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
        public BoundingBox[] bounds { get; private set; }

        public ModelInstance (String n, Model baseM, Vector3 pos, Quaternion rot, uint modelId)
        {
            name = n;
            baseModel = baseM;
            position = pos;
            rotation = rot;
            id = modelId;
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

        public Triangle getTri(int meshIndex, int triIndex)
        {
            Triangle oldTri = baseModel.meshes[meshIndex][triIndex];
            return new Triangle((Vector3.Transform(oldTri.x, rotation) + position),
                                (Vector3.Transform(oldTri.y, rotation) + position),
                                (Vector3.Transform(oldTri.z, rotation) + position),
                                oldTri.u,
                                oldTri.v,
                                oldTri.w);
        }

    }
}
