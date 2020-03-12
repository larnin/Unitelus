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
    public int height { get { return m_height; } }

    public BlockNeighbors(int size, int height = 0)
    {
        m_size = size;
        m_height = height;

        m_blocks = new BlockData[(size * 2 + 1) * (height * 2 + 1) * (size * 2 + 1)];
    }

    public BlockData GetCurrent()
    {
        return GetBlock(0, 0, 0);
    }

    public BlockData GetBlock(int x, int z)
    {
        return m_blocks[PosToIndex(x, 0, z)];
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        return m_blocks[PosToIndex(x, y, z)];
    }

    public void SetBlock(int x, int y, int z, BlockData block)
    {
        m_blocks[PosToIndex(x, y, z)] = block;
    }

    public void SetBlock(int x, int z, BlockData block)
    {
        m_blocks[PosToIndex(x, z)] = block;
    }

    int PosToIndex(int x, int z)
    {
        return PosToIndex(x, 0, z);
    }

    int PosToIndex(int x, int y, int z)
    {
        Debug.Assert(x >= -m_size && x <= m_size && y >= -m_height && y <= m_height && z >= -m_size && z <= m_size);

        return ((x + m_size) * (2 * m_size + 1) + (y + m_height)) * (2 * m_height + 1) + z + m_size;
    }

    void GetBlockNeighbors(int x, int y, int z, BlockNeighbors neighbors)
    {
        for (int i = -neighbors.size; i <= neighbors.size; i++)
            for (int j = -neighbors.height; j <= neighbors.height; j++)
                for (int k = -neighbors.size; k <= neighbors.size; k++)
                    if (x + i >= -m_size && x + i <= m_size && y + j >= -m_height && y + j <= m_height && z + k >= -m_size && z + k <= m_size)
                        neighbors.SetBlock(i, j, k, GetBlock(x + i, y + j, z + k));
    }

    BlockNeighbors GetBlockNeighbors(int x, int y, int z, int size, int height = 0)
    {
        BlockNeighbors b = new BlockNeighbors(size, height);

        GetBlockNeighbors(x, y, z, b);

        return b;
    }

    public static void FromMatrix(Matrix<BlockData> mat, int x, int z, BlockNeighbors b)
    {
        FromMatrix(mat, x, 0, z, b);
    }

    public static void FromMatrix(Matrix<BlockData> mat, int x, int y, int z, BlockNeighbors b)
    {
        for (int i = -b.size; i <= b.size; i++)
            for (int j = -b.height; j <= b.height; j++)
                for (int k = -b.size; k <= b.size; k++)
                {
                    int realX = x + i;
                    int realY = y + j;
                    int realZ = z + k;

                    if (realX < 0 || realX >= mat.width || realY < 0 || realY >= mat.height || realZ < 0 || realZ >= mat.depth)
                        b.SetBlock(i, j, k, BlockData.GetDefault());
                    else b.SetBlock(i, j, k, mat.Get(realX, realY, realZ));
                }

    }

    public static BlockNeighbors FromMatrix(Matrix<BlockData> mat, int x, int z, int size)
    {
        return FromMatrix(mat, x, 0, z, size, 0);
    }

    public static BlockNeighbors FromMatrix(Matrix<BlockData> mat, int x, int y, int z, int size, int height)
    {
        BlockNeighbors b = new BlockNeighbors(size, height);

        FromMatrix(mat, x, y, z, b);

        return b;
    }
}

