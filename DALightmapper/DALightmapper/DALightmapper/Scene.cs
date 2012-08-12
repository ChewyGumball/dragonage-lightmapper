using System;
using System.Collections.Generic;

using Bioware.Structs;
using Bioware.Files;
using Geometry;

namespace DALightmapper
{
    public interface Scene
    {
        List<ModelInstance> lightmapModels { get; }
        List<Light> lights { get; }

        void exportLightmaps(List<LightMap> lightmaps);
    }
}
