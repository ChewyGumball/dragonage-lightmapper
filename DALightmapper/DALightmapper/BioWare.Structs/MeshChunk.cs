using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Bioware.Files;

using Ben;

using DALightmapper;

namespace Bioware.Structs
{
    class MeshChunk
    {
        String _name;
        uint _vertexSize;
        uint _vertexCount;
        uint _vertexOffset;
        uint _indexCount;
        uint _indexOffset;
        int _positionOffset;
        int _textureOffset;
        int _texture2Offset;
        int _normalOffset;
        Boolean _usesTwoTexCoords;

        Triangle[] _tris;
        double _area;

        String _materialObjectName;
        Boolean _casts, _receives;
        uint _textureID;
        uint _lightmapID;

        public String name
        {
            get { return _name; }
        }
        public uint vertexSize
        {
            get { return _vertexSize; }
        }
        public uint vertexCount
        {
            get { return _vertexCount; }
        }
        public uint indexCount
        {
            get { return _indexCount; }
        }
        public int positionOffset
        {
            get { return _positionOffset; }
        }
        public int textureOffset
        {
            get { return _textureOffset; }
        }
        public int texture2Offset
        {
            get 
            { 
                if (usesTwoTexCoords) 
                    return _texture2Offset;
                else 
                    return _textureOffset; 
            }
        }
        public int normalOffset
        {
            get { return _normalOffset; }
        }
        public Boolean usesTwoTexCoords
        {
            get { return _usesTwoTexCoords; }
        }
        public uint startIndex
        {
            get { return _indexOffset; }
        }
        public uint vertexOffset
        {
            get { return _vertexOffset; }
        }

        public Triangle[] tris
        {
            get { return _tris; }
        }
        public double area
        {
            get { return _area; }
            set { _area = value; }
        }
        public Boolean casts
        {
            get { return _casts; }
            set { _casts = value; }
        }
        public Boolean receives
        {
            get { return _receives; }
            set { _receives = value; }
        }


        public String materialObjectName
        {
            get { return _materialObjectName; }
            set { _materialObjectName = value; }
        }
        public uint textureID
        {
            get { return _textureID; }
            set { _textureID = value; }
        }
        public uint lightmapID
        {
            get { return _lightmapID; }
            set { _lightmapID = value; }
        }
        
        //Already made triangles constructor(terrain meshes)
        public MeshChunk(Triangle[] triangles)
        {
            _tris = triangles;
            _usesTwoTexCoords = triangles[0].isLightmapped;
            for (int i = 0; i < _tris.Length; i++)
            {
                _area += 0.5 * Vector3.Cross(tris[i].y - tris[i].x, tris[i].z - tris[i].x).Length;
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
            _name = IOUtilities.readECString(file, dataOffset + file.ReadInt32());


            //Seek to vertex size offset and read it
            file.BaseStream.Seek(chunkDef.fields[1].index + position, SeekOrigin.Begin);
            _vertexSize = file.ReadUInt32();

            //Seek to vertex count offset and read it
            file.BaseStream.Seek(chunkDef.fields[2].index + position, SeekOrigin.Begin);
            _vertexCount = file.ReadUInt32();

            //Seek to vertex offset offset and read it
            file.BaseStream.Seek(chunkDef.fields[7].index + position, SeekOrigin.Begin);
            _vertexOffset = file.ReadUInt32();

            //Seek to index count offset and read it
            file.BaseStream.Seek(chunkDef.fields[3].index + position, SeekOrigin.Begin);
            _indexCount = file.ReadUInt32();
            
            //Seek to index offset offset and read it
            file.BaseStream.Seek(chunkDef.fields[10].index + position, SeekOrigin.Begin);
            _indexOffset = file.ReadUInt32();

            //Seek to vertex declarator offset and read in the list reference
            file.BaseStream.Seek(chunkDef.fields[13].index + position, SeekOrigin.Begin);
            reference = file.ReadUInt32();

            //Make the triangle array to be filled later
            _tris = new Triangle[_indexCount / 3];

            //Seek to the list
            file.BaseStream.Seek(reference + dataOffset, SeekOrigin.Begin);
            listLength = file.ReadUInt32();
            reference = file.BaseStream.Position;
            _usesTwoTexCoords = false;
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
                        _positionOffset = file.ReadInt32();
                        break;

                    case Usage.TEXCOORD:
                        int offset = file.ReadInt32();
                        file.BaseStream.Seek(reference + (vertStruct.structSize * i) + vertStruct.fields[4].index, SeekOrigin.Begin);
                        uint index = file.ReadUInt32();
                        if (index == 0)
                            _textureOffset = offset;
                        else if (index == 1)
                        {
                            _texture2Offset = offset;
                            _usesTwoTexCoords = true;
                        }
                        break;

                    case Usage.NORMAL:
                        _normalOffset = file.ReadInt32();
                        break;
                }
            }
        }
    }
}
