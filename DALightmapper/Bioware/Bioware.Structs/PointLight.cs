using OpenTK;
using System;

using Geometry;

namespace Bioware.Structs
{
    public class PointLight : Light
    {
        public PointLight(Vector3 pos, Vector3 col, float intense, LightType t, Boolean shoots)
            : base(pos, col, intense, t, shoots)
        { }
        public override float influence(Vector3 patch)
        {
            //Find the distance from the light
            float distance = Vector3.Subtract(position, patch).LengthFast;
            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 d = new Vector3(1, distance, (distance * distance));
            //Multiply the intensity by the attenuation
            return intensity / Vector3.Dot(d, attenuation);
        }

        public override Vector3 generateRandomDirection()
        {
            Vector3 temp = new Vector3((float)nextGaussian(), (float)nextGaussian(), (float)nextGaussian());
            temp.Normalize();
            return temp;

        }
    }
}
