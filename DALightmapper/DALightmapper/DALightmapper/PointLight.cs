using OpenTK;

namespace DALightmapper
{
    class PointLight : Light
    {
        public PointLight(Vector3 pos, Vector3 col, float intense, LightType t)
            : base(pos, col, intense, t)
        { }
        public override float influence(Vector3 point)
        {
            //Find the distance from the light
            float distance = Vector3.Subtract(position, point).LengthFast;
            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 d = new Vector3(1, distance, (distance * distance));
            //Multiply the intensity by the attenuation
            return intensity / Vector3.Dot(d, attenuation);
        }
    }
}
