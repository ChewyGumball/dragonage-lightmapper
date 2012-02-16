using System.Collections.Generic;

using OpenTK;

namespace DALightmapper
{
    public class Texel
    {
        public List<Patch> patches {get; private set;}
        public Vector3 excidentLight
        {
            get
            {
                Vector3 temp = new Vector3();
                foreach (Patch p in patches)
                {
                    temp += p.excidentLight;
                }
                if (patches.Count > 0)
                {
                    temp /= patches.Count;
                }
                return temp;
            }
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
