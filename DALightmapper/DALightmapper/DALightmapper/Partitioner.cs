using System.Collections.Generic;

using Geometry;
using OpenTK;

namespace DALightmapper
{
    public interface Partitioner
    {
        float intersection(Vector3 start, Vector3 direction, out Triangle intersectedTriangle);
        void getWithinDistance(Vector3 point, float distance, ref List<Photon> photons);
        void getNearest(Vector3 point, int n, ref List<Photon> photons);
        void Clear();
    }
}
