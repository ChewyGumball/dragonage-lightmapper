using OpenTK;
using System;

using Geometry;
using System.Collections.Generic;

namespace Bioware.Structs
{
    public class SpotLight : Light
    {
        Vector3 direction { get; set; }
        float innerAngle { get; set; }
        float outerAngle { get; set; }
        float distance { get; set; }

        private static Vector3[] axes = { new Vector3(1,0,0), new Vector3(0,1,0), new Vector3(0,0,1) };


        public SpotLight(Vector3 pos, Vector3 lookAt, Vector3 col, Vector3 shadowCol, float intense, float inAngle, float outAngle, float dis, Boolean shoots)
            : base(pos, col, shadowCol, intense, shoots)
        {
            direction = lookAt - pos;
            innerAngle = inAngle;
            outerAngle = outAngle;
            distance = dis;
        }

        public override float influence(float distance)
        {
            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 d = new Vector3(1, distance, (distance * distance));
            //Multiply the intensity by the attenuation
            return intensity / Vector3.Dot(d, attenuation);
        }

        public override Vector3 generateRandomDirection()
        {
            //Find component closest to zero in direction
            float diff = Math.Abs(direction.X);
            Vector3 perpAxis = axes[0];
            if (Math.Abs(direction.Y) < diff)
            {
                diff = Math.Abs(direction.Y);
                perpAxis = axes[1];
            }
            if (Math.Abs(direction.Z) < diff)
            {
                diff = Math.Abs(direction.Z);
                perpAxis = axes[2];
            }

            //Find a perpendicular vector
            Vector3 perp = Vector3.Cross(direction, perpAxis);

            float randomAngleA = (float)(outerAngle * nextRandom());
            float randomAngleB = (float)(Math.PI * 2 * nextRandom());

            Vector3 dir = Vector3.Transform(direction, Matrix4.CreateFromAxisAngle(perp, randomAngleA));

            return Vector3.Transform(dir, Matrix4.CreateFromAxisAngle(perp, randomAngleB));
        } 
    }
}
