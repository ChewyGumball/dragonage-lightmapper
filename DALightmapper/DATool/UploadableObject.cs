using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATool
{
    interface UploadableObject
    {
        void upload(ref List<VBO> vboList, ref Dictionary<String, int> textureList);
    }
}
