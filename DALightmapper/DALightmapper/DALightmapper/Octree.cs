using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;

using Geometry;

namespace DALightmapper
{
    public class Octree : Partitioner
    {
        public Octree[] children = new Octree[0];
        public BoundingBox bounds = new BoundingBox(new Vector3(), new Vector3());
        private List<Triangle> tris = new List<Triangle>();
        private List<Photon> points = new List<Photon>();

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
        {
            build(triangles, 100, 15, new BoundingBox(triangles));
        }
        public Octree(List<Triangle> triangles, int maxTriangles, int maxDepth, BoundingBox box)
        {
            build(triangles, maxTriangles, maxDepth, box);
        }
        private void build(List<Triangle> triangles, int maxTriangles, int maxDepth, BoundingBox box)
        {
            bounds = box;
            if (triangles.Count <= maxTriangles || maxDepth == 0)
            {
                tris = triangles;
                count = tris.Count;
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

        public Octree(List<Photon> p)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (Photon photon in p)
            {
                points.Add(photon.position);
            }
            build(p, 100, 8, new BoundingBox(points));
        }
        public Octree(List<Photon> p, int maxPoints, int maxDepth, BoundingBox box)
        {
            build(p, maxPoints, maxDepth, box);
        }
        private void build(List<Photon> p, int maxPoints, int maxDepth, BoundingBox box)
        {
            bounds = box;
            if (p.Count <= maxPoints || maxDepth == 0)
            {
                points = p;
                count = points.Count;
            }
            else
            {
                children = new Octree[8];
                count = 0;
                BoundingBox newBox;
                List<Photon> childrenPoints;
                Vector3 lengths = (bounds.max - bounds.min) / 2;
                Vector3 halfLengths = lengths / 2;
                for (int i = 0; i < 8; i++)
                {
                    childrenPoints = new List<Photon>();
                    Vector3 offset = Vector3.Multiply(lengths, offsets[i]);
                    newBox = new BoundingBox(bounds.center + offset, halfLengths.X, halfLengths.Y, halfLengths.Z);
                    foreach (Photon point in p)
                    {
                        if (newBox.containsPoint(point.position))
                        {
                            childrenPoints.Add(point);
                            //triangles.Remove(t);
                        }
                    }
                    children[i] = new Octree(childrenPoints, maxPoints, maxDepth - 1, newBox);
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

        public void getWithinDistance(Vector3 point, float distance, ref List<Photon> photons)
        {
            if (bounds.sphereIntersect(point, distance))
            {
                if (children.Length == 0)
                {
                    float distanceSquared = distance * distance;
                    foreach (Photon p in points)
                    {
                        Vector3 diff = p.position - point;
                        if (diff.LengthSquared <= distanceSquared)
                        {
                            photons.Add(p);
                        }
                    }
                }
                else
                {
                    foreach (Octree o in children)
                    {
                        o.getWithinDistance(point, distance, ref photons);
                    }
                }
            }
        }
        public void getNearest(Vector3 point, int n, ref List<Photon> photons)
        {
            //If we are a leaf
            if (children.Length == 0)
            {
                //See if any of our photons are closer than what is already in the list
                photons.AddRange(points);
                photons.OrderBy(s => (s.position - point).LengthSquared);
                if (photons.Count > n)
                {
                    photons.RemoveRange(n, photons.Count - n);
                }
            }
            //Else if this is not a leaf 
            else
            {
                //If the point is in this bounding box then we need to check the child containing one
                if (bounds.containsPoint(point))
                {
                    //If the child node that contains the point has >= n points in it then just use that node, 
                    //  otherwise need need to query all the children to get the n nearest points
                    foreach (Octree o in children)
                    {
                        if (o.bounds.containsPoint(point) && o.count >= n)
                        {
                            o.getNearest(point, n, ref photons);
                            return;
                        }
                    }
                }

                //If we got here we need to check all the children because the containing box doesn't have enough points
                foreach (Octree o in children)
                {
                    o.getNearest(point, n, ref photons);
                }
            }
        }

        public void Clear()
        {
            foreach (Octree o in children)
            {
                o.Clear();
            }
            tris.Clear();
            points.Clear();
            count = 0;
        }
    }
}
