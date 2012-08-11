using OpenTK;
using System;

using Geometry;

namespace Bioware.Structs
{
    public class SpotLight : Light
    {
        Vector3 direction { get; set; }
        float innerAngle { get; set; }
        float outerAngle { get; set; }
        float distance { get; set; }

        private static Vector3[] axes = { new Vector3(1,0,0), new Vector3(0,1,0), new Vector3(0,0,1) };


        public SpotLight(Vector3 pos, Quaternion rot, Vector3 col, Vector3 shadowCol, float intense, float inAngle, float outAngle, float dis, Boolean shoots)
            : base(pos, col, shadowCol, intense, shoots)
        {
            direction = rot.ToAxisAngle().Xyz;
            innerAngle = inAngle;
            outerAngle = outAngle;
            distance = dis;
        }

        public override float influence(Vector3 patch)
        {
            //Find vector from light to point
            Vector3 lightToPoint = Vector3.Subtract(patch, position);
            //Find the angle between direction vector and above vector, only positive angle needed
            float angle = Math.Abs(Vector3.Dot(direction, lightToPoint) / lightToPoint.LengthFast);
            //When outside the outside cone, no influence
            if (angle > outerAngle)
                return 0;

            //Find the distance from edge of full intensity boundary
            float dis = lightToPoint.LengthFast - distance;
            //If the distance is less than the full intensity boundary, make it 1 so its actually full intensity
            if (dis < 1)
                dis = 1;

            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 reach = new Vector3(1, 1 / dis, 1 / (dis * dis));

            //When inside the inner cone, full intensity * attenuation.
            //When between inner cone and outter cone, linear falloff*intensity*attenuation
            return (1 - (angle - innerAngle) / (outerAngle - innerAngle)) * intensity * Vector3.Dot(reach, attenuation);
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
