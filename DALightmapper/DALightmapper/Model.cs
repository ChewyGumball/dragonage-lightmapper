using System;
using Ben;
using OpenTK;

namespace DALightmapper
{
    class Model
    {
        String _name;
        Mesh[] _meshes;

        public String name
        {
            get { return _name; }
        }
        public Mesh[] meshes
        {
            get { return _meshes; }
        }

        public Model(String name, Mesh[] mesh)
        {
            _name = name;
            _meshes = mesh;
        }
    }
}
