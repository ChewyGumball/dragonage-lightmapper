using OpenTK;
using System;

using Geometry;

namespace Bioware.Structs
{
    public class PointLight : Light
    {
        private float radius;
        public PointLight(Vector3 pos, Vector3 col, Vector3 shadowCol, float intense, float r, Boolean shoots)
            : base(pos, col, shadowCol, intense, shoots)
        {
            radius = r;
        }
        public override float influence(Vector3 patch)
        {
            //Find the distance from the light
            float distance = Vector3.Subtract(position, patch).Length;
           
            //If the patch is outside the radius of the light, the influence is 0
            if (distance > radius)
                return 0;

            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 d = new Vector3(1, distance, (distance * distance));
            //Multiply the intensity by the attenuation
            return intensity / Vector3.Dot(d, attenuation);
        }

        public override Vector3 generateRandomDirection()
        {
            //http://mathworld.wolfram.com/SpherePointPicking.html
            double theta = 2 * Math.PI * nextRandom();
            double phi = Math.Acos(2 * nextRandom() - 1);

            Vector3 randomDirection = new Vector3((float)(Math.Cos(theta) * Math.Sin(phi)),
                                                  (float)(Math.Sin(theta) * Math.Sin(phi)),
                                                  (float)Math.Cos(phi));

            randomDirection.Normalize();
            return randomDirection;

        }
    }
}
