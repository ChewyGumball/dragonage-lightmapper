using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;

namespace Geometry
{
    [DebuggerDisplay("Count = {patches.Count}")]
    public class Texel
    {
        public List<Patch> patches { get; private set; }
        public Vector3 excidentLight
        {
            get
            {
                Vector3 temp = new Vector3();
                if (patches.Count > 0)
                {
                    IEnumerable<Patch> lightPatches = patches.TakeWhile(patch => patch.inLightMap);
                    foreach (Patch p in lightPatches)
                    {
                        temp += p.excidentLight;
                    }

                    if (lightPatches.Count() > 0)
                    {
                        temp /= lightPatches.Count();
                    }
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
                        ambient += p.ambientOcclusion;
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
                    IEnumerable<Patch> shadowPatches = patches.TakeWhile(patch => patch.inShadowMap);
                    foreach (Patch p in shadowPatches)
                    {
                        temp += p.exidentShadow;
                    }
                    if (shadowPatches.Count() > 0)
                    {
                        temp /= shadowPatches.Count();
                    }
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
