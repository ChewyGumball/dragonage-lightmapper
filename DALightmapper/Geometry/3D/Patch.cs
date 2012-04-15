using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace Geometry
{
    public class Patch
    {
        public Vector3 emmission { get; private set; }
        //public Vector3 reflectance { get; private set; }

        public Vector3 normal { get; private set; }
        public Vector3 position { get; private set; }

        public Vector3 incidentLight { get; set; }
        public Vector3 excidentLight
        {
            get
            {
                return emmission + incidentLight;
            }
        }

        public Patch()
        { }
        public Patch(Vector3 pos, Vector3 norm, Vector3 emm, Vector3 refl)
        {
            emmission = emm;
            incidentLight = new Vector3();
            //reflectance = refl;
            normal = norm;
            position = pos;
        }
        public Patch(Patch p, Vector3 offset, Quaternion rotation)
            : this( p.position, p.normal, p.emmission, new Vector3()/*p.reflectance*/)
        {
            position = Vector3.Transform(p.position, rotation) + offset;
            normal = Vector3.Transform(p.normal, rotation);
        }
    }
}
