using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct BlockData
{
    public int id;

    BlockData(int _id)
    {
        id = _id;
    }

    public static BlockData GetDefault()
    {
        BlockData b;
        b.id = 0;

        return b;
    }

    public static bool operator ==(BlockData a, BlockData b)
    {
        return a.id == b.id;
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
        return 1877310944 + id.GetHashCode();
    }
}