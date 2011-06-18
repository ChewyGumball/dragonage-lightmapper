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
        Patch[,] _patches;

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
            _patches = new Patch[width,height];
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
                              //                        uv        Position            normal     emmision             reflection              incident        exident
                            _patches[i, j] = new Patch(uvCoords, t.uvTo3d(uvCoords), t.normal, new Vector3(), new Vector3(0.7f, 0.7f, 0.7f), new Vector3(), new Vector3());
                            break;
                        }
                    }

                    //if the uv coords were not on a triangle, make an inactive patch
                    if (_patches[i, j] == null)
                    {
                        _patches[i, j] = new Patch();
                    }
                }
            }
        }
    }
}
