using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Bioware.Structs;

namespace DALightmapper
{
    class Triangle
    {
        Vector3[] _mVerts;
        Vector2[] _tVerts;
        Vector2[] _lVerts;
        Vector3 _normal;
        uint textureID;
        uint normalTextureID;

        MeshChunk _chunk;

        Vector2[] lightMapInverseMatrix;
        float determinant;

        public bool isLightmapped {get; private set;} 

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
        public MeshChunk chunk
        {
            get { return _chunk; }
            set { _chunk = value; }
        }

        public Vector3 this[int i]
        {
            get { return _mVerts[i]; }
        }
        uint id
        {
            get { return textureID; }
            set { textureID = value; }
        }

        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w)
        {
            _mVerts = new Vector3[3];
            _tVerts = new Vector2[3];
            lightMapInverseMatrix = new Vector2[2];
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
            this.v = v;
            this.w = w;
            calculateNormal();
            isLightmapped = false;
        }

        public Triangle(Vector3 x, Vector3 y, Vector3 z, Vector2 u, Vector2 v, Vector2 w, Vector2 a, Vector2 b, Vector2 c)
            : this(x, y, z, u, v, w)
        {
            isLightmapped = true;
            _lVerts = new Vector2[3];
            _lVerts[0] = a;
            _lVerts[1] = b;
            _lVerts[2] = c;
            calculateInverseMatrix();
        }

        public bool isOnUVPixel(Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);
            
            //if a corner of the triangle is on the pixel
            if (pointIsBetween(a, topLeft, bottomRight) ||
                pointIsBetween(b, topLeft, bottomRight) ||
                pointIsBetween(c, topLeft, bottomRight))
                return true;  

            //if a corner of the pixel is on the triangle 
            if( uvIsOnThisTriangle(topLeft) || 
                uvIsOnThisTriangle(bottomRight) ||
                uvIsOnThisTriangle(bottomLeft) ||
                uvIsOnThisTriangle(topRight))
                return true;   

            //Could still be on the pixel if the triangle intersects with the 2 borders but not the corners
            // IE:  ___\____/___
            //     |____\__/____|
            //           \/
           
            //TODO: do this



            //if none of the above tests pass then this triangle is not on the pixel
            return false;
        }

        public bool uvIsOnThisTriangle(Vector2 coord)
        {
            Vector2 convertedCoord = convertToTextureBasis(coord);
            //If the points are >= 0 and add up to <= 1 then its on the triangle.
            //   Remove the = to ignore the edges
            return ((convertedCoord.X >= 0) && (convertedCoord.Y >= 0) && (convertedCoord.X + convertedCoord.Y <= 1));
        }

        //Find where the 2d input coordinates are on the 3d plane defined by this triangle
        public Vector3 uvTo3d(Vector2 coord)
        {
            if (uvIsOnThisTriangle(coord))
            {
                //Find the input coordinates in texture basis coordinates
                Vector2 convertedCoord = convertToTextureBasis(coord);

                //Find the direction vector pointing from the origin point to the input point
                Vector3 directionVector = Vector3.Add(Vector3.Multiply(Vector3.Subtract(y, x), convertedCoord.X),
                                                      Vector3.Multiply(Vector3.Subtract(z, x), convertedCoord.Y));

                //Add the direction vector to the origin point to get the point we need
                return Vector3.Add(directionVector, x);
            }
            else
            {
                return (x + y + z) / 3;
            }
        }

        public bool lineIntersects(Vector3 start, Vector3 end)
        {
            //Find intersection of line and plane containing triangle
            float denominator = Vector3.Dot(normal, end - start);

            //If the denominator is 0 then it is parallel
            if (denominator == 0)
            {
                return false;
            }
            else
            {
                float numerator = Vector3.Dot(normal, x - start);
                float param = numerator / denominator;

                //The triangle is past the end or before the start
                if (param > 1 || param < 0)
                {
                    return false;
                }
                else
                {
                    //Find coefficients of triangle vectors to get to that point

                    Vector3 a = y - x;
                    Vector3 b = z - x;
                    Vector3 intersection = start + param * (end - start);

                    float adota = Vector3.Dot(a,a);
                    float bdotb = Vector3.Dot(b,b);
                    float adotb = Vector3.Dot(a,b);
                    float intdota = Vector3.Dot(intersection,a);
                    float intdotb = Vector3.Dot(intersection,b);

                    float denominator2 = (adotb * adotb) - (adota * bdotb);

                    //This is a degenerate triangle
                    if (denominator2 == 0)
                    {
                        return false;
                    }
                    else
                    {
                        //Find the combination of vectors to get the intersection point
                        float u = ((adotb * intdota) - (adota * intdotb)) / denominator2;
                        float t = ((adotb * intdotb) - (bdotb * intdota)) / denominator2;
                        float sum = t + u;
                        
                        //if the sum of the parameters is not between 0 and 1 then outside the triangle
                        if (sum > 1 || sum < 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool pointIsBetween(Vector2 a, Vector2 topLeft, Vector2 bottomRight)
        {
            return a.X > topLeft.X && a.X < bottomRight.X && a.Y < topLeft.Y && a.Y > bottomRight.Y;
        }

        private Vector2 convertToTextureBasis(Vector2 coord){
            
            //Find vector from origin to input point
            Vector2 point = Vector2.Subtract(coord, a);

            //Solve for the u and v values using the inverse matrix (matrix multiplication)
            float u = Vector2.Dot(lightMapInverseMatrix[0], point);
            float v = Vector2.Dot(lightMapInverseMatrix[1], point);

            return new Vector2(u,v);
        }

        private void calculateInverseMatrix()
        {
            //Calculate the basis matrix, this matrix will only be singular if the triangle is degenerate
            Vector2 firstCol = Vector2.Subtract(b, a);
            Vector2 secondCol = Vector2.Subtract(c, a);

            //Find the reciprocal determinant of the matrix
            determinant = 1 / (firstCol.X * secondCol.Y - secondCol.X * firstCol.Y);

            //calculate the inverse matrix
            lightMapInverseMatrix[0] = Vector2.Multiply(new Vector2(-firstCol.X, secondCol.Y), determinant);
            lightMapInverseMatrix[1] = Vector2.Multiply(new Vector2(secondCol.X, -firstCol.Y), determinant);

        }

        private void calculateNormal()
        {
            _normal = Vector3.Cross(Vector3.Subtract(y, x), Vector3.Subtract(z, x));
            _normal.Normalize();
        }


    }
}
