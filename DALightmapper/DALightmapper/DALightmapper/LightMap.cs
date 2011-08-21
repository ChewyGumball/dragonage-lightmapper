using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Bioware.Structs;
using Bioware.Files;

using Ben;

namespace DALightmapper
{
    class LightMap
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public PatchInstance[,] patches { get; private set; }
        public ModelInstance model { get; private set; }
        public Mesh mesh { get; private set; }

        public LightMap(ModelInstance mi, int meshIndex)
        {
            model = mi;
            mesh = mi.baseModel.meshes[meshIndex];
            Patch[,] meshPatches = mesh.patches;

            width = meshPatches.GetLength(0);
            height = meshPatches.GetLength(1);

            patches = new PatchInstance[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    patches[i, j] = new PatchInstance(meshPatches[i, j], model.position, model.rotation);
                }
            }
        }

        public Targa makeIntoTexture(String directory)
        {
            Targa texture = new Targa(directory + "\\" + model.name + model.id);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    texture[i, j] = new Pixel(patches[i, j].excidentLight);
                }
            }
            return texture;
        }
    }
}
