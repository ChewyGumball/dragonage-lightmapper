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

        bool isLightmapped;

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
            //Find the input coordinates in texture basis coordinates
            Vector2 convertedCoord = convertToTextureBasis(coord);

            //Find the direction vector pointing from the origin point to the input point
            Vector3 directionVector = Vector3.Add(Vector3.Multiply(Vector3.Subtract(y, x),convertedCoord.X),
                                                  Vector3.Multiply(Vector3.Subtract(z, x), convertedCoord.Y));

            //Add the direction vector to the origin point to get the point we need
            return Vector3.Add(directionVector, x);
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
