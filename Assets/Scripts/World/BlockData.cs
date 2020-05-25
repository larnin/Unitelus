using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct BlockData
{
    public UInt16 id;
    public byte data;

    BlockData(UInt16 _id, byte _data = 0)
    {
        id = _id;
        data = _data;
    }

    public static BlockData GetDefault()
    {
        BlockData b;
        b.id = 0;
        b.data = 0;

        return b;
    }

    public static bool operator ==(BlockData a, BlockData b)
    {
        return a.id == b.id && a.data == b.data;
    }

    public static bool operator !=(BlockData a, BlockData b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is BlockData))
        {
            return false;
        }

        var data = (BlockData)obj;
        return this == data;
    }

    public override int GetHashCode()
    {
        var hashCode = 1213751429;
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        hashCode = hashCode * -1521134295 + data.GetHashCode();
        return hashCode;
    }
}