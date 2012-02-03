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
    public class ModelMesh:BiowareMesh
    {
        public ModelMesh(GFF gffFile)
            : base(gffFile)
        {
            _isTerrainMesh = false;
        }
        public override void readData()
        {
            BinaryReader file = binaryFile.getReader();

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
            _chunks = new MeshChunk[listLength];
            int triangleNum = 0;
            
            //Create all the chunks
            for (int i = 0; i < listLength && !IO.abort; i++)
            {
                file.BaseStream.Seek(startPosition + (i * 4), SeekOrigin.Begin);
                file.BaseStream.Seek(binaryFile.dataOffset + file.ReadInt32(), SeekOrigin.Begin);
                _chunks[i] = new MeshChunk(file, binaryFile.dataOffset, binaryFile.structs[4], binaryFile.structs[1]);
                triangleNum += (int)_chunks[i].indexCount / 3;
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
            for (int i = 0; i < _chunks.Length && !IO.abort; i++)
            {
                verts = new Vector3[_chunks[i].vertexCount];
                uvs = new Vector2[_chunks[i].vertexCount];
                luvs = new Vector2[_chunks[i].vertexCount];
                //Read in the vertex data
                for (int j = 0; j < _chunks[i].vertexCount && !IO.abort; j++)
                {
                    long currentIndex = vertexOffset + _chunks[i].vertexOffset + (_chunks[i].vertexSize * j);
                    //Read in the positions
                    file.BaseStream.Seek(currentIndex + _chunks[i].positionOffset, SeekOrigin.Begin);
                    verts[j] = new Vector3(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    //Read in the uvs
                    file.BaseStream.Seek(currentIndex + _chunks[i].textureOffset, SeekOrigin.Begin);
                    uvs[j] = new Vector2(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    //If there are a second set, read them in too
                    if (_chunks[i].usesTwoTexCoords)
                    {
                        file.BaseStream.Seek(currentIndex + _chunks[i].texture2Offset, SeekOrigin.Begin);
                        luvs[j] = new Vector2(IOUtilities.readFloat16(file), IOUtilities.readFloat16(file));
                    }
                }

                //Seek to index data (skipping the length int at beginning)
                file.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
                for (int j = 0; j < _chunks[i].indexCount / 3 && !IO.abort; j++)
                {
                    //Read in the indices
                    index1 = file.ReadUInt16();
                    index2 = file.ReadUInt16();
                    index3 = file.ReadUInt16();

                    //If lightmap coords are specified make a triangle with them 
                    if (_chunks[i].usesTwoTexCoords)
                        _chunks[i].tris[j] = new Triangle(verts[index1], verts[index2], verts[index3], uvs[index1], uvs[index2], uvs[index3], luvs[index1], luvs[index2], luvs[index3]);
                    else
                        //Otherwise make one without them
                        _chunks[i].tris[j] = new Triangle(verts[index1], verts[index2], verts[index3], uvs[index1], uvs[index2], uvs[index3]);

                    //Calculate the area of the triangle and add it to the total area of the mesh
                    _chunks[i].area += 0.5 * Vector3.Cross(_chunks[i].tris[j].y - _chunks[i].tris[j].x, _chunks[i].tris[j].z - _chunks[i].tris[j].x).Length;

                    //Associate this chunk to the newly created triangle
                    _chunks[i].tris[j].chunk = _chunks[i];

                    //Increase offset by 3 indices (2 bytes each)
                    indexOffset += 6;
                }
            }


            //Close the file
            binaryFile.Close();
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
