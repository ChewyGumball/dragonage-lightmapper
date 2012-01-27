using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace DALightmapper
{
    class BoundingBox
    {
        public Vector3 max { get; set;}
        public Vector3 min { get; set; }
        public Vector3 center { get; set; }

        //bounding box at the origin
        public BoundingBox(Vector3 maximum, Vector3 minimum)
        {
            max = maximum;
            min = minimum;
            center = new Vector3();
        }

        public BoundingBox(List<Triangle> triangles)
        {
            float minX = triangles[0].x.X;
            float minY = triangles[0].x.Y;
            float minZ = triangles[0].x.Z;
            float maxX = triangles[0].x.X;
            float maxY = triangles[0].x.Y;
            float maxZ = triangles[0].x.Z;

            foreach(Triangle t in triangles)
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
        }

        public BoundingBox(List<Photon> points)
        {
            float minX = points[0].position.X;
            float minY = points[0].position.Y;
            float minZ = points[0].position.Z;
            float maxX = points[0].position.X;
            float maxY = points[0].position.Y;
            float maxZ = points[0].position.Z;

            foreach (Photon v in points)
            {
                minX = Math.Min(v.position.X, minX);
                minY = Math.Min(v.position.Y, minY);
                minZ = Math.Min(v.position.Z, minZ);

                maxX = Math.Max(v.position.X, maxX);
                maxY = Math.Max(v.position.Y, maxY);
                maxZ = Math.Max(v.position.Z, maxZ);
            }

            max = new Vector3(maxX, maxY, maxZ);
            min = new Vector3(minX, minY, minZ);
            center = (max + min) / 2;
        }

        //Bounding box with offset and rotation from another bounding box
        public BoundingBox(BoundingBox bb, Vector3 offset, Quaternion rotation)
        {
            center = bb.center + offset;

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
                rotated[i] = Vector3.Transform(normal[i], rotation);
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
        }

        public BoundingBox(Vector3 c, float lengthX, float lengthY, float lengthZ)
        {
            center = c;
            max = center + new Vector3(lengthX, lengthY, lengthZ);
            min = center - new Vector3(lengthX, lengthY, lengthZ);
        }

        public Boolean lineIntersects(Vector3 start, Vector3 end)
        {
            //   (x1,y1,z1) + t(x2,y2,z2) = (x,y,z)
            //      x1 + tx2 = x
            //      t = (x-x1)/x2
            // if t <= 1 or t >= 0 
            //      if the point is on the bounding box return true

            Vector3 magnitude = end - start;
            Vector3 diffMin = min - start;
            Vector3 diffMax = max - start;
            float t, u, x, y, z, a, b, c;

            //x planes
            //If the magnitude is 0, parallel to the plane, considering this not an intersection
            //      as it will get picked up by the other planes if its on the bounding box
            if (magnitude.X != 0)
            {
                //t is for the min plane, u is for the max plane
                t = diffMin.X / magnitude.X;

                //This part is the same for both planes, 
                //  check if the intersection point is between the end points of the line
                if (t <= 1 && t >= 0)
                {
                    //Check the intersection point is on the bounding box
                    y = start.Y + t * magnitude.Y;
                    z = start.Z + t * magnitude.Z;

                    if ((y >= min.Y && y <= max.Y && z >= min.Z && z <= max.Z))
                    {
                        return true;
                    }
                }

                u = diffMax.X / magnitude.X;
                if (u <= 1 && u >= 0)
                {
                    b = start.Y + u * magnitude.Y;
                    c = start.Z + u * magnitude.Z;

                    if (b >= min.Y && b <= max.Y && c >= min.Z && c <= max.Z)
                    {
                        return true;
                    }
                }
            }

            //y planes
            if (magnitude.Y != 0)
            {
                t = diffMin.Y / magnitude.Y;
                if (t <= 1 && t >= 0)
                {
                    x = start.X + t * magnitude.X;
                    z = start.Z + t * magnitude.Z;

                    if (x >= min.X && x <= max.X && z >= min.Z && z <= max.Z)
                    {
                        return true;
                    }
                }

                u = diffMax.Y / magnitude.Y;
                if (u <= 1 && u >= 0)
                {
                    a = start.X + u * magnitude.X;
                    c = start.Z + u * magnitude.Z;

                    if (a >= min.X && a <= max.X && c >= min.Z && c <= max.Z)
                    {
                        return true;
                    }
                }
            }

            //z planes
            if (magnitude.Z != 0)
            {
                t = diffMin.Z / magnitude.Z;
                if (t <= 1 && t >= 0)
                {
                    x = start.X + t * magnitude.X;
                    y = start.Y + t * magnitude.Y;


                    if (x >= min.X && x <= max.X && y >= min.Y && y <= max.Y) 
                    {
                        return true;
                    }
                }

                u = diffMax.Z / magnitude.Z;
                if (u <= 1 && u >= 0)
                {
                    a = start.X + u * magnitude.X;
                    b = start.Y + u * magnitude.Y;

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
            Vector3 topA = max;
            Vector3 topB = new Vector3(max.X,min.Y,max.Z);
            Vector3 topC = new Vector3(min.X,min.Y,max.Z);
            Vector3 topD = new Vector3(min.X,max.Y,max.Z);

            Vector3 bottomA = new Vector3(max.X,max.Y,min.Z);
            Vector3 bottomB = new Vector3(max.X,min.Y,min.Z);
            Vector3 bottomC = min;
            Vector3 bottomD = new Vector3(min.X,max.Y,min.Z);

            return (containsPoint(t.x) || containsPoint(t.y) || containsPoint(t.z)) || // Triangle vertex is inside box
                   (lineIntersects(t.x, t.y) || lineIntersects(t.y, t.z) || lineIntersects(t.z, t.x))  || // Triangle edge intersects box
                   (t.lineIntersects(topA,topB) || t.lineIntersects(topB,topC) || t.lineIntersects (topC,topD) || t.lineIntersects(topD,topA)) || // Top edges of box intersect triangle
                   (t.lineIntersects(bottomA,bottomB) || t.lineIntersects(bottomB,bottomC) || t.lineIntersects (bottomC,bottomD) || t.lineIntersects(bottomD,bottomA)) || // Bottom edges of box intersect triangle
                   (t.lineIntersects(topA,bottomA) || t.lineIntersects(topB,bottomB) || t.lineIntersects(topC, bottomC) || t.lineIntersects(topD,bottomD)); //Middle edges of box intersect triangle */
        }

        public Boolean containsPoint(Vector3 p)
        {
            return lessThanOrEqual(p, max) && lessThanOrEqual(p, max) &&
                   greaterThanOrEqual(p, min) && greaterThanOrEqual(p, min);
        }

        public Boolean containsLine(Vector3 start, Vector3 end)
        {
            return containsPoint(start) && containsPoint(end);
        }

        public Boolean containsTriangle(Triangle t)
        {
            return containsPoint(t.x) && containsPoint(t.y) && containsPoint(t.z);
        }
        
        public Boolean sphereIntersect(Vector3 point, double distance)
        {
            BoundingBox b = new BoundingBox(point, (float)distance, (float)distance, (float)distance);
            return boxIntersects(b);
        }

        public Boolean boxIntersects(BoundingBox b)
        {
            Vector3 topA = max;
            Vector3 topB = new Vector3(max.X, min.Y, max.Z);
            Vector3 topC = new Vector3(min.X, min.Y, max.Z);
            Vector3 topD = new Vector3(min.X, max.Y, max.Z);

            Vector3 bottomA = new Vector3(max.X, max.Y, min.Z);
            Vector3 bottomB = new Vector3(max.X, min.Y, min.Z);
            Vector3 bottomC = min;
            Vector3 bottomD = new Vector3(min.X, max.Y, min.Z);

            Vector3 otherTopA = b.max;
            Vector3 otherTopB = new Vector3(b.max.X, b.min.Y, b.max.Z);
            Vector3 otherTopC = new Vector3(b.min.X, b.min.Y, b.max.Z);
            Vector3 otherTopD = new Vector3(b.min.X, b.max.Y, b.max.Z);

            Vector3 otherBottomA = new Vector3(b.max.X, b.max.Y, b.min.Z);
            Vector3 otherBottomB = new Vector3(b.max.X, b.min.Y, b.min.Z);
            Vector3 otherBottomC = b.min;
            Vector3 otherBottomD = new Vector3(b.min.X, b.max.Y, b.min.Z);

            return b.containsPoint(topA) || b.containsPoint(topB) || b.containsPoint(topC) || b.containsPoint(topD) ||
                    b.containsPoint(bottomA) || b.containsPoint(bottomB) || b.containsPoint(bottomC) || b.containsPoint(bottomD) ||
                    containsPoint(otherTopA) || containsPoint(otherTopB) || b.containsPoint(otherTopC) || b.containsPoint(otherTopD) ||
                    containsPoint(otherBottomA) || containsPoint(otherBottomB) || b.containsPoint(otherBottomC) || b.containsPoint(otherBottomD);
        }

        private Boolean lessThanOrEqual(Vector3 a, Vector3 b)
        {
            return a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z;
        }
        private Boolean greaterThanOrEqual(Vector3 a, Vector3 b)
        {
            return a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z;
        }
    }
}
