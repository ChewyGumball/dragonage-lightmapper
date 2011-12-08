using System.Collections.Generic;

using OpenTK;

namespace DALightmapper
{
    interface Partitioner
    {
        void build(List<Triangle> triangles, int maxTriangles, BoundingBox box);
        bool lineIsUnobstructed(Vector3 start, Vector3 end);
    }
}
