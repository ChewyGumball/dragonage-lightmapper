using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Geometry
{
    public class Triangle
    {
        Vector3[] _mVerts;
        Vector2[] _tVerts;
        Vector2[] _lVerts;
        Vector3 _normal;
        
        Vector2[] lightMapInverseMatrix;
        float determinant;

        public bool isDegenerate()
        {
            return areColinear(c - a, b - a);
        }

        public bool isLightmapped { get; private set; }

        public Vector3 x
        {
            get { return _mVerts[0]; }
            set
            {
                _mVerts[0] = value;
                calculateNormal();
            }
        }
        public Vector3 y
        {
            get { return _mVerts[1]; }
            set
            {
                _mVerts[1] = value;
                calculateNormal();
            }
        }
        public Vector3 z
        {
            get { return _mVerts[2]; }
            set
            {
                _mVerts[2] = value;
                calculateNormal();
            }
        }
        public Vector3 normal
        {
            get { return _normal; }
        }
        public Vector2 u
        {
            get { return _tVerts[0]; }
            set
            {
                _tVerts[0] = value;
            }
        }
        public Vector2 v
        {
            get { return _tVerts[1]; }
            set
            {
                _tVerts[1] = value;
            }
        }
        public Vector2 w
        {
            get { return _tVerts[2]; }
            set
            {
                _tVerts[2] = value;
            }
        }
        public Vector2 a
        {
            get { return _lVerts[0]; }
            set
            {
                _lVerts[0] = value;
                calculateInverseMatrix();
            }
        }
        public Vector2 b
        {
            get { return _lVerts[1]; }
            set
            {
                _lVerts[1] = value;
                calculateInverseMatrix();
            }
        }
        public Vector2 c
        {
            get { return _lVerts[2]; }
            set
            {
                _lVerts[2] = value;
                calculateInverseMatrix();
            }
        }

        public Vector3 this[int i]
        {
            get { return _mVerts[i]; }
        }

        public Triangle(Triangle t, Vector3 offset, Quaternion rotation)
        {
            Matrix4 transform = new Matrix4(new Vector4(Vector3.Transform(Vector3.UnitX, rotation), 0),
                                      new Vector4(Vector3.Transform(Vector3.UnitY, rotation), 0),
                                      new Vector4(Vector3.Transform(Vector3.UnitZ, rotation), 0),
                                      new Vector4(offset, 1));
            _mVerts = new Vector3[3] { Vector3.Transform(t.x, transform), Vector3.Transform(t.y, transform), Vector3.Transform(t.z, transform) };
            _tVerts = new Vector2[3] { t.u, t.v, t.w };
            lightMapInverseMatrix = new Vector2[2];
            calculateNormal();
            if (t.isLightmapped)
            {
                isLightmapped = true;
                _lVerts = new Vector2[3] { t.a, t.b, t.c };
                calculateInverseMatrix();
            }
        }
        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w)
        {
            _mVerts = new Vector3[3] {x,y,z};
            _tVerts = new Vector2[3] {u,v,w};
            lightMapInverseMatrix = new Vector2[2];
            calculateNormal();
            isLightmapped = false;
        }

        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w, Vector2 a, Vector2 b, Vector2 c)
            : this(x, y, z, u, v, w)
        {
            isLightmapped = true;
            _lVerts = new Vector2[3] {a,b,c};
            calculateInverseMatrix();
        }

        public bool isOnUVPixel(Vector2 topLeft, Vector2 bottomRight)
        {
            if (!isLightmapped)
            {
                //return false;
            }

            //Completely above, left, below, or right of the triangle
            if ((a.Y >= topLeft.Y && b.Y >= topLeft.Y && c.Y >= topLeft.Y) ||
               (a.X <= topLeft.X && b.X <= topLeft.X && c.X <= topLeft.X) ||
               (a.Y <= bottomRight.Y && b.Y <= bottomRight.Y && c.Y <= bottomRight.Y) ||
               (a.X >= bottomRight.X && b.X >= bottomRight.X && c.X >= bottomRight.X))
            {
                return false;
            }

            //if a corner of the triangle is on the pixel
            if (pointIsBetween(a, topLeft, bottomRight) ||
                pointIsBetween(b, topLeft, bottomRight) ||
                pointIsBetween(c, topLeft, bottomRight))
                return true;
            
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);

            //if a corner of the pixel is on the triangle 
            if (uvIsOnThisTriangle(topLeft) ||
                uvIsOnThisTriangle(bottomRight) ||
                uvIsOnThisTriangle(bottomLeft) ||
                uvIsOnThisTriangle(topRight))
                return true;

            //Could still be on the pixel if the triangle intersects with the 2 borders but not the corners
            // IE:  ___\____/___
            //     |____\__/____|
            //           \/
            //This could be optimized to check only three sides I THINK! but will leave it checking 4 unless a there is a problem
            if (intersects(a, b, topLeft, topRight) ||
                intersects(a, b, topLeft, bottomLeft) ||
                intersects(a, b, topRight, bottomRight) ||
                intersects(a, b, bottomLeft, bottomRight) ||
                intersects(a, c, topLeft, topRight) ||
                intersects(a, c, topLeft, bottomLeft) ||
                intersects(a, c, topRight, bottomRight) ||
                intersects(a, c, bottomLeft, bottomRight) ||
                intersects(c, b, topLeft, topRight) ||
                intersects(c, b, topLeft, bottomLeft) ||
                intersects(c, b, topRight, bottomRight) ||
                intersects(c, b, bottomLeft, bottomRight))
                return true;


            //if none of the above tests pass then this triangle is not on the pixel
            return false;
        }

        public bool uvIsOnThisTriangle(Vector2 coord)
        {
            if (!isLightmapped)
            {
                return false;
            }

            Vector2 convertedCoord = convertToTextureBasis(coord);
            //If the points are >= 0 and add up to <= 1 then its on the triangle.
            //   Remove the = to ignore the edges
            
            //Floating point precision errors can cause problems so extremely close to 1 is good enough here
            bool isOn = ((convertedCoord.X >= 0f) && (convertedCoord.Y >= 0f) && (((float)(convertedCoord.X + convertedCoord.Y - 1)) <= 0.000001f));
            return isOn;
        }

        //Find where the 2d input coordinates are on the 3d plane defined by this triangle
        public Vector3 uvTo3d(Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 coord = new Vector2();  //The uv coord of the center of the polygon the pixel overlaps with

            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);

            List<Vector2> points = new List<Vector2>();

            //A minimum of 3 points and a maximum of 7 points of the 19 tested below should be added to the list
            if (pointIsBetween(a, topLeft, bottomRight))
                points.Add(a);
            if (pointIsBetween(b, topLeft, bottomRight))
                points.Add(b);
            if (pointIsBetween(c, topLeft, bottomRight))
                points.Add(c);

            if (uvIsOnThisTriangle(topLeft))
                points.Add(topLeft);
            if (uvIsOnThisTriangle(bottomRight))
                points.Add(bottomRight);
            if (uvIsOnThisTriangle(bottomLeft))
                points.Add(bottomLeft);
            if (uvIsOnThisTriangle(topRight))
                points.Add(topRight);

            if (intersects(a, b, topLeft, topRight))
                points.Add(intersectionPoint(a, b, topLeft, topRight));
            if (intersects(a, b, topLeft, bottomLeft))
                points.Add(intersectionPoint(a, b, topLeft, bottomLeft));
            if (intersects(a, b, topRight, bottomRight))
                points.Add(intersectionPoint(a, b, topRight, bottomRight));
            if (intersects(a, b, bottomLeft, bottomRight))
                points.Add(intersectionPoint(a, b, bottomLeft, bottomRight));

            if (intersects(a, c, topLeft, topRight))
                points.Add(intersectionPoint(a, c, topLeft, topRight));
            if (intersects(a, c, topLeft, bottomLeft))
                points.Add(intersectionPoint(a, c, topLeft, bottomLeft));
            if (intersects(a, c, topRight, bottomRight))
                points.Add(intersectionPoint(a, c, topRight, bottomRight));
            if (intersects(a, c, bottomLeft, bottomRight))
                points.Add(intersectionPoint(a, c, bottomLeft, bottomRight));

            if (intersects(c, b, topLeft, topRight))
                points.Add(intersectionPoint(c, b, topLeft, topRight));
            if (intersects(c, b, topLeft, bottomLeft))
                points.Add(intersectionPoint(c, b, topLeft, bottomLeft));
            if (intersects(c, b, topRight, bottomRight))
                points.Add(intersectionPoint(c, b, topRight, bottomRight));
            if (intersects(c, b, bottomLeft, bottomRight))
                points.Add(intersectionPoint(c, b, bottomLeft, bottomRight));

            /*
            if (points.Count < 3 || points.Count > 7)
            {
                if (pointIsBetween(a, topLeft, bottomRight))
                    System.Console.WriteLine("A is on");
                if (pointIsBetween(b, topLeft, bottomRight))
                    System.Console.WriteLine("B is on");
                if (pointIsBetween(c, topLeft, bottomRight))
                    System.Console.WriteLine("C is on");

                if (uvIsOnThisTriangle(topLeft))
                    System.Console.WriteLine("topleft is on");
                if (uvIsOnThisTriangle(bottomRight))
                    System.Console.WriteLine("bottomRight is on");
                if (uvIsOnThisTriangle(bottomLeft))
                    System.Console.WriteLine("bottomleft is on");
                if (uvIsOnThisTriangle(topRight))
                    System.Console.WriteLine("topright is on");

                if (intersects(a, b, topLeft, topRight))
                    System.Console.WriteLine("a b intersects top");
                if (intersects(a, b, topLeft, bottomLeft))
                    System.Console.WriteLine("a b intersects left");
                if (intersects(a, b, topRight, bottomRight))
                    System.Console.WriteLine("a b intersects right");
                if (intersects(a, b, bottomLeft, bottomRight))
                    System.Console.WriteLine("a b intersects bottom");

                if (intersects(a, c, topLeft, topRight))
                    System.Console.WriteLine("a c intersects top");
                if (intersects(a, c, topLeft, bottomLeft))
                    System.Console.WriteLine("a c intersects left");
                if (intersects(a, c, topRight, bottomRight))
                    System.Console.WriteLine("a c intersects right");
                if (intersects(a, c, bottomLeft, bottomRight))
                    System.Console.WriteLine("a c intersects bottom");

                if (intersects(c, b, topLeft, topRight))
                    System.Console.WriteLine("c b intersects top");
                if (intersects(c, b, topLeft, bottomLeft))
                    System.Console.WriteLine("c b intersects left");
                if (intersects(c, b, topRight, bottomRight))
                    System.Console.WriteLine("c b intersects right");
                if (intersects(c, b, bottomLeft, bottomRight))
                    System.Console.WriteLine("c b intersects bottom");
                throw new Exception(String.Format("There are {0} points in the list. a={1} b={2} c={3} topLeft={4} bottomRight={5}", points.Count,a,b,c,topLeft,bottomRight));
            }
            //*/

            //Find the middle
            foreach (Vector2 v in points)
            {
                coord += v;
            }
            coord /= points.Count;

            //Find the coordinates in texture basis coordinates
            Vector2 convertedCoord = convertToTextureBasis(coord);

            //Find the direction vector pointing from the origin point to the input point
            Vector3 directionVector = Vector3.Add(Vector3.Multiply(Vector3.Subtract(y, x), convertedCoord.X),
                                                    Vector3.Multiply(Vector3.Subtract(z, x), convertedCoord.Y));

            //Add the direction vector to the origin point to get the point we need
            return directionVector + x;
        }

        public bool lineIntersects(Vector3 start, Vector3 end)
        {
            Vector3 edge1 = y - x;
            Vector3 edge2 = z - x;

            Vector3 direction = end - start;

            Vector3 crossed = Vector3.Cross(direction, edge2);

            float determinant = Vector3.Dot(edge1, crossed);

            if (determinant > 0)
            {
                Vector3 distance = start - x;
                float u = Vector3.Dot(distance, crossed);
                if (u > 0 && u < determinant)
                {
                    crossed = Vector3.Cross(distance, edge1);
                    float v = Vector3.Dot(direction, crossed);
                    if (v > 0 && (u + v) < determinant)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Vector3 lineIntersectionPoint(Vector3 start, Vector3 end)
        {
            /*
            if(!lineIntersects(start,end))
            {
                throw new Exception(String.Format("The line from {0} to {1} does not intersect this triangle ({2},{3},{4}).", start, end, x, y, z));
            }
            //*/
            //Find intersection of line and plane containing triangle
            float denominator = Vector3.Dot(normal, end - start);
            float numerator = Vector3.Dot(normal, x - start);
            float param = numerator / denominator;

            Vector3 intersection = start + param * (end - start);

            return intersection;
                
        }

        private bool pointIsBetween(Vector2 a, Vector2 topLeft, Vector2 bottomRight)
        {
            if (!isLightmapped)
            {
                return false;
            }
            else
            {
                return a.X >= topLeft.X && a.X <= bottomRight.X && a.Y <= topLeft.Y && a.Y >= bottomRight.Y;
            }
        }

        private Vector2 convertToTextureBasis(Vector2 coord)
        {

            //Find vector from origin to input point
            Vector2 point = Vector2.Subtract(coord, a);

            //Solve for the u and v values using the inverse matrix (matrix multiplication)
            float u = Vector2.Dot(lightMapInverseMatrix[0], point);
            float v = Vector2.Dot(lightMapInverseMatrix[1], point);

            return new Vector2(u, v);
        }

        private void calculateInverseMatrix()
        {
            //Calculate the basis matrix, this matrix will only be singular if the triangle is degenerate
            Vector2 firstCol = Vector2.Subtract(b, a);
            Vector2 secondCol = Vector2.Subtract(c, a);

            //Find the reciprocal determinant of the matrix
            determinant = 1 / (firstCol.X * secondCol.Y - secondCol.X * firstCol.Y);

            //calculate the inverse matrix
            lightMapInverseMatrix[0] = Vector2.Multiply(new Vector2(secondCol.Y, -secondCol.X), determinant);
            lightMapInverseMatrix[1] = Vector2.Multiply(new Vector2(-firstCol.Y, firstCol.X), determinant);

        }

        private void calculateNormal()
        {
            Vector3 a = Vector3.Subtract(y, x);
            Vector3 b = Vector3.Subtract(z, x);
            _normal = Vector3.Cross(a, b);
            if (_normal.Length == 0)
                _normal = new Vector3(1, 0, 0);
            else
                _normal.Normalize();
        }

        private bool intersects(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            Vector2 aDirection = aEnd - aStart;
            Vector2 bDirection = bEnd - bStart;
            Vector2 aTobStart = bStart - aStart;

            if (areColinear(aDirection, bDirection)) return false;

            float detDenom = aDirection.X * bDirection.Y - aDirection.Y * bDirection.X;

            if (detDenom == 0) return false;

            //Need to explicitly cast these otherwise it will be optimized into the comparasin below at higher than float precision 
            //  causing errors
            float t = (float)(1 / detDenom * (bDirection.Y * aTobStart.X - bDirection.X * aTobStart.Y));
            float u = (float)(-(1 / detDenom * (-aDirection.Y * aTobStart.X + aDirection.X * aTobStart.Y)));

            //Do not count the end points 
            return t > 0f && t < 1f && u > 0f && u < 1f;
        }

        private bool areColinear(Vector2 a, Vector2 b)
        {

            //if either point at (0,0) then there is no line as direction vector has 0 length
            if (b.X == 0 && b.Y == 0 ||
                a.X == 0 && a.Y == 0)
                return true;

            //if one component is zero, its matching component must not be 0
            if (a.X == 0 && b.X == 0 ||
                a.Y == 0 && b.Y == 0 ||
                b.X == 0 && a.X == 0 ||
                b.Y == 0 && a.Y == 0)
                return true;

            //If the ratio of the components is the same then they are colinear
            return (a.X / b.X == a.Y / b.Y);
        }

        private Vector2 intersectionPoint(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            Vector2 aDirection = aEnd - aStart;
            Vector2 bDirection = bEnd - bStart;
            Vector2 aTobStart = bStart - aStart;

            float detDenom = aDirection.X * bDirection.Y - aDirection.Y * bDirection.X;

            float t = 1 / detDenom * (bDirection.Y * aTobStart.X - bDirection.X * aTobStart.Y);
            return aStart + t * aDirection;
        }

        private bool pointsStraddleEdge(Vector2 start, Vector2 end, Vector2 topLeft, Vector2 bottomRight)
        {
            return true;
        }
    }
}
