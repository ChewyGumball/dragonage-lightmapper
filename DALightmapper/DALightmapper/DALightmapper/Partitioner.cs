using System.Collections.Generic;

using Geometry;
using OpenTK;

namespace DALightmapper
{
    public interface Partitioner
    {
        Triangle firstIntersection(Vector3 start, Vector3 direction);
        bool lineIsUnobstructed(Vector3 start, Vector3 end);
        void getWithinDistance(Vector3 point, float distance, ref List<Photon> photons);
        void getNearest(Vector3 point, int n, ref List<Photon> photons);
        void Clear();
    }
}
