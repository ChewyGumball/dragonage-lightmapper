using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace Geometry
{
    public class BoundingBox
    {
        public Vector3 max { get; private set; }
        public Vector3 min { get; private set; }
        public Vector3 center { get; private set; }

        private Vector3 topB, topC, topD, bottomA, bottomB, bottomD;

        //bounding box at the origin
        public BoundingBox(Vector3 maximum, Vector3 minimum)
        {
            max = maximum;
            min = minimum;
            center = new Vector3();
            calculateOtherPoints();
        }

        public BoundingBox(List<Triangle> triangles)
        {
            float minX = triangles[0].x.X;
            float minY = triangles[0].x.Y;
            float minZ = triangles[0].x.Z;
            float maxX = triangles[0].x.X;
            float maxY = triangles[0].x.Y;
            float maxZ = triangles[0].x.Z;

            foreach (Triangle t in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    minX = Math.Min(t[i].X, minX);
                    minY = Math.Min(t[i].Y, minY);
                    minZ = Math.Min(t[i].Z, minZ);

                    maxX = Math.Max(t[i].X, maxX);
                    maxY = Math.Max(t[i].Y, maxY);
                    maxZ = Math.Max(t[i].Z, maxZ);
                }
            }

            max = new Vector3(maxX, maxY, maxZ);
            min = new Vector3(minX, minY, minZ);
            center = (max + min) / 2;
            calculateOtherPoints();
        }

        public BoundingBox(List<Vector3> points)
        {
            if (points.Count > 0)
            {
                float minX = points[0].X;
                float minY = points[0].Y;
                float minZ = points[0].Z;
                float maxX = points[0].X;
                float maxY = points[0].Y;
                float maxZ = points[0].Z;

                foreach (Vector3 v in points)
                {
                    minX = Math.Min(v.X, minX);
                    minY = Math.Min(v.Y, minY);
                    minZ = Math.Min(v.Z, minZ);

                    maxX = Math.Max(v.X, maxX);
                    maxY = Math.Max(v.Y, maxY);
                    maxZ = Math.Max(v.Z, maxZ);
                }

                max = new Vector3(maxX, maxY, maxZ);
                min = new Vector3(minX, minY, minZ);
                center = (max + min) / 2;
            }
            else
            {
                max = new Vector3();
                min = new Vector3();
                center = new Vector3();
            }
            calculateOtherPoints();
        }

        //Bounding box with offset and rotation from another bounding box
        public BoundingBox(BoundingBox bb, Matrix4 transform)
        {
            center = bb.center + transform.Row3.Xyz;

            //make the 8 corners, rotate them, then find the max and min for each axis

            Vector3[] normal = new Vector3[8];
            //Top
            normal[0] = max;
            normal[1] = new Vector3(max.X, min.Y, max.Z);
            normal[2] = new Vector3(min.X, max.Y, max.Z);
            normal[3] = new Vector3(min.X, min.Y, max.Z);
            //Bottom
            normal[4] = min;
            normal[5] = new Vector3(min.X, max.Y, min.Z);
            normal[6] = new Vector3(max.X, min.Y, min.Z);
            normal[7] = new Vector3(max.X, max.Y, min.Z);

            //Rotate all the vectors
            Vector3[] rotated = new Vector3[8];
            for (int i = 0; i < rotated.Length; i++)
            {
                rotated[i] = Vector3.Transform(normal[i], transform) - transform.Row3.Xyz;
            }

            //find the min and max of the rotated corners
            float minX = rotated[0].X;
            float minY = rotated[0].Y;
            float minZ = rotated[0].Z;
            float maxX = rotated[0].X;
            float maxY = rotated[0].Y;
            float maxZ = rotated[0].Z;

            for (int i = 1; i < rotated.Length; i++)
            {
                float x = rotated[i].X;
                float y = rotated[i].Y;
                float z = rotated[i].Z;

                minX = Math.Min(x, minX);
                minY = Math.Min(y, minY);
                minZ = Math.Min(z, minZ);

                maxX = Math.Max(x, maxX);
                maxY = Math.Max(y, maxY);
                maxZ = Math.Max(z, maxZ);
            }

            max = new Vector3(maxX, maxY, maxZ);
            min = new Vector3(minX, minY, minZ);
            calculateOtherPoints();
        }

        public BoundingBox(Vector3 c, float lengthX, float lengthY, float lengthZ)
        {
            center = c;
            max = center + new Vector3(lengthX, lengthY, lengthZ);
            min = center - new Vector3(lengthX, lengthY, lengthZ);
            calculateOtherPoints();
        }

        public Boolean lineIntersects(Vector3 start, Vector3 direction)
        {
            if (containsPoint(start))
            {
                return true;
            }
            //   (x1,y1,z1) + t(x2,y2,z2) = (x,y,z)
            //      x1 + tx2 = x
            //      t = (x-x1)/x2
            // if t <= 1 or t >= 0 
            //      if the point is on the bounding box return true

            Vector3 diffMin = min - start;
            Vector3 diffMax = max - start;

            //x planes
            //If the magnitude is 0, parallel to the plane, considering this not an intersection
            //      as it will get picked up by the other planes if its on the bounding box
            if (direction.X != 0)
            {
                //t is for the min plane, u is for the max plane
                float t = diffMin.X / direction.X;

                //This part is the same for both planes, 
                //  check if the intersection point is between the end points of the line
                if (t <= 1 && t >= 0)
                {
                    //Check the intersection point is on the bounding box
                    float y = start.Y + t * direction.Y;
                    float z = start.Z + t * direction.Z;

                    if ((y >= min.Y && y <= max.Y && z >= min.Z && z <= max.Z))
                    {
                        return true;
                    }
                }

                float u = diffMax.X / direction.X;
                if (u <= 1 && u >= 0)
                {
                    float b = start.Y + u * direction.Y;
                    float c = start.Z + u * direction.Z;

                    if (b >= min.Y && b <= max.Y && c >= min.Z && c <= max.Z)
                    {
                        return true;
                    }
                }
            }

            //y planes
            if (direction.Y != 0)
            {
                float t = diffMin.Y / direction.Y;
                if (t <= 1 && t >= 0)
                {
                    float x = start.X + t * direction.X;
                    float z = start.Z + t * direction.Z;

                    if (x >= min.X && x <= max.X && z >= min.Z && z <= max.Z)
                    {
                        return true;
                    }
                }

                float u = diffMax.Y / direction.Y;
                if (u <= 1 && u >= 0)
                {
                    float a = start.X + u * direction.X;
                    float c = start.Z + u * direction.Z;

                    if (a >= min.X && a <= max.X && c >= min.Z && c <= max.Z)
                    {
                        return true;
                    }
                }
            }

            //z planes
            if (direction.Z != 0)
            {
                float t = diffMin.Z / direction.Z;
                if (t <= 1 && t >= 0)
                {
                    float x = start.X + t * direction.X;
                    float y = start.Y + t * direction.Y;


                    if (x >= min.X && x <= max.X && y >= min.Y && y <= max.Y)
                    {
                        return true;
                    }
                }

                float u = diffMax.Z / direction.Z;
                if (u <= 1 && u >= 0)
                {
                    float a = start.X + u * direction.X;
                    float b = start.Y + u * direction.Y;

                    if (a >= min.X && a <= max.X && b >= min.Y && b <= max.Y)
                    {
                        return true;
                    }
                }
            }

            //No intersection with any of the planes
            return false;
        }

        public Boolean triangleIntersects(Triangle t)
        {
            //Separating axis theorem
            return !((t.x.X > max.X && t.y.X > max.X && t.z.X > max.X) ||   //Max X plane
                     (t.x.Y > max.Y && t.y.Y > max.Y && t.z.Y > max.Y) ||   //Max Y plane
                     (t.x.Z > max.Z && t.y.Z > max.Z && t.z.Z > max.Z) ||   //Max Z plane
                     (t.x.X < min.X && t.y.X < min.X && t.z.X < min.X) ||   //Min X plane
                     (t.x.Y < min.Y && t.y.Y < min.Y && t.z.Y < min.Y) ||   //Min Y plane
                     (t.x.Z < min.Z && t.y.Z < min.Z && t.z.Z < min.Z) ||   //Min Z plane
                      allOnSameSideOfTriangle(t));                          //Triangle plane
        }

        private bool allOnSameSideOfTriangle(Triangle t)
        {
            bool topAPositive = Vector3.Dot(t.normal, max - t.x) > 0;
            bool topBPositive = Vector3.Dot(t.normal, topB - t.x) > 0;
            bool topCPositive = Vector3.Dot(t.normal, topC - t.x) > 0;
            bool topDPositive = Vector3.Dot(t.normal, topD - t.x) > 0;
            bool bottomAPositive = Vector3.Dot(t.normal, bottomA - t.x) > 0;
            bool bottomBPositive = Vector3.Dot(t.normal, bottomB - t.x) > 0;
            bool bottomCPositive = Vector3.Dot(t.normal, min - t.x) > 0;
            bool bottomDPositive = Vector3.Dot(t.normal, bottomD - t.x) > 0;

            bool allPositive = topAPositive && topBPositive && topCPositive && topDPositive &&
                               bottomAPositive && bottomBPositive && bottomCPositive && bottomDPositive;
            bool allNegative = !(topAPositive || topBPositive || topCPositive || topDPositive ||
                                 bottomAPositive || bottomBPositive || bottomCPositive || bottomDPositive);

            return allPositive || allNegative;
        }

        public Boolean containsPoint(Vector3 p)
        {
            return lessThanOrEqual(p, max) && greaterThanOrEqual(p, min);
        }
        public Boolean containsLine(Vector3 start, Vector3 end)
        {
            return containsPoint(start) && containsPoint(end);
        }
        public Boolean containsTriangle(Triangle t)
        {
            return containsPoint(t.x) && containsPoint(t.y) && containsPoint(t.z);
        }
        public Boolean sphereIntersect(Vector3 point, float distance)
        {
            return !(min.X > (point.X + distance) || min.Y > (point.Y + distance) || min.Z > (point.Z + distance) ||
                    (point.X - distance) > max.X || (point.Y - distance) > max.Y || (point.Z - distance) > max.Z);
        }
        public Boolean boxIntersects(BoundingBox b)
        {
            return !(min.X > b.max.X || min.Y > b.max.Y || min.Z > b.max.Z || b.min.X > max.X || b.min.Y > max.Y || b.min.Z > max.Z);
        }

        private Boolean lessThanOrEqual(Vector3 a, Vector3 b)
        {
            return a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z;
        }
        private Boolean greaterThanOrEqual(Vector3 a, Vector3 b)
        {
            return a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z;
        }
        private void calculateOtherPoints()
        {
            topB = new Vector3(max.X, min.Y, max.Z);
            topC = new Vector3(min.X, min.Y, max.Z);
            topD = new Vector3(min.X, max.Y, max.Z); 
            bottomA = new Vector3(max.X, max.Y, min.Z);
            bottomB = new Vector3(max.X, min.Y, min.Z);
            bottomD = new Vector3(min.X, max.Y, min.Z);
        }
    }
}
