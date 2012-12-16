using OpenTK;

namespace Geometry
{
    public interface TrianglePartitioner
    {
        float intersection(Vector3 start, Vector3 direction, out Triangle intersectedTriangle);
        void Clear();
    }
}
