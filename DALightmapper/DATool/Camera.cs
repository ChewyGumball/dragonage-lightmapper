using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace DATool
{
    public class Camera
    {
        public Vector3 position { get; private set; }
        public Matrix4 matrix { get { return Matrix4.CreateTranslation(position) * Matrix4.CreateRotationZ(-rightAngle) * Matrix4.CreateRotationX(-upAngle); } }
        private float upAngle = 0;
        private float rightAngle = 0;

        public Camera()
        {
            position = new Vector3();
        }
        

        public void localTranslate(Vector3 trans)
        {
            translate(getLocalVector(trans));
        }
        public void translate(Vector3 translate)
        {
            position -= translate;
        }

        public void rotateRight(float angle)
        {
            rightAngle += angle;
            
            rightAngle = rightAngle % (float)(Math.PI * 2);
        }

        public void rotateUp(float angle)
        {
            upAngle += angle;

            if (upAngle > Math.PI)
                upAngle = (float)(Math.PI);
            else if (upAngle < 0)
                upAngle = 0;
        }

        private Vector3 getLocalVector(Vector3 world)
        {
            return Vector3.Transform(world, Matrix4.CreateRotationX(upAngle) * Matrix4.CreateRotationZ(rightAngle));
        }
    }
}
