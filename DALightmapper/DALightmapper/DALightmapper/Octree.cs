using System;
using System.Collections.Generic;
using OpenTK;

namespace DALightmapper
{
    class Octree : Partitioner
    {
        private Octree[] children;
        private BoundingBox bounds;
        private List<Triangle> tris;

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
            build(triangles, 5, new BoundingBox(triangles));
        }


        public Octree(List<Triangle> triangles, int maxTriangles, BoundingBox box)
        {
            build(triangles, maxTriangles, box);
        }

        public void build(List<Triangle> triangles, int maxTriangles, BoundingBox box)
        {
            bounds = box;
            if (triangles.Count <= maxTriangles)
            {
                tris = triangles;
                children = new Octree[0];
            }
            else
            {
                tris = new List<Triangle>();
                children = new Octree[8];
                BoundingBox newBox;
                List<Triangle> childrenTriangles;
                Vector3 lengths = (bounds.max - bounds.min) /2;
                Vector3 halfLengths = lengths / 2;
                for (int i = 0; i < 8; i++)
                {
                    childrenTriangles = new List<Triangle>();
                    Vector3 ooffset = Vector3.Multiply(lengths, offsets[i]);
                    newBox = new BoundingBox(bounds.center + ooffset, halfLengths.X,halfLengths.Y, halfLengths.Z);
                    foreach (Triangle t in triangles)
                    {
                        if (newBox.containsTriangle(t))
                        {
                            childrenTriangles.Add(t);
                            //triangles.Remove(t);
                        }
                    }
                    children[i] = new Octree(childrenTriangles, maxTriangles, newBox);
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
    }
}
