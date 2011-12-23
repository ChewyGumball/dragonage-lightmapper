using System;
using OpenTK;

namespace DALightmapper
{
    class AmbientLight : Light
    {
        public AmbientLight(Vector3 pos, Vector3 col, float intense, LightType t, Boolean shoots)
            : base(pos, col, intense, t, shoots )
        { }
        public override float influence(Patch patch)
        {
            //Same intensity no matter where point is
            return intensity;
        }

        public override Vector3 generateRandomDirection()
        {
            throw new System.NotImplementedException();
        }
    }
}
