using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;

using Bioware.Structs;
using DALightmapper;

namespace Bioware.Files
{
    class TerrainMesh : BiowareMesh
    {
        #region Index Values and Struct Definitions
        //These are constant for all files
        private const int SECTOR_ID_INDEX = 1;
        private const int MESH_FACE_LIST_INDEX = 2;
        private const int MESH_EDGE_LIST_INDEX = 3;
        private const int MESH_VERTEX_LIST_INDEX = 4;
        private const int MAP_VERTEX_LIST_INDEX = 5;
        private const int MAP_EDGE_LIST_INDEX = 6;
        private const int MAP_FACE_LIST_INDEX = 7;

        //These MAY be different between files but I have never seen them so
        private BiowareStruct infoStruct;
        private BiowareStruct meshFaceStruct;
        private BiowareStruct meshEdgeStruct;
        private BiowareStruct meshVertexStruct;
        private BiowareStruct mapFaceStruct;
        private BiowareStruct mapEdgeStruct;
        private BiowareStruct mapVertexStruct;
        #endregion

        private int _sectorID;

        private List<TerrainMeshFace> _faces;
        private List<TerrainMeshEdge> _edges;
        private List<TerrainMeshVertex> _verts;

        private List<TerrainMapFace> _mapFaces;
        private List<TerrainMapEdge> _mapEdges;
        private List<TerrainMapVertex> _mapVerts;

        public List<TerrainMeshFace> faces
        {
            get { return _faces; }
        }
        public List<TerrainMeshEdge> edges
        {
            get { return _edges; }
        }
        public List<TerrainMeshVertex> verts
        {
            get { return _verts; }
        }

        public List<TerrainMapFace> mapFaces
        {
            get { return _mapFaces; }
        }
        public List<TerrainMapEdge> mapEdges
        {
            get { return _mapEdges; }
        }
        public List<TerrainMapVertex> mapVerts
        {
            get { return _mapVerts; }
        }

        public int sectorID
        {
            get { return _sectorID; }
        }

        public TerrainMesh(GFF binaryFile):base(binaryFile)
        {
            _isTerrainMesh = true;
        }

        public override void  readData()
        {
            int reference, length;
            long startOfList;

            //Get the binary reader
            BinaryReader file = binaryFile.getReader();
            //Set up the struct definitions
            setStructDefinitions();

            //Get the sector ID
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[SECTOR_ID_INDEX].index, SeekOrigin.Begin);
            _sectorID = file.ReadInt32();




            /*----------MAP VERTS----------*/
            //Get the reference to the map vertex list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MAP_VERTEX_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);

            //Fill the map vertex list with all the map verts
            length = file.ReadInt32();
            _mapVerts = new List<TerrainMapVertex>(length);
            startOfList = file.BaseStream.Position;
            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * mapVertexStruct.structSize), SeekOrigin.Begin);
                //Add it to the list
                _mapVerts.Add(new TerrainMapVertex(mapVertexStruct, file));
            }




            /*----------MAP EDGES----------*/
            //Get the reference to the map edge list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MAP_EDGE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);

            //Fill the map edge list with all the map edges
            length = file.ReadInt32();
            _mapEdges = new List<TerrainMapEdge>(length);
            startOfList = file.BaseStream.Position;
            TerrainMapEdge tempEdge;

            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * mapEdgeStruct.structSize), SeekOrigin.Begin);
                //Create the edge
                tempEdge = new TerrainMapEdge(mapEdgeStruct, file);
                //Get the actual index in the array of the map vert
                for (int j = 0; j < _mapVerts.Count; j++)
                {
                    if (_mapVerts[j].id == tempEdge.startVertexIndex)
                    {
                        tempEdge.startVertexIndex = j;
                        break;
                    }
                }
                //Add it to the list
                _mapEdges.Add(tempEdge);
            }




            /*----------MAP FACES----------*/
            //Get the reference to the map face list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MAP_FACE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);

            //Fill the map face list with all the map faces
            length = file.ReadInt32();
            _mapFaces = new List<TerrainMapFace>(length);
            startOfList = file.BaseStream.Position;
            TerrainMapFace tempFace;

            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * mapFaceStruct.structSize), SeekOrigin.Begin);
                //Create the face
                tempFace = new TerrainMapFace(mapFaceStruct, file);
                //Find the actual edge indices
                for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
                {
                    for (int j = 0; j < _mapEdges.Count; j++)
                    {
                        if (tempFace[edgeIndex] == _mapEdges[j].id)
                        {
                            tempFace[edgeIndex] = j;
                            break;
                        }
                    }
                }
                //Add it to the list
                _mapFaces.Add(tempFace);
            }




            /*----------MESH VERTS----------*/
            //Get the reference to the mesh vertex list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MESH_VERTEX_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);

            //Fill the vertex list with all the vertex
            length = file.ReadInt32();
            _verts = new List<TerrainMeshVertex>(length);
            startOfList = file.BaseStream.Position;
            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * meshVertexStruct.structSize), SeekOrigin.Begin);
                //Add it to the list
                _verts.Add(new TerrainMeshVertex(meshVertexStruct, file));
            }



            /*----------MESH EDGES----------*/
            //Get the reference to the mesh edge list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MESH_EDGE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);

            //Fill the edge list with all the edge
            length = file.ReadInt32();
            _edges = new List<TerrainMeshEdge>(length);
            startOfList = file.BaseStream.Position;
            TerrainMeshEdge tempMeshEdge;

            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * meshEdgeStruct.structSize), SeekOrigin.Begin);
                //Create the edge
                tempMeshEdge = new TerrainMeshEdge(meshEdgeStruct, file);
                //Get the actual index in the array of the mesh vert
                for (int j = 0; j < _mapVerts.Count; j++)
                {
                    if (_verts[j].id == tempMeshEdge.startVertexIndex)
                    {
                        tempMeshEdge.startVertexIndex = j;
                        break;
                    }
                }
                //Add it to the list
                _edges.Add(tempMeshEdge);
            }




            /*----------MESH FACES----------*/
            //Get the reference to the mesh face list and go there
            file.BaseStream.Seek(binaryFile.dataOffset + infoStruct.fields[MESH_FACE_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            file.BaseStream.Seek(binaryFile.dataOffset + reference, SeekOrigin.Begin);
            //Fill the face list with all the faces
            length = file.ReadInt32();
            _faces = new List<TerrainMeshFace>(length);
            startOfList = file.BaseStream.Position;
            TerrainMeshFace tempMeshFace;

            for (int i = 0; i < length; i++)
            {
                //Seek to the next struct in the list
                file.BaseStream.Seek(startOfList + (i * meshFaceStruct.structSize), SeekOrigin.Begin);
                //Make the face
                tempMeshFace = new TerrainMeshFace(meshFaceStruct, file);

                //Find the actual edge indices
                for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
                {
                    for (int j = 0; j < _edges.Count; j++)
                    {
                        if (tempMeshFace[edgeIndex] == _edges[j].id)
                        {
                            tempMeshFace[edgeIndex] = j;
                            break;
                        }
                    }
                }
                //Find the actual map face index
                for (int j = 0; j < _mapFaces.Count; j++)
                {
                    if (tempMeshFace.mapId == _mapFaces[j].id)
                    {
                        tempMeshFace.mapId = j;
                        break;
                    }
                }
                //Add it to the list
                _faces.Add(tempMeshFace);
            }
            binaryFile.Close();

            _chunks = new MeshChunk[1]; 
            Triangle[] tris = new Triangle[faces.Count];
            //Hold temporary vertex values
            Vector3 x, y, z;
            Vector2 u, v, w;

            //For each faces make a triangle
            for (int i = 0; i < faces.Count; i++)
            {
                //Get the mesh coordinates
                x = verts[edges[faces[i].edges[0]].startVertexIndex].position;
                y = verts[edges[faces[i].edges[1]].startVertexIndex].position;
                z = verts[edges[faces[i].edges[2]].startVertexIndex].position;

                //Get the map coordinates (lightmap coordinates are the same)
                u = mapVerts[mapEdges[mapFaces[faces[i].mapId].edges[0]].startVertexIndex].position.Xy;
                v = mapVerts[mapEdges[mapFaces[faces[i].mapId].edges[1]].startVertexIndex].position.Xy;
                w = mapVerts[mapEdges[mapFaces[faces[i].mapId].edges[2]].startVertexIndex].position.Xy;

                //Make the triangle
                tris[i] = new Triangle(x, y, z, u, v, w, u, v, w);
            }
            _chunks[0] = new MeshChunk(tris);
            _chunks[0].casts = true;
            _chunks[0].receives = true;
        }

        public BiowareModel toModel(uint modelID)
        {
            //Initialize the model without a mesh
            BiowareModel terrainModel = new BiowareModel(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), binaryFile.path, modelID, true);
            //Make an array to hold all the triangles
            
            //Set the mesh for the model
            ModelHierarchy terrainHierarchy = new ModelHierarchy(this,"Sector"+sectorID);
            terrainModel.hierarchy = terrainHierarchy;
            return terrainModel;
        }

        private void setStructDefinitions()
        {
            for (int i = 0; i < binaryFile.structs.Length; i++)
            {
                switch (binaryFile.structs[i].type)
                {
                    case GFFSTRUCTTYPE.TRN_MAP_EDGE:
                        mapEdgeStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_MAP_FACE:
                        mapFaceStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_MAP_VERT:
                        mapVertexStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_MESH_EDGE:
                        meshEdgeStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_MESH_FACE:
                        meshFaceStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_MESH_VERT:
                        meshVertexStruct = binaryFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TRN_AREA_INFO:
                        infoStruct = binaryFile.structs[i]; break;
                }
            }
        }
    }
}
