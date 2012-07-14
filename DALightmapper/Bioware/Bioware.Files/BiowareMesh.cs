using System;

using Bioware.Structs;

using Geometry;

namespace Bioware.Files
{
    

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
                meshes[i] = new Mesh(chunks[i].name, chunks[i].tris, chunks[i].receives, chunks[i].casts, chunks[i].chunkOffset, chunks[i].chunkRotation, chunks[i].id);
            }
            return new Model(name, meshes);
        }
        
        public abstract void readData();
    }
}
