using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Chunk
{
    public const int chunkSize = 16;

    World m_world;
    BlockData[] m_blocks = new BlockData[chunkSize * chunkSize];

    public Chunk(World world)
    {
        m_world = world;
    }

    public BlockData GetBlock(int x, int y)
    {
        Debug.Assert(x >= 0 && x < chunkSize && y >= 0 && y < chunkSize);

        return m_blocks[PosToIndex(x, y)];
    }

    public void SetBlock(int x, int y, BlockData bloc)
    {
        Debug.Assert(x >= 0 && x < chunkSize && y >= 0 && y < chunkSize);

        m_blocks[PosToIndex(x, y)] = bloc;
    }

    int PosToIndex(int x, int y)
    {
        return x * chunkSize + y;
    }
}
