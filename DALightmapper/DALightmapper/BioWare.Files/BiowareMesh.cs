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
    public enum Usage : uint
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

    public abstract class BiowareMesh
    {
        protected GFF binaryFile;

        protected String name;
        public MeshChunk[] chunks { get; protected set; }

        public BiowareMesh()
        {
            name = "EMPTY MESH";
            chunks = new MeshChunk[0];
        }

        public BiowareMesh(GFF gffFile)
        {
            binaryFile = gffFile;
            readData();
        }

        //Currently mesh chunks dont store texture ids in triangles, need to do that if textures are needed
        public Model toModel()
        {
            Mesh[] meshes = new Mesh[chunks.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new Mesh(chunks[i].name, chunks[i].tris, chunks[i].receives, chunks[i].casts, chunks[i].chunkOffset, chunks[i].id);
            }
            return new Model(name, meshes);
        }
        
        public abstract void readData();
    }
}
