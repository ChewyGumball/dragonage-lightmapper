using System.Collections.Generic;

using OpenTK;

namespace DALightmapper
{
    class Texel
    {
        public List<Patch> patches {get; private set;}
        public Vector3 excidentLight
        {
            get { Vector3 temp = new Vector3(); foreach (Patch p in patches) temp += p.excidentLight; return temp / patches.Count; }
        }

        public Texel()
        {
            patches = new List<Patch>();
        }

        public void add(Patch p)
        {
            patches.Add(p);
        }
    }
}
