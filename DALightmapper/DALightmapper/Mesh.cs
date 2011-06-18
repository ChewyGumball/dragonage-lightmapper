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
        Patch[] _patches;

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

        public void generatePatches()
        {
            //Do stuff
        }

        public Vector3 uvTo3d(Vector2 uvCoords)
        {
            Vector3 d3coords = new Vector3();
            //doStuff
            return d3coords;
        }
    }
}
