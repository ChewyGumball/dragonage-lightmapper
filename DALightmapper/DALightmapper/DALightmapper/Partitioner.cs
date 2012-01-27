using System.Collections.Generic;

using OpenTK;

namespace DALightmapper
{
    interface Partitioner
    {
        void build(List<Triangle> triangles, int maxTriangles, BoundingBox box);
        Triangle firstIntersection(Vector3 start, Vector3 direction);
        bool lineIsUnobstructed(Vector3 start, Vector3 end);
        List<Photon> getWithinDistanceSquared(Vector3 point, double distance);
    }
}
