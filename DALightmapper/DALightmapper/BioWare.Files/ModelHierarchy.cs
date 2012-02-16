using System;
using System.IO;
using OpenTK;

using Ben;

using Bioware.Structs;

using DALightmapper;

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
        private static readonly int TRANSLATION_ATTRIBUTE = 1;

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

        private int meshChunkInfoIndex;
        private BiowareStruct meshChunkInfoStruct;
        private BiowareStruct nodeStruct;

        private GFF binaryFile;

        String _mmhName, _mshName;
        int _numBones;
        private BiowareMesh _mesh;

        public Boolean isFXModel { get; private set; }
        public String mmhName
        {
            get { return _mmhName; }
        }
        public String mshName
        {
            get { return _mshName; }
        }
        public BiowareMesh mesh
        {
            get { return _mesh; }
        }

        public ModelHierarchy(BiowareMesh m, String name)
        {
            _mmhName = name;
            _mshName = name;
            _mesh = m;
        }
        public ModelHierarchy(GFF gffFile)
        {
            binaryFile = gffFile;
            setStructDefinitions();
            readData();
        }

        public void readData()
        {
            BinaryReader file = binaryFile.getReader();
            int reference;

            //Get the name of the mmh file
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[MMH_NAME_INDEX].index, SeekOrigin.Begin);
            _mmhName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32()).ToLower();

            //Get the name of the msh file
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[MSH_NAME_INDEX].index, SeekOrigin.Begin);
            _mshName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32()).ToLower();

            //Get the total number of bones in the mmh
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[TOTAL_BONES_INDEX].index, SeekOrigin.Begin);
            _numBones = file.ReadInt32();

            //Apparently fx models have an extra field...
            isFXModel = binaryFile.structs[0].fields.Length == 8;
            if (isFXModel)
            {
                return;
            }

            //Get the children list (should only contain GOB)
            file.BaseStream.Seek(binaryFile.dataOffset + binaryFile.structs[0].fields[TOP_LEVEL_CHILDREN_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            GenericList childrenList = new GenericList(file);

            //Get the children of the GOB object
            file.BaseStream.Seek(binaryFile.dataOffset + childrenList[0] + nodeStruct.fields[GOB_CHILDREN_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            childrenList = new GenericList(file);



            //Find the mesh
            GFF temp = IO.findGFFFile(_mshName);
            //If its not there throw an exception cause we need it
            if (temp == null)
            {
                Console.WriteLine("Could not find mesh file \"{0}\".", _mshName);
                throw new Exception("COULD NOT FIND MESH FILE, LOOK AT CONSOLE!!!!!!");
            }

            //Make the mesh
            _mesh = new ModelMesh(temp);

            //Fill in the missing mesh chunk info from the mmh file
            MeshChunk currentMeshChunk;
            String currentMeshChunkName;
            long startPosition;

            //For each thing in the child list
            for (int i = 0; i < childrenList.length; i++)
            {
                //If the child is a mesh chunk info struct
                if ((int)childrenList.type[i].id == meshChunkInfoIndex)
                {
                    //add the info into the appropriate mesh chunk

                    startPosition = binaryFile.dataOffset + childrenList[i];
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_GROUP_NAME_INDEX].index,SeekOrigin.Begin);

                    //Get the name of the meshchunk this info is for
                    currentMeshChunkName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());

                    //Find the meshChunk we need
                    currentMeshChunk = null;
                    foreach (MeshChunk m in mesh.meshChunks)
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
                        Console.WriteLine("Could not find mesh chunk \"{0}\" in msh file \"{1}\".", currentMeshChunkName, _mshName);
                        throw new Exception("COULD NOT FIND MESHCHUNK, LOOK AT CONSOLE!!!!!!");
                    }

                    //Get the material name
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_MATERIAL_INDEX].index, SeekOrigin.Begin);
                    currentMeshChunk.materialObjectName = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());

                    //Get the chunk ID
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_ID_INDEX].index, SeekOrigin.Begin);
                    currentMeshChunk.id = IOUtilities.readECString(file, binaryFile.dataOffset + file.ReadInt32());
                    

                    //Get whether it casts shadows
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_CASTS_BAKED_INDEX].index, SeekOrigin.Begin);
                    currentMeshChunk.casts = file.ReadByte() == 1;

                    //Get whether it receives shadows
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_RECEIVES_BAKED_INDEX].index, SeekOrigin.Begin);
                    currentMeshChunk.receives = currentMeshChunk.usesTwoTexCoords? file.ReadByte() == 1 : false;

                    //Get translation offset
                    file.BaseStream.Seek(startPosition + meshChunkInfoStruct.fields[MSH_CHUNK_CHILDREN_INDEX].index, SeekOrigin.Begin);
                    reference = file.ReadInt32();
                    file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
                    GenericList attributes = new GenericList(file);

                    file.BaseStream.Seek(binaryFile.dataOffset + attributes[TRANSLATION_ATTRIBUTE], SeekOrigin.Begin);
                    currentMeshChunk.chunkOffset = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
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
                        nodeStruct = binaryFile.structs[i]; break;
                }
            }
        }
    }
}
