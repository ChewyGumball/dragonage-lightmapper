using System;
using System.Collections.Generic;
using OpenTK;

namespace DALightmapper
{
    public class Octree : Partitioner
    {
        public Octree[] children;
        public BoundingBox bounds;
        public List<Triangle> tris;
        public List<Triangle> unused;
        private List<Photon> points;

        public int getUnused()
        {
            int childrenUnused = 0;
            if (children.Length > 0)
            {
                foreach (Octree o in children)
                    childrenUnused += o.getUnused();
            }
            return childrenUnused + unused.Count;
        }

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
        public void build(List<Triangle> triangles, int maxTriangles, int maxDepth, BoundingBox box)
        {
            bounds = box;
            if (triangles.Count <= maxTriangles || maxDepth == 0)
            {
                tris = triangles;
                unused = new List<Triangle>();
                children = new Octree[0];
            }
            else
            {
                tris = new List<Triangle>();
                children = new Octree[8];
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
                            if (!used.Contains(t))
                            {
                                used.Add(t);
                            }
                        }
                    }
                    children[i] = new Octree(childrenTriangles, maxTriangles, maxDepth - 1, newBox);
                }
                unused = new List<Triangle>();
                foreach (Triangle t in triangles)
                {
                    if (!used.Contains(t))
                        unused.Add(t);
                }
                if (unused.Count > 0)
                {
                    //throw new Exception("SHIT");
                    System.Console.WriteLine("There were {0} unused triangles.", unused.Count);
                }
            }
        }

        public Octree(List<Photon> p)
        {
            build(p, 100, 8, new BoundingBox(p));
        }
        public Octree(List<Photon> p, int maxPoints, int maxDepth, BoundingBox box)
        {
            build(p, maxPoints, maxDepth, box);
        }
        public void build(List<Photon> p, int maxPoints, int maxDepth, BoundingBox box)
        {
            bounds = box;
            if (p.Count <= maxPoints || maxDepth == 0)
            {
                points = p;
                children = new Octree[0];
            }
            else
            {
                points = new List<Photon>();
                children = new Octree[8];
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
                }
            }
        }

        public bool lineIsUnobstructed(Vector3 start, Vector3 end)
        {
            //If the line doesn't go through this octree then it is unobstructed
            if (!bounds.lineIntersects(start, end))
                return true;

            //If we are a leaf check intersection with our triangles
            if (tris.Count > 0)
            {
                foreach (Triangle t in tris)
                {
                    if (t.lineIntersects(start, end))
                    {
                        Vector3 intersection = t.lineIntersectionPoint(start, end);

                        //Dont want to count the start or end points because this will be used for sight calculations and the end points
                        //  are going to be on triangles and thus always intersect with this line segment
                        if (intersection != start && intersection != end)
                            return false;
                    }
                }

                //If no triangles intersect then the line is unobstructed
                return true;
            }

            //If we are not a leaf then check our children
            foreach (Octree o in children)
            {
                //If the line is obstructed in a child, return false
                if (!o.lineIsUnobstructed(start, end))
                    return false;
            }

            //The line was not obstructed in any child so return true
            return true;
        }

        public Triangle firstIntersection(Vector3 start, Vector3 end)
        {
            Triangle nearest = null;
            Vector3 nearPoint = end;
            //If the line doesn't go through this octree then it is unobstructed
            if (!bounds.lineIntersects(start, end))
                return null;

            //If we are a leaf check intersection with our triangles
            if (tris.Count > 0)
            {
                foreach (Triangle t in tris)
                {
                    if (t.lineIntersects(start, end))
                    {
                        Vector3 intersection = t.lineIntersectionPoint(start, end);

                        //Dont want to count the start or end points because this will be used for sight calculations and the end points
                        //  are going to be on triangles and thus always intersect with this line segment
                        if (intersection != start && intersection != end)
                            if ((intersection - start).LengthSquared < (nearPoint - start).LengthSquared)
                            {
                                nearPoint = intersection;
                                nearest = t;
                            }
                    }
                }

                //If no triangles intersect then the line is unobstructed
                return nearest;
            }

            //If we are not a leaf then check our children
            foreach (Octree o in children)
            {
                Triangle t = o.firstIntersection(start, end);
                if (t != null)
                {
                    Vector3 intersection = t.lineIntersectionPoint(start, end);
                    if ((intersection - start).LengthSquared < (nearPoint - start).LengthSquared)
                    {
                        nearPoint = intersection;
                        nearest = t;
                    }
                }
            }

            //The line was not obstructed in any child so return true
            return nearest;
        }

        public void getWithinDistanceSquared(Vector3 point, float distance, ref List<Photon> photons)
        {
            if (bounds.sphereIntersect(point, distance))
            {
                if (points.Count > 0)
                {
                    foreach (Photon p in points)
                    {
                        Vector3 diff = p.position - point;
                        if(diff.LengthSquared >= distance)
                        {
                            photons.Add(p);
                        }
                    }
                }
                else
                {
                    foreach (Octree o in children)
                    {
                        o.getWithinDistanceSquared(point, distance, ref photons);
                    }
                }
            }
        }
    }
}
