using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace Geometry
{
    public class Mesh
    {
        public String name { get; private set; }
        public String id { get; private set; }
        public Triangle[] tris { get; private set; }
        public Texel[,] texels { get; private set; }
        public BoundingBox bounds { get; private set; }
        public Boolean isLightmapped { get; private set; }
        public Boolean castsShadows { get; private set; }

        public Boolean hasGeneratedPatches { get; private set; }


        public Mesh(String n, Triangle[] triangles, Boolean lightmap, Boolean shadows, Vector3 offset, Quaternion rotation, String chunkID)
        {
            id = chunkID;
            isLightmapped = lightmap;
            castsShadows = shadows;

            name = n;
            tris = new Triangle[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                tris[i] = new Triangle(triangles[i], offset, rotation);
            }

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
            if (!hasGeneratedPatches && isLightmapped)
            {
                hasGeneratedPatches = true;
                //*
                texels = new Texel[width, height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        texels[i, j] = new Texel();
                    }
                }

                foreach (Triangle t in tris)
                {
                    int test = (int)(t.a.X * width);
                    int minX = (int)Math.Floor(t.a.X * width);
                    int minY = (int)Math.Floor(t.a.Y * height);
                    int maxX = (int)Math.Ceiling(t.a.X * width);
                    int maxY = (int)Math.Ceiling(t.a.Y * height); 

                    int nextMinX = (int)Math.Floor(t.b.X * width);
                    int nextMinY = (int)Math.Floor(t.b.Y * height);
                    int nextMaxX = (int)Math.Ceiling(t.b.X * width);
                    int nextMaxY = (int)Math.Ceiling(t.b.Y * height);

                    maxX = Math.Max(maxX, nextMaxX);
                    maxY = Math.Max(maxY, nextMaxY);
                    minX = Math.Min(minX, nextMinX);
                    minY = Math.Min(minY, nextMinY);

                    nextMinX = (int)Math.Floor(t.c.X * width);
                    nextMinY = (int)Math.Floor(t.c.Y * height);
                    nextMaxX = (int)Math.Ceiling(t.c.X * width);
                    nextMaxY = (int)Math.Ceiling(t.c.Y * height);

                    maxX = Math.Max(maxX, nextMaxX) + 1;
                    maxY = Math.Max(maxY, nextMaxY) + 1;
                    minX = Math.Min(minX, nextMinX) - 1;
                    minY = Math.Min(minY, nextMinY) - 1; 

                    /*
                    maxX = Math.Min(width, maxX + 1);
                    maxY = Math.Min(height, maxY + 1);
                    minX = Math.Max(0, minX - 1);
                    minY = Math.Max(0, minY - 1);
                    //*/
                    for (int i = minX; i < maxX; i++)
                    {
                        for (int j = minY; j < maxY; j++)
                        {
                            Vector2 topLeft = new Vector2(((float)i) / width, ((float)j + 1) / height);
                            Vector2 bottomRight = new Vector2(((float)(i + 1)) / width, ((float)j) / height);
                            if (!t.isDegenerate && t.isOnUVPixel(topLeft, bottomRight))
                            {
                                //                              Position                   normal     emmision             reflection     
                                texels[i % width, j % height].add(new Patch(t.uvTo3d(topLeft, bottomRight), t.normal, new Vector3(), new Vector3(0.7f, 0.7f, 0.7f)));
                            }
                        }
                    }
                }
            }
        }
    }
}
