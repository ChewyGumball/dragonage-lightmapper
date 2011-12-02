using OpenTK;
using System;

namespace DALightmapper
{
    class SpotLight : Light
    {
        Vector3 _direction;
        float _innerAngle;
        float _outerAngle;
        float _distance;

        Vector3 direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
        float innerAngle
        {
            get { return _innerAngle; }
            set { _innerAngle = value; }
        }
        float outerAngle
        {
            get { return _outerAngle; }
            set { _outerAngle = value; }
        }
        float distance
        {
            get { return _distance; }
            set { _distance = value; }
        }


        public SpotLight(Vector3 pos, Quaternion rot, Vector3 col, float intense, float inAngle, float outAngle, float dis, LightType t)
            : base(pos, col, intense, t)
        {
            direction = rot.ToAxisAngle().Xyz;
            innerAngle = inAngle;
            outerAngle = outAngle;
            distance = dis;
        }

        public override float influence(Patch patch)
        {
            //Find vector from light to point
            Vector3 lightToPoint = Vector3.Subtract(patch.position, position);
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
    }
}
