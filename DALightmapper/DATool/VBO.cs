using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bioware.Structs;

namespace DATool
{
    class VBO
    {
        public int vertexSize { get; private set; }
        public int indexElementCount { get; private set; }
        public int vertexBuffer { get; private set; }
        public int elementBuffer { get; private set; }
        public int textureBuffer { get; private set; }

        public VBO(int vb, int eb, int tb, int vs, int ec)
        {
            vertexBuffer = vb;
            elementBuffer = eb;
            textureBuffer = tb;

            vertexSize = vs;
            indexElementCount = ec;
        }
    }
}
