using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DALightmapper
{
    public interface FindableFile
    {
        void createFromPathWithOffset(String filename, long offset, int length);
        void createFromPath(String filename);
    }
}
