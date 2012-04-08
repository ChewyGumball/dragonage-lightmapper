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

        public Targa makeLightMapTexture(String directory)
        {
            Pixel[] pixels = new Pixel[width * height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixels[j * width + i] = new Pixel((texels[i, j].excidentLight) * 255);
                }
            }
            Targa texture = new Targa(directory + "\\lmXXX_" + model.roomID + model.id + model.baseModel.meshes[index].id + ".StandardLightMap.tga", pixels, (short)width, (short)height, 24);
            return texture;
        }

        public Targa makeAmbientOcclutionTexture(String directory)
        {
            return new Targa(directory + "\\lmXXX_" + model.roomID + model.id + model.baseModel.meshes[index].id + ".AmbientOcclusionMap.tga", (short)width, (short)height);
        }

        public Targa makeShadowMapTexture(String directory)
        {
            return new Targa(directory + "\\lmXXX_" + model.roomID + model.id + model.baseModel.meshes[index].id + ".ShadowMap.tga", (short)width, (short)height);
        }
    }
}
