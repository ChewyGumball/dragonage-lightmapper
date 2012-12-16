using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using OpenTK;
using Geometry;

namespace Tests.Geometry._3D
{
    [TestFixture]
    public class TriangleTests
    {
        [Test]
        public void intersection()
        {
            Triangle vertical = new Triangle(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, Vector2.Zero, Vector2.UnitY, Vector2.UnitX);

            Vector3 intersectionPoint;
            float t = vertical.intersection(new Vector3(-0.5f, 0.2f, 0.2f), Vector3.UnitX, out intersectionPoint);
            Assert.AreEqual(0.5f, t);
            Assert.AreEqual(new Vector3(0, 0.2f, 0.2f), intersectionPoint);

            Triangle horizontal = new Triangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, Vector2.Zero, Vector2.UnitX, Vector2.UnitY);

            Vector3 intersectionPoint2;
            float u = horizontal.intersection(new Vector3(0.2f, -0.5f, 0.2f), Vector3.UnitY, out intersectionPoint2);
            Assert.AreEqual(0.5f, u);
            Assert.AreEqual(new Vector3(0.2f, 0, 0.2f), intersectionPoint2);

            Vector3 intersectionPoint3;
            float v = vertical.intersection(new Vector3(0.2f, -0.5f, 0.2f), Vector3.UnitY, out intersectionPoint3);
            Assert.AreEqual(-1.0f, v);
            Assert.AreEqual(Vector3.Zero, intersectionPoint3);

            Vector3 intersectionPoint4;
            float w = vertical.intersection(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(1.1f, -1.0f, -1.0f), out intersectionPoint4);
            Assert.Greater(w, 0);
            Assert.AreEqual(0, intersectionPoint4.X);
            Assert.AreEqual(new Vector3(-1.0f, 1.0f, 1.0f) + new Vector3(1.1f, -1.0f, -1.0f) * w, intersectionPoint4);
        }

        [Test]
        public void onUV()
        {
            Triangle t = new Triangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector2.Zero, Vector2.UnitX, Vector2.UnitY, Vector2.Zero, Vector2.UnitX, Vector2.UnitY);
            Assert.True(t.isOnUVPixel(new Vector2(-2.0f, 2.0f), new Vector2(3.0f, -1.0f))); //Surrounds triangle
            Assert.True(t.isOnUVPixel(new Vector2(0, 0.5f), new Vector2(0.5f, 0))); //Completely inside
            Assert.True(t.isOnUVPixel(new Vector2(0.5f, 1.0f), new Vector2(1.0f, 0.5f))); //On the edge

            Assert.False(t.isOnUVPixel(new Vector2(-2.0f, 2.0f), new Vector2(-1.0f, 0))); //Not inside
        }
    }
}
