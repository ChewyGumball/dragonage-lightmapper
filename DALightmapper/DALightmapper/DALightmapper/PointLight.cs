﻿using OpenTK;
using System;

namespace DALightmapper
{
    class PointLight : Light
    {
        public PointLight(Vector3 pos, Vector3 col, float intense, LightType t, Boolean shoots)
            : base(pos, col, intense, t, shoots)
        { }
        public override float influence(Patch patch)
        {
            //Find the distance from the light
            float distance = Vector3.Subtract(position, patch.position).LengthFast;
            //Make a vector to use with constant/linear/quadratic attenuation
            Vector3 d = new Vector3(1, distance, (distance * distance));
            //Multiply the intensity by the attenuation
            return intensity / Vector3.Dot(d, attenuation);
        }

        public override Vector3 generateRandomDirection()
        {
            Vector3 temp = new Vector3((float)Ben.MathHelper.nextGaussian(), (float)Ben.MathHelper.nextGaussian(), (float)Ben.MathHelper.nextGaussian());
            temp.Normalize();
            return temp;

        }
    }
}
