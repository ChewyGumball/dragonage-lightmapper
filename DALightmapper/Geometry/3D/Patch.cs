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

        public Vector3 ambientLight { get; set; }
        public Vector3 incidentLight { get; set; }
        public Vector3 excidentLight
        {
            get
            {
                return emmission + incidentLight + ambientLight;
            }
        }

        public int ambientOcclusion { get; set; }
        public Vector3 incidentShadow { get; set; }
        public Vector3 exidentShadow
        {
            get
            {
                return incidentShadow;
            }
        }

        public bool inLightMap { get; private set; }
        public bool inShadowMap { get; private set; }

        public Patch()
        { }
        public Patch(Vector3 pos, Vector3 norm, Vector3 emm, Vector3 refl)
        {
            emmission = emm;
            incidentLight = new Vector3();
            //reflectance = refl;
            normal = norm;
            position = pos;
            //ambient = 0;
            ambientOcclusion = 255;
            incidentShadow = new Vector3();
        }
        public Patch(Patch p, Matrix4 transform)
            : this( p.position, p.normal, p.emmission, new Vector3()/*p.reflectance*/)
        {
            position = Vector3.Transform(p.position, transform);
            normal = Vector3.Transform(p.normal, transform) - transform.Row3.Xyz;
        }

        public void absorbPhoton(Photon p)
        {
            if (p.affectsLightMap)
            {
                inLightMap = true;
                incidentLight += p.colour; // (p.position - position).LengthSquared;
            }
            if (p.affectsShadowMap)
            {
                inShadowMap = true;
                incidentShadow += p.shadowColour; // (p.position - position).LengthSquared;
            }
        }
    }
}
