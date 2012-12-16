using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Bioware.Structs;
using OpenTK;
using Geometry;

namespace Tests.Geometry.Partitioners
{
    [TestFixture]
    public class KDTreeTests
    {
        Random r = new Random();
        private float randomFloat()
        {
            return (float)((r.NextDouble() - 0.5) * float.MaxValue) * 2.0f;
        }
        private List<Photon> createList(int number)
        {
            Light l = new AmbientLight(Vector3.Zero, Vector3.Zero, Vector3.Zero, 3.0f, true);
            List<Photon> list = new List<Photon>(number);
            for (int i = 0; i < number; i++)
            {
                list.Add(new Photon(new Vector3(randomFloat(), randomFloat(), randomFloat()), l, 4.0f));
            }
            return list;
        }

        void evenLengthConstructor()
        {
            KDTree test = new KDTree(createList(40));
        }
        void oddLengthConstructor()
        {
            KDTree test = new KDTree(createList(55));
        }
        void longListConstructor()
        {
            KDTree test = new KDTree(createList(30000));
        }
        void zeroConstructor()
        {
            KDTree test = new KDTree(new List<Photon>());
        }
        void oneConstructor()
        {
            KDTree test = new KDTree(createList(1));
        }
        void twoConstructor()
        {
            KDTree test = new KDTree(createList(2));
        }

        [Test]
        public void construction()
        {
            Assert.DoesNotThrow(new TestDelegate(evenLengthConstructor), "Even length contructor failure");
            Assert.DoesNotThrow(new TestDelegate(oddLengthConstructor), "Odd length constructor failure");
            Assert.DoesNotThrow(new TestDelegate(longListConstructor), "Long list constructor failure");
            Assert.Throws<ArgumentOutOfRangeException>(new TestDelegate(zeroConstructor), "0 length constructor failure");
            Assert.DoesNotThrow(new TestDelegate(oneConstructor), "1 length constructor failure");
            Assert.DoesNotThrow(new TestDelegate(twoConstructor), "2 length constructor failure");
        }
    }
}
