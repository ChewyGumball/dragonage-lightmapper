using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Bioware.Structs;
using Bioware.Files;

namespace DALightmapper
{
    class LightMap
    {
        BiowareModel _model;
        MeshChunk _meshChunk;
        Patch[,] _lightMap;
        int _textureID;

        int _dimension;

        public BiowareModel model { get { return _model; } }
        public MeshChunk meshChunk { get { return _meshChunk; } }
        public Patch[,] lightMap { get { return _lightMap; } }
        public int textureID { get { return _textureID; } set { _textureID = value; } }
        public int dimension { get { return _dimension; } }

        public LightMap(BiowareModel m, MeshChunk mc)
        {
            _model = m;
            _meshChunk = mc;
            _dimension = (int)(Math.Sqrt(mc.area) * Settings.pixelsPerUnit);
            _dimension = (int)Math.Pow(2,Math.Ceiling(Math.Log(dimension,2)));
            _lightMap = new Patch[dimension,dimension];
        }
    }
}
