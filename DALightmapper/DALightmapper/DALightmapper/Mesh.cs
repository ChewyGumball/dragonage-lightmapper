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
        Triangle[] _tris;
        public Patch[,] patches { get; private set; }
        public BoundingBox bounds { get; private set; }
        public Boolean isLightmapped { get; private set; }
        
        public Triangle this[int i]
        {
            get{ return _tris[i];}
        }

        public Mesh(String name, Triangle[] triangles, Boolean lightmap)
        {
            isLightmapped = lightmap;

            _name = name;
            _tris = triangles;

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
            patches = new Patch[width,height];
            //For each pixel
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2 uvCoords = new Vector2(i / width, j / height);

                    //See if this uv is on any triangle in the mesh
                    foreach (Triangle t in _tris)
                    { 
                        if (t.uvIsOnThisTriangle(uvCoords))
                        {
                              //                        uv        Position            normal     emmision             reflection     
                            patches[i, j] = new Patch(uvCoords, t.uvTo3d(uvCoords), t.normal, new Vector3(), new Vector3(0.7f, 0.7f, 0.7f));
                            break;
                        }
                    }

                    //if the uv coords were not on a triangle, make an inactive patch
                    if (patches[i, j] == null)
                    {
                        patches[i, j] = new Patch();
                    }
                }
            }
        }

        public int getNumTris()
        {
            return _tris.Length;
        }
    }
}
