using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BlockNeighbors
{
    int m_size;
    int m_height;
    BlockData[] m_blocks;

    public BlockNeighbors(int size, int height = 0)
    {
        m_size = size;
        m_height = height;

        m_blocks = new BlockData[(size * 2 + 1) * (size * 2 + 1) * (height * 2 + 1)];
    }

    public BlockData GetCurrent()
    {
        return GetBlock(0, 0, 0);
    }

    public BlockData GetBlock(int x, int y, int z = 0)
    {
        return m_blocks[PosToIndex(x, y, z)];
    }

    public void SetBlock(int x, int y, int z, BlockData block)
    {
        m_blocks[PosToIndex(x, y, z)] = block;
    }

    public void SetBlock(int x, int y, BlockData block)
    {
        m_blocks[PosToIndex(x, y)] = block;
    }

    int PosToIndex(int x, int y, int z = 0)
    {
        Debug.Assert(x >= -m_size && x <= m_size && y >= -m_size && y <= m_size && z >= -m_height && z <= m_height);

        return ((x + m_size) * (2 * m_size + 1) + (y + m_size)) * (2 * m_size + 1) + z;
    }
}

