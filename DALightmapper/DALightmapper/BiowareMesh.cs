using System;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenTK;

using Bioware.Structs;

using Ben;

using DALightmapper;

namespace Bioware.Files
{
    enum Usage : uint
    {
        /// Position
        POSITION = 0,
        /// Blend weights
        BLENDWEIGHT = 1,
        /// Blend indices
        BLENDINDICES = 2,
        /// Normal
        NORMAL = 3,
        /// Point Size 
        PSIZE = 4,
        /// Texture coordinates
        TEXCOORD = 5,
        /// Tangent vector
        TANGENT = 6,
        /// binormal vector
        BINORMAL = 7,
        /// tessellation factor
        TESSFACTOR = 8,
        /// PositionT 
        POSITIONT = 9,
        /// color channel
        COLOR = 10,
        /// fog value
        FOG = 11,
        /// depth 
        DEPTH = 12,
        /// sample
        SAMPLE = 13,
        // error/other/unset
        UNUSED = 0xffffffff
    }

    abstract class BiowareMesh
    {
        protected GFF binaryFile;

        protected String name;
        protected MeshChunk[] _chunks;

        protected Boolean _isTerrainMesh;

        public MeshChunk[] meshChunks
        {
            get { return _chunks; }
        }
        public bool isTerrain
        {
            get { return _isTerrainMesh; }
        }

        public BiowareMesh(GFF gffFile)
        {
            binaryFile = gffFile;
            readData();
        }

        //Currently mesh chunks dont store texture ids in triangles, need to do that if textures are needed
        public Model toModel()
        {
            Mesh[] meshes = new Mesh[_chunks.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Mesh(_chunks[i].name, _chunks[i].tris);
            }
            return new Model(name, meshes);
        }

        public abstract void readData();
    }
}
