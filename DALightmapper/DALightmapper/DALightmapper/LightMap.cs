using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using Bioware.Structs;
using Bioware.Files;

using Ben;

using Geometry;

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

        String name;

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

            name = "lm" + model.layoutName + "_" + convertToBase36(model.id) + "_0" + model.baseModel.meshes[index].id;
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
            Targa texture = new Targa(directory + "\\" + name + ".StandardLightMap.tga", pixels, (short)width, (short)height, 24);
            return texture;
        }

        public Targa makeAmbientOcclutionTexture(String directory)
        {
            return new Targa(directory + "\\" + name + ".AmbientOcclusionMap.tga", (short)width, (short)height);
        }

        public Targa makeShadowMapTexture(String directory)
        {
            return new Targa(directory + "\\" + name + ".ShadowMap.tga", (short)width, (short)height);
        }
        
        private static String convertToBase36(uint number)
        {
            char[] digits = {   '0','1','2','3','4','5','6','7','8','9',
                                'a','b','c','d','e','f','g','h','i','j',
                                'k','l','m','n','o','p','q','r','s','t',
                                'u','v','w','x','y','z'
                            };
            String convertedNumber = "";

            while (number > 1)
            {
                convertedNumber = digits[number % 36] + convertedNumber;
                number = number / 36;
            }

            return convertedNumber;
        }
    }
}
