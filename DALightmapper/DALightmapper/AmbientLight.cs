
using OpenTK;

namespace DALightmapper
{
    class AmbientLight : Light
    {
        public AmbientLight(Vector3 pos, Vector3 col, float intense, LightType t)
            : base(pos, col, intense, t)
        { }
        public override float influence(Vector3 point)
        {
            //Same intensity no matter where point is
            return intensity;
        }
    }
}
