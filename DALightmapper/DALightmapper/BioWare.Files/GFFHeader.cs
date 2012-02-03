using System;
using System.IO;

namespace Bioware.Files
{
    public class GFFHeader
    {
        String _magicNumber;
        String _versionNumber;
        String _targetPlatform;
        String _fileType;
        String _fileVersion;
        uint _structCount;
        uint _dataOffset;


        public String magicNumber
        {
            get { return _magicNumber; }
        }
        public String versionNumber
        {
            get { return _versionNumber; }
        }
        public String targetPlatform
        {
            get { return _targetPlatform; }
        }
        public String fileType
        {
            get { return _fileType; }
        }
        public String fileVersion
        {
            get { return _fileVersion; }
        }
        public uint structCount
        {
            get { return _structCount; }
        }
        public uint dataOffset
        {
            get { return _dataOffset; }
        }

        public GFFHeader(BinaryReader file)
        {
            _magicNumber = new String(file.ReadChars(4));
            _versionNumber = new String(file.ReadChars(4));
            _targetPlatform = new String(file.ReadChars(4));
            _fileType = new String(file.ReadChars(4));
            _fileVersion = new String(file.ReadChars(4));
            _structCount = file.ReadUInt32();
            _dataOffset = file.ReadUInt32();
        }
    }
}
