using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace DALightmapper
{
    class PatchInstance
    {
        public Boolean isActive { get { return _original.isActive; } }
        Patch _original;
        public Vector3 position { get; private set; }
        public Vector3 normal { get; private set; }

        public Vector3 incidentLight { get; set; }
        public Vector3 excidentLight { get; private set; }

        public PatchInstance(Patch p, Vector3 offset, Quaternion rotation)
        {
            _original = p;

            if (_original.isActive)
            {
                position = Vector3.Transform(p.position, rotation) + offset;
                normal = Vector3.Transform(p.normal, rotation);
            }
        }
    }
}
