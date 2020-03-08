using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockNeighbors
{
    int m_size;
    int m_height;
    BlockData[] m_blocks;

    public int size { get { return m_size; } }

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

    BlockNeighbors GetBlockNeighbors(int x, int y, int z, int size, int height = 0)
    {
        BlockNeighbors b = new BlockNeighbors(size, height);

        for (int i = -size; i <= size; i++)
            for (int j = -size; j <= size; j++)
                for (int k = -height; k <= height; k++)
                    if (x + i >= -m_size && x + i <= m_size && y + j >= -m_size && y + j <= m_size && z + k >= -m_height && z + k <= m_height)
                        b.SetBlock(i, j, k, GetBlock(x + i, y + j, z + k));

        return b;
    }

    public static BlockNeighbors FromMatrix(Matrix<BlockData> mat, int x, int y, int size)
    {
        return FromMatrix(mat, x, y, 0, size, 0);
    }

    public static BlockNeighbors FromMatrix(Matrix<BlockData> mat, int x, int y, int z, int size, int height)
    {
        BlockNeighbors b = new BlockNeighbors(size, height);

        for(int i = -size; i <= size; i++)
            for(int j = -size; j <= size; j++)
                for(int k = -height; k <= height; k++)
                {
                    int realX = x + i;
                    int realY = y + j;
                    int realZ = z + k;

                    if (realX < 0 || realX >= mat.width || realY < 0 || realY >= mat.depth || realZ < 0 || realZ >= mat.height)
                        b.SetBlock(i, j, k, BlockData.GetDefault());
                    else b.SetBlock(i, j, k, mat.Get(realX, realY, realZ));
                }

        return b;
    }
}

