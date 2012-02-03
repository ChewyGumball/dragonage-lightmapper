using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using Bioware.Files;

using Ben;

using DALightmapper;

namespace Bioware.Structs
{
    public class MeshChunk
    {
        public String name {get; private set; }
        public String id { get; set; }
        public uint vertexSize {get; private set; }
        public uint vertexCount {get; private set; }
        public uint indexCount {get; private set; }
        public int positionOffset {get; private set; }
        public int textureOffset {get; private set; }
        public int texture2Offset {get; private set; }
        public int normalOffset {get; private set; }
        public Boolean usesTwoTexCoords {get; private set; }
        public uint startIndex {get; private set; }
        public uint vertexOffset {get; private set; }

        public Triangle[] tris {get; private set; }
        public double area { get; set; }
        public Boolean casts { get; set; }
        public Boolean receives { get; set; }


        public String materialObjectName { get; set; }
        public uint textureID { get; set; }
        public uint lightmapID { get; set; }

        public Vector3 chunkOffset { get; set; }
        
        //Already made triangles constructor(terrain meshes)
        public MeshChunk(Triangle[] triangles)
        {
            tris = triangles;
            usesTwoTexCoords = triangles[0].isLightmapped;
            for (int i = 0; i < tris.Length; i++)
            {
                area += 0.5 * Vector3.Cross(tris[i].y - tris[i].x, tris[i].z - tris[i].x).Length;
            }
        }

        //Reading from a file constructor (normal meshes)
        public MeshChunk(BinaryReader file, long dataOffset, BiowareStruct chunkDef, BiowareStruct vertStruct)
        {
            uint listLength;
            long reference;
            long position = file.BaseStream.Position;

            //Read the name
            file.BaseStream.Seek(chunkDef.fields[0].index + position, SeekOrigin.Begin);
            name = IOUtilities.readECString(file, dataOffset + file.ReadInt32());


            //Seek to vertex size offset and read it
            file.BaseStream.Seek(chunkDef.fields[1].index + position, SeekOrigin.Begin);
            vertexSize = file.ReadUInt32();

            //Seek to vertex count offset and read it
            file.BaseStream.Seek(chunkDef.fields[2].index + position, SeekOrigin.Begin);
            vertexCount = file.ReadUInt32();

            //Seek to vertex offset offset and read it
            file.BaseStream.Seek(chunkDef.fields[7].index + position, SeekOrigin.Begin);
            vertexOffset = file.ReadUInt32();

            //Seek to index count offset and read it
            file.BaseStream.Seek(chunkDef.fields[3].index + position, SeekOrigin.Begin);
            indexCount = file.ReadUInt32();
            
            //Seek to index offset offset and read it
            file.BaseStream.Seek(chunkDef.fields[10].index + position, SeekOrigin.Begin);
            startIndex = file.ReadUInt32();

            //Seek to vertex declarator offset and read in the list reference
            file.BaseStream.Seek(chunkDef.fields[13].index + position, SeekOrigin.Begin);
            reference = file.ReadUInt32();

            //Make the triangle array to be filled later
            tris = new Triangle[indexCount / 3];

            //Seek to the list
            file.BaseStream.Seek(reference + dataOffset, SeekOrigin.Begin);
            listLength = file.ReadUInt32();
            reference = file.BaseStream.Position;
            usesTwoTexCoords = false;
            Usage type;

            //Get the offsets
            for (int i = 0; i < listLength; i++)
            {
                file.BaseStream.Seek(reference + (vertStruct.structSize * i) + vertStruct.fields[3].index, SeekOrigin.Begin);
                type = (Usage)file.ReadUInt32();
                file.BaseStream.Seek(reference + (vertStruct.structSize * i) + vertStruct.fields[1].index, SeekOrigin.Begin);
                switch (type)
                {
                    case Usage.POSITION:
                        positionOffset = file.ReadInt32();
                        break;

                    case Usage.TEXCOORD:
                        int offset = file.ReadInt32();
                        file.BaseStream.Seek(reference + (vertStruct.structSize * i) + vertStruct.fields[4].index, SeekOrigin.Begin);
                        uint index = file.ReadUInt32();
                        if (index == 0)
                        {
                            textureOffset = offset;
                            texture2Offset = offset;
                        }
                        else if (index == 1)
                        {
                            texture2Offset = offset;
                            usesTwoTexCoords = true;
                        }
                        break;

                    case Usage.NORMAL:
                        normalOffset = file.ReadInt32();
                        break;
                }
            }
        }
    }
}
