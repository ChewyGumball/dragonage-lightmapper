using System.Collections.Generic;

using OpenTK;

namespace Geometry
{
    public class Octree : TrianglePartitioner
    {
        public Octree[] children = new Octree[0];
        public BoundingBox bounds = new BoundingBox(new Vector3(), new Vector3());
        private Triangle[] tris;

        public int count { get; private set; }

        private static Vector3[] offsets =   {
                                                new Vector3(-0.5f,-0.5f,-0.5f),
                                                new Vector3(0.5f,-0.5f,-0.5f),
                                                new Vector3(-0.5f,0.5f,-0.5f),
                                                new Vector3(0.5f,0.5f,-0.5f),
                                                new Vector3(-0.5f,-0.5f,0.5f),
                                                new Vector3(0.5f,-0.5f,0.5f),
                                                new Vector3(-0.5f,0.5f,0.5f),
                                                new Vector3(0.5f,0.5f,0.5f)
                                            };

        public Octree(List<Triangle> triangles)
            : this(triangles, 25, 15, new BoundingBox(triangles))
        { }
        public Octree(List<Triangle> triangles, int maxTriangles, int maxDepth, BoundingBox box)
        {
            build(triangles, maxTriangles, maxDepth, box);
        }
        private void build(List<Triangle> triangles, int maxTriangles, int maxDepth, BoundingBox box)
        {
            bounds = box;

            if (triangles.Count <= maxTriangles || maxDepth == 0)
            {
                tris = triangles.ToArray();
                count = tris.Length;
            }
            else
            {
                children = new Octree[8];
                count = 0;
                BoundingBox newBox;
                List<Triangle> childrenTriangles;
                List<Triangle> used = new List<Triangle>();
                Vector3 lengths = (bounds.max - bounds.min) / 2;
                Vector3 halfLengths = lengths / 2;
                for (int i = 0; i < 8; i++)
                {
                    childrenTriangles = new List<Triangle>();
                    Vector3 ooffset = Vector3.Multiply(lengths, offsets[i]);
                    newBox = new BoundingBox(bounds.center + ooffset, halfLengths.X, halfLengths.Y, halfLengths.Z);
                    foreach (Triangle t in triangles)
                    {
                        if (newBox.triangleIntersects(t))
                        {
                            childrenTriangles.Add(t);
                        }
                    }
                    
                    children[i] = new Octree(childrenTriangles, maxTriangles, maxDepth - 1, newBox);
                    count += children[i].count;
                }
            }
        }

        public float intersection(Vector3 start, Vector3 direction, out Triangle intersectedTriangle)
        {
            float leastDistance = float.MaxValue;
            intersectedTriangle = null;

            //If the line doesn't go through this octree then it is unobstructed
            if (bounds.lineIntersects(start, direction))
            {
                if (children.Length == 0)
                {
                    foreach (Triangle t in tris)
                    {
                        Vector3 throwAway;
                        float nextDistance = t.intersection(start, direction, out throwAway);
                        if (nextDistance > 0 && nextDistance < leastDistance)
                        {
                            leastDistance = nextDistance;
                            intersectedTriangle = t;
                        }
                    }
                }
                else
                {
                    foreach (Octree o in children)
                    {
                        Triangle nextIntersection;
                        float nextDistance = o.intersection(start, direction, out nextIntersection);
                        if (nextDistance > 0 && nextDistance < leastDistance)
                        {
                            leastDistance = nextDistance;
                            intersectedTriangle = nextIntersection;
                        }
                    }
                }
            }

            return leastDistance != float.MaxValue ? leastDistance : -1.0f;
        }

        public void Clear()
        {
            foreach (Octree o in children)
            {
                o.Clear();
            }
            tris = new Triangle[0];
            count = 0;
        }
    }
}
