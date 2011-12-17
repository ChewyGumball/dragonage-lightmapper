using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace DALightmapper
{
    class Mesh
    {
        String _name;
        public Triangle[] tris { get; private set; }
        public Texel[,] texels { get; private set; }
        public BoundingBox bounds { get; private set; }
        public Boolean isLightmapped { get; private set; }
        public Boolean castsShadows { get; private set; }
        
        public String getName()
        {
            return _name;
        }

        public Mesh(String name, Triangle[] triangles, Boolean lightmap, Boolean shadows)
        {
            isLightmapped = lightmap;
            castsShadows = shadows;

            _name = name;
            tris = triangles;

            //make the bounding box
            float minX = triangles[0].x.X;
            float minY = triangles[0].x.Y;
            float minZ = triangles[0].x.Z;
            float maxX = triangles[0].x.X;
            float maxY = triangles[0].x.Y;
            float maxZ = triangles[0].x.Z;

            for (int i = 0; i < triangles.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    minX = Math.Min(triangles[i][j].X, minX);
                    minY = Math.Min(triangles[i][j].Y, minY);
                    minZ = Math.Min(triangles[i][j].Z, minZ);

                    maxX = Math.Max(triangles[i][j].X, maxX);
                    maxY = Math.Max(triangles[i][j].Y, maxY);
                    maxZ = Math.Max(triangles[i][j].Z, maxZ);
                }
            }

            bounds = new BoundingBox(new Vector3(maxX, maxY, maxZ), new Vector3(minX, minY, minZ));
        }

        //Make patches for a lightmap with dimensions [width,height]
        public void generatePatches(int width, int height)
        {
            texels = new Texel[width,height];
            //For each pixel
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2 topLeft = new Vector2(((float)i) / width, ((float)j + 1) / height);
                    Vector2 bottomRight = new Vector2(((float)(i + 1)) / width, ((float)(j)) / height);
                    texels[i, j] = new Texel();
                    //See if this uv is on any triangle in the mesh
                    foreach (Triangle t in tris)
                    { 
                        if (!t.isDegenerate() && t.isOnUVPixel(topLeft,bottomRight))
                        {
                              //                              Position                   normal     emmision             reflection     
                            texels[i, j].add( new Patch(t.uvTo3d(topLeft,bottomRight), t.normal, new Vector3(), new Vector3(0.7f, 0.7f, 0.7f)));
                        }
                    }
                }
            }
        }
    }
}
