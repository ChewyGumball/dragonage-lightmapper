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
        public Boolean isLightmapped { get; private set; }
        public Boolean castsShadows { get; private set; }
        public Model(String name, Mesh[] mesh)
        {
            _name = name;
            _meshes = mesh;
            foreach (Mesh m in mesh)
            {
                if (m.isLightmapped)
                {
                    isLightmapped = true;
                    break;
                }
            }

            foreach (Mesh m in mesh)
            {
                if (m.castsShadows)
                {
                    castsShadows = true;
                    break;
                }
            }
        }
    }
}
