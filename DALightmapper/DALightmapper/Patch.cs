using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace DALightmapper
{
    class Patch
    {
        public Vector2 uvCoords { get; set; }

        public Boolean isActive { get; set; }
        public Vector3 incidentLight { get; set; }
        public Vector3 excidentLight { get; private set; }
        public Vector3 emmission { get; private set; }
        public Vector3 reflectance { get; private set; }

        public Vector3 normal { get; private set; }
        public Vector3 position { get; private set; }


        public Patch()
        {
            isActive = false;
        }
        public Patch(Vector2 uv, Vector3 pos, Vector3 norm, Vector3 emm, Vector3 refl, Vector3 incident, Vector3 excident)
        {
            uvCoords = uv;
            incidentLight = incident;
            excidentLight = excident;
            emmission = emm;
            reflectance = refl;
            normal = norm;
            position = pos;
            isActive = true;
        }

        public void updatePatch(int textureID)
        {

        }
    }
}
