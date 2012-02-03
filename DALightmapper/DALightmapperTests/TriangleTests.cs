using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenTK;
using DALightmapper;

namespace DALightmapperTests
{
    [TestFixture]
    public class TriangleTests
    {
        [TestCase]
        public void NonLightmappedTests()
        {
            Triangle t = new Triangle(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0));

            Assert.IsFalse(t.isOnUVPixel(new Vector2(0, 0.5f), new Vector2(0.5f, 0)));//compeltely on the triangle
            Assert.IsFalse(t.isOnUVPixel(new Vector2(0.2f, 1), new Vector2(1, 0.2f)));//one corner on the triangle
            Assert.IsFalse(t.isOnUVPixel(new Vector2(0.2f, 2), new Vector2(0.4f, -0.5f)));//no corners on the triangle or pixel

            Assert.IsFalse(t.isOnUVPixel(new Vector2(0.7f, 1), new Vector2(1, 0.7f)));//Not on the triangle but not completely above/below/left/right

            Assert.IsFalse(t.isOnUVPixel(new Vector2(0, 3), new Vector2(1, 2)));//Completely above
            Assert.IsFalse(t.isOnUVPixel(new Vector2(2, 0.5f), new Vector2(3, 0)));//Completely right
            Assert.IsFalse(t.isOnUVPixel(new Vector2(0, -0.5f), new Vector2(0.5f, -1)));//Completely below
            Assert.IsFalse(t.isOnUVPixel(new Vector2(-0.5f, 0.5f), new Vector2(-0.1f, 0)));//Completely left
        }

        [TestCase]
        public void LightmappedTests()
        {
            Triangle t = new Triangle(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0));
            
            Assert.IsTrue(t.isOnUVPixel(new Vector2(0, 0.5f), new Vector2(0.5f, 0)));//compeltely on the triangle
            Assert.IsTrue(t.isOnUVPixel(new Vector2(0.2f, 1), new Vector2(1, 0.2f)));//one corner on the triangle
            Assert.IsTrue(t.isOnUVPixel(new Vector2(0.2f, 2), new Vector2(0.4f, -0.5f)));//no corners on the triangle or pixel

            Assert.IsFalse(t.isOnUVPixel(new Vector2(0.7f, 1), new Vector2(1, 0.7f)));//Not on the triangle but not completely above/below/left/right
            
            Assert.IsFalse(t.isOnUVPixel(new Vector2(0, 3), new Vector2(1, 2)));//Completely above
            Assert.IsFalse(t.isOnUVPixel(new Vector2(2, 0.5f), new Vector2(3, 0)));//Completely right
            Assert.IsFalse(t.isOnUVPixel(new Vector2(0, -0.5f), new Vector2(0.5f, -1)));//Completely below
            Assert.IsFalse(t.isOnUVPixel(new Vector2(-0.5f, 0.5f), new Vector2(-0.1f, 0)));//Completely left
        }
    }
}
