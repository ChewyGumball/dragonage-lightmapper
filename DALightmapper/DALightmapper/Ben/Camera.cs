using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Ben
{
    class Camera
    {
        public Vector3 position { get; set; }
        public Matrix4 matrix { get; set; }

        public Camera()
            : this(new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,0,2))
        { }

        public Camera(Vector3 inPosition, Vector3 inUp, Vector3 inLookAt)
        {
            matrix = Matrix4.LookAt(inPosition, inLookAt, inUp);
        }

        public void localTranslate(Vector3 trans)
        {
            translate(getLocalVector(trans));
        }
        public void translate(Vector3 translate)
        {
            matrix = Matrix4.Translation(translate) * matrix;
        }

        public void localRotate(Vector3 axis, float angle)
        {
            rotate(getLocalVector(axis), angle);
        }
        public void rotate(Vector3 axis, float angle)
        {
            matrix = Matrix4.Rotate(axis, angle) * matrix;
        }

        private Vector3 getLocalVector(Vector3 world)
        {
            Vector3 x = matrix.Row0.Xyz;
            Vector3 y = matrix.Row1.Xyz;
            Vector3 z = matrix.Row2.Xyz;
            return (x * world.X) + (y * world.Y) + (z * world.Z); 
        }
    }
}
