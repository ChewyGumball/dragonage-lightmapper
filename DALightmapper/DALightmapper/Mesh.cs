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

        public Triangle[] tris
        {
            get { return _tris; }
        }

        public Triangle this[int i]
        {
            get{ return _tris[i];}
        }

        public Mesh(String name, Triangle[] triangles)
        {
            _name = name;
            _tris = triangles;
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
                    foreach (Triangle t in tris)
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
    }
}
