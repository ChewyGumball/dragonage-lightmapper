using System;
using System.Collections.Generic;

using Bioware.Structs;
using Bioware.Files;

namespace DALightmapper
{
    public interface Scene
    {
        public List<ModelInstance> lightmapModels { get; private set; }
        public List<Light> lights { get; private set; }

        public void exportLightmaps(List<LightMap> lightmaps);
    }
}
