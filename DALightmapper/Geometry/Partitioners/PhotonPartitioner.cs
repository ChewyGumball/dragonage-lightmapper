using System.Collections.Generic;

using OpenTK;

namespace Geometry
{
    public interface PhotonPartitioner
    {
        List<Photon> nearest(int number, Vector3 point);
    }
}
