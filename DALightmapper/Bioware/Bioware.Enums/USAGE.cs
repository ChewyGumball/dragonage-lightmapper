namespace Bioware.Enums
{
    public enum Usage : uint
    {
        /// Position
        POSITION = 0,
        /// Blend weights
        BLENDWEIGHT = 1,
        /// Blend indices
        BLENDINDICES = 2,
        /// Normal
        NORMAL = 3,
        /// Point Size 
        PSIZE = 4,
        /// Texture coordinates
        TEXCOORD = 5,
        /// Tangent vector
        TANGENT = 6,
        /// binormal vector
        BINORMAL = 7,
        /// tessellation factor
        TESSFACTOR = 8,
        /// PositionT 
        POSITIONT = 9,
        /// color channel
        COLOR = 10,
        /// fog value
        FOG = 11,
        /// depth 
        DEPTH = 12,
        /// sample
        SAMPLE = 13,
        // error/other/unset
        UNUSED = 0xffffffff
    }
}
