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

        public Photon(Vector3 p, Light l)
        {
            position = p;
            colour = l.colour * l.influence(position);
            shadowColour = l.shadowColour * l.influence(position);
            affectsLightMap = l.inLightMap;
            affectsShadowMap = l.inShadowMap;
        }
    }
}
