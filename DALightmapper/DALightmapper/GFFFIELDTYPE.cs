namespace Bioware.Structs
{
    enum GFFFIELDTYPE : ushort
    {
        UINT8 = 0,
        INT8 = 1,
        UINT16 = 2,
        INT16 = 3,
        UINT32 = 4,
        INT32 = 5,
        UINT64 = 6,
        INT64 = 7,
        FLOAT32 = 8,
        FLOAT64 = 9,
        Vector3f = 10,
        Vector4f = 12,
        Quaternionf = 13,
        ECString = 14,
        Color4f = 15,
        Matrix4x4f = 16,
        TlkString = 17,
        GenericList = 0xffff
    };
}
