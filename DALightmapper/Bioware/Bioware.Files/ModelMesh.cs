﻿using System;
using System.Text;
using System.IO;
using OpenTK;

using Bioware.Structs;
using Bioware.IO;

using Geometry;

namespace Bioware.Files
{
    public class ModelMesh : BiowareMesh
    {
        public ModelMesh()
            : base()
        { }

        public ModelMesh(MeshChunk[] meshChunks)
            : base()
        {
            chunks = meshChunks;
        }

        public ModelMesh(GFF gffFile)
            : base(gffFile)
        { }

        public override void readData()
        {
            BinaryReader file = binaryFile.openReader();

            //holds references
            int reference;
            //Holds length of lists
            uint listLength;

            //Positions
            Vector3[] verts;
            //1st channel uvs
            Vector2[] uvs;
            //second channel (lightmap) uvs
            Vector2[] luvs;

            //Make sure this is not a FX mesh (instance stream is 0)

            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[5].index, SeekOrigin.Begin);
            if (file.ReadByte() != 0)
            {
                chunks = new MeshChunk[0];
                return;
            }

            //seek to the beginning of the data part of the file
            file.BaseStream.Seek(binaryFile.dataOffset, SeekOrigin.Begin);

            //get the reference to the name of the mesh
            //get the name of the mesh
            name = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());

            //Get the reference to the list of chunks
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[1].index, SeekOrigin.Begin);
            reference = file.ReadInt32();

            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            listLength = file.ReadUInt32();

            //Save the current position for the loop
            long startPosition = file.BaseStream.Position;
            //Make an array of chunks of the correct size
            chunks = new MeshChunk[listLength];
            int triangleNum = 0;

            //Create all the chunks
            for (int i = 0; i < listLength; i++)
            {
                file.BaseStream.Seek(startPosition + (i * 4), SeekOrigin.Begin);
                file.BaseStream.Seek(binaryFile.dataOffset + file.ReadInt32(), SeekOrigin.Begin);
                chunks[i] = new MeshChunk(file, binaryFile.dataOffset, binaryFile.structs[4], binaryFile.structs[1]);
                triangleNum += (int)chunks[i].indexCount / 3;
            }

            triangleNum = 0;

            //Seek to the vertex data position
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[2].index, SeekOrigin.Begin);
            //Get the reference to the list
            long vertexOffset = file.ReadUInt32() + binaryFile.dataOffset + 4;

            //Seek to the index data position
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[3].index, SeekOrigin.Begin);
            //Get the reference to the list
            long indexOffset = file.ReadUInt32() + binaryFile.dataOffset + 4;
            //Index variables for triangles
            ushort index1, index2, index3;

            //For each chunk read in vertex data
            for (int i = 0; i < chunks.Length; i++)
            {
                verts = new Vector3[chunks[i].vertexCount];
                uvs = new Vector2[chunks[i].vertexCount];
                luvs = new Vector2[chunks[i].vertexCount];
                //Read in the vertex data
                for (int j = 0; j < chunks[i].vertexCount; j++)
                {
                    int vertArrayIndex = j * (chunks[i].vertexElementCount);
                    int positionIndex = vertArrayIndex;
                    int normalIndex = vertArrayIndex + 3;
                    int uvIndex = vertArrayIndex + 6;

                    long currentIndex = vertexOffset + chunks[i].vertexOffset + (chunks[i].vertexSize * j);
                    //Read in the positions
                    file.BaseStream.Seek(currentIndex + chunks[i].positionOffset, SeekOrigin.Begin);
                    //Put them in the vert array
                    verts[j] = new Vector3(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    //Put them in the chunk for easy rendering
                    chunks[i].verts[positionIndex] = verts[j].X;
                    chunks[i].verts[positionIndex + 1] = verts[j].Y;
                    chunks[i].verts[positionIndex + 2] = verts[j].Z;

                    //Read in the normals for rendering only, we make our own normals later
                    file.BaseStream.Seek(currentIndex + chunks[i].normalOffset, SeekOrigin.Begin);
                    chunks[i].verts[normalIndex] = IOUtilities.readFloat16(file);
                    chunks[i].verts[normalIndex + 1] = IOUtilities.readFloat16(file);
                    chunks[i].verts[normalIndex + 2] = IOUtilities.readFloat16(file);


                    //Read in the uvs
                    file.BaseStream.Seek(currentIndex + chunks[i].textureOffset, SeekOrigin.Begin);
                    uvs[j] = new Vector2(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    //Put them in the chunk for easy rendering
                    chunks[i].verts[uvIndex] = uvs[j].X;
                    chunks[i].verts[uvIndex + 1] = uvs[j].Y;

                    //If there are a second set, read them in too
                    if (chunks[i].usesTwoTexCoords)
                    {
                        file.BaseStream.Seek(currentIndex + chunks[i].texture2Offset, SeekOrigin.Begin);
                        luvs[j] = new Vector2(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    }
                }

                //Seek to index data (skipping the length int at beginning)
                file.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
                for (int j = 0; j < chunks[i].indexCount / 3; j++)
                {
                    int indexIndex = j * 3;
                    //Read in the indices
                    index1 = file.ReadUInt16();
                    index2 = file.ReadUInt16();
                    index3 = file.ReadUInt16();

                    chunks[i].indices[indexIndex] = index1;
                    chunks[i].indices[indexIndex + 1] = index2;
                    chunks[i].indices[indexIndex + 2] = index3;

                    //If lightmap coords are specified make a triangle with them 
                    if (chunks[i].usesTwoTexCoords)
                        chunks[i].tris[j] = new Triangle(verts[index1], verts[index2], verts[index3], uvs[index1], uvs[index2], uvs[index3], luvs[index1], luvs[index2], luvs[index3]);
                    else
                        //Otherwise make one without them
                        chunks[i].tris[j] = new Triangle(verts[index1], verts[index2], verts[index3], uvs[index1], uvs[index2], uvs[index3]);

                    //Calculate the area of the triangle and add it to the total area of the mesh
                    chunks[i].area += 0.5 * Vector3.Cross(chunks[i].tris[j].y - chunks[i].tris[j].x, chunks[i].tris[j].z - chunks[i].tris[j].x).Length;
 
                    //Increase offset by 3 indices (2 bytes each)
                    indexOffset += 6;
                }
            }


            //Close the file
            file.Close();
        }

        private int getChunkSize()
        {
            foreach (BiowareStruct bStruct in binaryFile.structs)
            {
                if (bStruct.type == GFFSTRUCTTYPE.MESH_CHUNK)
                    return (int)bStruct.structSize;
            }
            return 0;
        }
    }
}
