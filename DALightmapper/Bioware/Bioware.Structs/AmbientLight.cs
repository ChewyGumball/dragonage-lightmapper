using System;
using OpenTK;

using Geometry;

namespace Bioware.Structs
{
    public class AmbientLight : Light
    {
        public AmbientLight(Vector3 pos, Vector3 col, Vector3 shadowCol, float intense, Boolean shadows, Boolean shoots)
            : base(pos, col, shadowCol, intense, shadows, shoots )
        { }
        public override float influence(Vector3 patch)
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
