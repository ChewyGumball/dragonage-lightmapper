using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Geometry
{
    public class Triangle
    {
        private Vector3[] _mVerts;
        private Vector2[] _tVerts;
        private Vector2[] _lVerts;
        private Vector3 _normal;

        private Vector2[] lightMapInverseMatrix;
        private float determinant;
        private bool degenerate;

        public bool isDegenerate
        {
            get { return degenerate; }
        } 
        public bool isLightmapped { get; private set; }

        public Vector3 x
        {
            get { return _mVerts[0]; }
        }
        public Vector3 y
        {
            get { return _mVerts[1]; }
        }
        public Vector3 z
        {
            get { return _mVerts[2]; }
        }
        public Vector3 normal
        {
            get { return _normal; }
        }
        public Vector2 u
        {
            get { return _tVerts[0]; }
        }
        public Vector2 v
        {
            get { return _tVerts[1]; }
        }
        public Vector2 w
        {
            get { return _tVerts[2]; }
        }
        public Vector2 a
        {
            get { return _lVerts[0]; }
        }
        public Vector2 b
        {
            get { return _lVerts[1]; }
        }
        public Vector2 c
        {
            get { return _lVerts[2]; }
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
            calculateNormal();
            _tVerts = new Vector2[3] { t.u, t.v, t.w};
            lightMapInverseMatrix = new Vector2[2];
            if (t.isLightmapped)
            {
                isLightmapped = true;
                _lVerts = new Vector2[3] { t.a, t.b, t.c };
                calculateInverseMatrix();
                degenerate = areColinear(c - a, b - a);
            }
        }
        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w)
        {
            _mVerts = new Vector3[3] { x, y, z };
            _tVerts = new Vector2[3] { u, v, w };
            lightMapInverseMatrix = new Vector2[2];
            isLightmapped = false;
            degenerate = false;
            calculateNormal();
        }

        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w, Vector2 a, Vector2 b, Vector2 c)
            : this(x, y, z, u, v, w)
        {
            isLightmapped = true;
            _lVerts = new Vector2[3] { a, b, c };
            calculateInverseMatrix();
            degenerate = areColinear(c - a, b - a);
        }

        public bool isOnUVPixel(Vector2 topLeft, Vector2 bottomRight)
        {
            //Separating axis theorem
            return  !((bottomRight.X < a.X && bottomRight.X < b.X && bottomRight.X < c.X) ||    //Right of uv
                      (bottomRight.Y > a.Y && bottomRight.Y > b.Y && bottomRight.Y > c.Y) ||    //Below uv
                      (topLeft.X > a.X && topLeft.X > b.X && topLeft.X > c.X) ||                //Left of uv
                      (topLeft.Y < a.Y && topLeft.Y < b.Y && topLeft.Y < c.Y) ||                //Above uv
                      allOnPositiveSide(b, a, topLeft, bottomRight) ||                          //Triangle edges
                      allOnPositiveSide(c, b, topLeft, bottomRight) ||
                      allOnPositiveSide(a, c, topLeft, bottomRight));
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
            return ((convertedCoord.X >= 0f) && (convertedCoord.Y >= 0f) && (((float)(convertedCoord.X + convertedCoord.Y - 1)) <= 0.000001f));
        }

        //Find where the 2d input coordinates are on the 3d plane defined by this triangle
        public Vector3 uvTo3d(Vector2 topLeft, Vector2 bottomRight)
        {
            //Find the center of the box in uv space
            Vector2 center = convertToTextureBasis((topLeft + bottomRight) / 2);

            //if the length is > 1 then we are outside the triangle
            if (center.LengthSquared > 1)
            {
                center.Normalize();
            }

            return x + center.X * (y - x) + center.Y * (z - x);
        }

        public float intersection(Vector3 start, Vector3 direction, out Vector3 intersectionPoint)
        {
            Vector3 edge1 = y - x;
            Vector3 edge2 = z - x;

            float dot00 = Vector3.Dot(edge1, edge1);
            float dot01 = Vector3.Dot(edge1, edge2);
            float dot11 = Vector3.Dot(edge2, edge2);

            float distanceDenominator = Vector3.Dot(normal, direction);
            float baryDenominator = (dot00 * dot11) - (dot01 * dot01);

            //If this is a degenerate triangle baryDenominator will be 0
            //If the ray is parallel to the triangle, the distanceDenominator will be 0
            if (baryDenominator != 0 && distanceDenominator != 0)
            {
                float t = Vector3.Dot(normal, x - start) / distanceDenominator;

                //If the triangle is in the opposite direction, this value will be negative
                //  I don't count the ray starting on the triangle as an intersection with that triangle
                //  so only consider positive values
                if (t > 0)
                {
                    intersectionPoint = start + t * direction;
                    Vector3 pointEdge = intersectionPoint - x;

                    float dot02 = Vector3.Dot(edge1, pointEdge);
                    float dot12 = Vector3.Dot(edge2, pointEdge);

                    float u = (dot11 * dot02 - dot01 * dot12) / baryDenominator;
                    float v = (dot00 * dot12 - dot01 * dot02) / baryDenominator;

                    if ((u >= 0) && (v >= 0) && (u + v < 1))
                    {
                        return t;
                    }
                }
            }

            intersectionPoint = Vector3.Zero;
            return -1.0f;
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

        private bool allOnPositiveSide(Vector2 start, Vector2 end, Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);

            return onPositiveSide(start, end, topLeft) && onPositiveSide(start, end, bottomLeft) &&
                    onPositiveSide(start, end, topRight) && onPositiveSide(start, end, bottomRight);
        }
        private bool onPositiveSide(Vector2 start, Vector2 end, Vector2 p)
        {
            return (end.X - start.X) * (p.Y - start.Y) - (end.Y - start.Y) * (p.X - start.X) > 0;

            /*
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
            //*/
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

        private void calculateNormal()
        {
            Vector3 first = Vector3.Subtract(y, x);
            Vector3 second = Vector3.Subtract(z, x);
            _normal = Vector3.Cross(first, second);
            if (_normal.Length == 0)
                _normal = new Vector3(1, 0, 0);
            else
                _normal.Normalize();
        }
    }
}
