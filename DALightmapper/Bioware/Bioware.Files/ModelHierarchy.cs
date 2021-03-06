﻿using System;
using System.IO;
using OpenTK;

using Bioware.IO;
using Bioware.Structs;

namespace Bioware.Files
{
    public class ModelHierarchy
    {
        #region Index Values
        private static readonly int MMH_NAME_INDEX = 0;
        private static readonly int MSH_NAME_INDEX = 1;
        private static readonly int TOTAL_BONES_INDEX = 3;
        private static readonly int TOP_LEVEL_CHILDREN_INDEX = 6;

        private static readonly int GOB_CHILDREN_INDEX = 3;

       // private static readonly int MSH_CHUNK_MESH_NAME_INDEX = 0;
        private static readonly int MSH_CHUNK_MATERIAL_INDEX = 1;
        private static readonly int MSH_CHUNK_ID_INDEX = 3;
        private static readonly int MSH_CHUNK_GROUP_NAME_INDEX = 4;
       // private static readonly int MSH_CHUNK_CASTS_RUNTIME_INDEX = 5;
        private static readonly int MSH_CHUNK_CASTS_BAKED_INDEX = 6;
        private static readonly int MSH_CHUNK_RECEIVES_BAKED_INDEX = 10;
       // private static readonly int MSH_CHUNK_RECEIVES_RUNTIME_INDEX = 11;
        private static readonly int MSH_CHUNK_CHILDREN_INDEX = 16;
        #endregion

        private int nodeStructIndex;
        private int meshChunkInfoIndex;
        private int translationStructIndex;
        private int rotationStructIndex;
        
        private BiowareStruct meshChunkInfoStruct;
        private BiowareStruct nodeStruct;

        private GFF binaryFile;
        int numBones;

        public Boolean isFXModel { get; private set; }
        public String mmhName { get; private set; }
        public String mshName { get; private set; }
        public ModelMesh mesh { get; private set; }

        public Boolean loadedMesh { get; private set; }

        public ModelHierarchy(GFF gffFile)
        {
            loadedMesh = false;
            binaryFile = gffFile;
            setStructDefinitions();
            readData();
        }

        public void readData()
        {
            BinaryReader file = binaryFile.openReader();
            int reference;

            //Get the name of the mmh file
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[MMH_NAME_INDEX].index, SeekOrigin.Begin);
            mmhName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32()).ToLower();

            //Get the name of the msh file
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[MSH_NAME_INDEX].index, SeekOrigin.Begin);
            mshName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32()).ToLower();

            //Get the total number of bones in the mmh
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[TOTAL_BONES_INDEX].index, SeekOrigin.Begin);
            numBones = file.ReadInt32();

            //Apparently fx models have an extra field...
            isFXModel = binaryFile.structs[0].fields.Length == 8;
            if (isFXModel)
            {
                file.Close();
                return;
            }

            //Get the children list (should only contain GOB)
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[TOP_LEVEL_CHILDREN_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            GenericList childrenList = new GenericList(file);

            //Get the children of the GOB object
            //Sometimes its a node struct, sometimes its a mshh struct . . .

            if ((int)(childrenList.type[0].id) == nodeStructIndex)
            {
                file.BaseStream.Seek(binaryFile.dataOffset + childrenList[0] + nodeStruct.fields[GOB_CHILDREN_INDEX].index, SeekOrigin.Begin);
            }
            else if ((int)(childrenList.type[0].id) == meshChunkInfoIndex)
            {
                file.BaseStream.Seek(binaryFile.dataOffset + childrenList[0] + meshChunkInfoStruct.fields[MSH_CHUNK_CHILDREN_INDEX].index, SeekOrigin.Begin);
            }

            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            childrenList = new GenericList(file);


            //Find the mesh
            GFF temp = ResourceManager.findFile<GFF>(mshName);
            //If its not there throw an exception cause we need it
            if (temp == null)
            {
                Console.WriteLine("Could not find mesh file \"{0}\".", mshName);
                file.Close();
                return;
                //throw new Exception("COULD NOT FIND MESH FILE, LOOK AT CONSOLE!!!!!!");
            }

            loadedMesh = true;

            //Make the mesh
            mesh = new ModelMesh(temp);

            //For each thing in the child list
            for (int i = 0; i < childrenList.length; i++)
            {
                //If the child is a mesh chunk info struct
                if ((int)childrenList.type[i].id == meshChunkInfoIndex)
                {
                    //Fill in the missing mesh chunk info
                    updateChunk(file, binaryFile.dataOffset + childrenList[i], new Vector3(), new Quaternion());
                }
            }
            file.Close();
        }

        private void updateChunk(BinaryReader file, long startPosition, Vector3 offset, Quaternion rotation)
        {
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_GROUP_NAME_INDEX].index,SeekOrigin.Begin);

            //Get the name of the meshchunk this info is for
            String currentMeshChunkName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());

            //Console.WriteLine("Doing Chunk " + currentMeshChunkName);

            //Find the meshChunk we need
            MeshChunk currentMeshChunk = null;
            foreach (MeshChunk m in mesh.chunks)
            {
                if (m.name == currentMeshChunkName)
                {
                    currentMeshChunk = m;
                    break;
                }
            }
            //If no mesh chunk could be found there was a problem
            if (currentMeshChunk == null)
            {
                Console.WriteLine("Could not find mesh chunk \"{0}\" in msh file \"{1}\".", currentMeshChunkName, mshName);
                throw new Exception("COULD NOT FIND MESHCHUNK, LOOK AT CONSOLE!!!!!!");
            }

            //Get the material name
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_MATERIAL_INDEX].index, SeekOrigin.Begin);
            currentMeshChunk.materialObjectName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32()) + ".mao";

            //Get the chunk ID
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_ID_INDEX].index, SeekOrigin.Begin);
            currentMeshChunk.id = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());


            //Get whether it casts shadows
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_CASTS_BAKED_INDEX].index, SeekOrigin.Begin);
            currentMeshChunk.casts = file.ReadByte() == 1;

            //Get whether it receives shadows
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_RECEIVES_BAKED_INDEX].index, SeekOrigin.Begin);
            currentMeshChunk.receives = currentMeshChunk.usesTwoTexCoords ? file.ReadByte() == 1 : false;

            //Get translation offset and rotation
            file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_CHILDREN_INDEX].index, SeekOrigin.Begin);
            int reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            GenericList attributes = new GenericList(file);

            for (int j = 0; j < attributes.length; j++)
            {
                if ((int)(attributes.type[j].id) == rotationStructIndex)
                {
                    file.BaseStream.Seek(binaryFile.dataOffset + attributes[j], SeekOrigin.Begin);
                    currentMeshChunk.chunkRotation = new Quaternion(file.ReadSingle(), file.ReadSingle(), file.ReadSingle(), file.ReadSingle()) * rotation;
                }
                else if ((int)(attributes.type[j].id) == translationStructIndex)
                {
                    file.BaseStream.Seek(binaryFile.dataOffset + attributes[j], SeekOrigin.Begin);
                    currentMeshChunk.chunkOffset = offset + new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
                }
                else if ((int)(attributes.type[j].id) == meshChunkInfoIndex)
                {
                    updateChunk(file, binaryFile.dataOffset + attributes[j], currentMeshChunk.chunkOffset, currentMeshChunk.chunkRotation);
                }
            }
        }
        
        
        private void setStructDefinitions()
        {
            for (int i = 0; i < binaryFile.structs.Length; i++)
            {
                switch (binaryFile.structs[i].type)
                {
                    case GFFSTRUCTTYPE.MMH_MSH_STRUCT:
                        meshChunkInfoIndex = i;
                        meshChunkInfoStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.MMH_NODE_STRUCT:
                        nodeStructIndex = i;
                        nodeStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.MMH_ORIENTATION:
                        rotationStructIndex = i; break;
                    case GFFSTRUCTTYPE.MMH_OFFSET:
                        translationStructIndex = i; break;
                }
            }
        }
    }
}
