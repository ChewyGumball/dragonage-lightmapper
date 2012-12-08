using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Geometry
{
    public class Photon
    {
        public Vector3 position { get; private set; }
        public Vector3 colour { get; private set; }
        public Vector3 shadowColour { get; private set; }
        public bool affectsLightMap { get; private set; }
        public bool affectsShadowMap { get; private set; }

        public Photon(Vector3 p, Light l, float intensity)
        {
            position = p;
            colour = l.colour * intensity;
            shadowColour = l.shadowColour * intensity;
            affectsLightMap = l.inLightMap;
            affectsShadowMap = l.inShadowMap;
        }
    }
}
