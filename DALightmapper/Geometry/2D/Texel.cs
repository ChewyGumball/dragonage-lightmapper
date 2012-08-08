using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

namespace Geometry
{
    [DebuggerDisplay("Count = {patches.Count}")]
    public class Texel
    {
        public List<Patch> patches {get; private set;}
        public Vector3 excidentLight
        {
            get
            {
                Vector3 temp = new Vector3();
                if (patches.Count > 0)
                {
                    foreach (Patch p in patches)
                    {
                        temp += p.excidentLight;
                    }

                    temp /= patches.Count;
                }
                return temp;
            }
        }
        public Vector3 ambientValue
        {
            get
            {
                int ambient = 0;
                if (patches.Count > 0)
                {
                    foreach (Patch p in patches)
                    {
                        ambient += p.ambient;
                    }

                    ambient /= patches.Count;
                }

                return new Vector3(ambient, ambient, ambient);
            }
        }

        public Vector3 shadowValue
        {
            get
            {
                Vector3 temp = new Vector3();
                if (patches.Count > 0)
                {
                    foreach (Patch p in patches)
                    {
                        temp += p.shadowColour;
                    }

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
