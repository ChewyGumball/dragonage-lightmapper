﻿using System;
using OpenTK;

using Geometry;

namespace Bioware.Structs
{
    public class AmbientLight : Light
    {
        public AmbientLight(Vector3 pos, Vector3 col, Vector3 shadowCol, float intense, Boolean shoots)
            : base(pos, col, shadowCol, intense, shoots )
        { }
        public override float influence(float distance)
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
