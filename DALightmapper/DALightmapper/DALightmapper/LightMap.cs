using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using Bioware.Structs;
using Bioware.Files;

using Ben;

namespace DALightmapper
{
    public class LightMap
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public List<Patch> patches { get; private set; }
        public ModelInstance model { get; private set; }
        public Mesh mesh { get; private set; }

        private int index;
        private Texel[,] texels;


        public LightMap(ModelInstance mi, int meshIndex)
        {
            index = meshIndex;
            model = mi;
            mesh = mi.baseModel.meshes[meshIndex];

            width = mesh.texels.GetLength(0);
            height = mesh.texels.GetLength(1);

            texels = new Texel[width,height];
            patches = new List<Patch>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    texels[i,j] = new Texel();
                    foreach (Patch p in mesh.texels[i,j].patches)
                    {
                        Patch temp = new Patch(p,model.position, model.rotation);
                        texels[i,j].add(temp);
                        patches.Add(temp);
                    }
                }
            }
        }

        public Targa makeIntoTexture(String directory)
        {
            Targa texture = new Targa(directory + "\\lmXXX_" + model.roomID + model.id + model.baseModel.meshes[index].id + ".tga", (short)width, (short)height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    texture[i, j] = new Pixel((texels[i, j].excidentLight) * 255);
                }
            }
            return texture;
        }
    }
}
