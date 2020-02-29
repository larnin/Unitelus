using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class World
{
    bool m_bWorldLoop;
    int m_chunkNb;
    Chunk[] m_chunks;

    public World(int chunkNb, bool worldLoop)
    {
        m_bWorldLoop = worldLoop;
        m_chunkNb = chunkNb;

        m_chunks = new Chunk[m_chunkNb * m_chunkNb];
    }

    public Chunk GetChunk(int x, int y)
    {
        return m_chunks[ChunkPosToIndex(x, y)];
    }

    public Chunk GetChunkAt(int x, int y)
    {
        int chunkX;
        int chunkY;
        PosToChunkPos(x, y, out chunkX, out chunkY);

        return GetChunk(chunkX, chunkY);
    }

    public BlockData GetBlock(int x, int y)
    {
        int chunkX;
        int chunkY;
        int blockX;
        int blockY;
        PosToBlockPosAndChunkPos(x, y, out blockX, out blockY, out chunkX, out chunkY);

        var chunk = GetChunkAt(x, y);
        Debug.Assert(chunk != null);
        return chunk.GetBlock(x, y);
    }

    void ClampChunkPos(int x, int y, out int outX, out int outY)
    {
        if(!m_bWorldLoop)
        {
            outX = x;
            outY = y;
        }
        else
        {
            if (x < 0)
                outX = x % m_chunkNb + m_chunkNb;
            else outX = x % m_chunkNb;
            if (y < 0)
                outY = y % m_chunkNb + m_chunkNb;
            else outY = y % m_chunkNb;
        }
    }

    public int ChunkPosToIndex(int x, int y)
    {
        if (m_bWorldLoop)
        {
            if (x < 0)
                x = x % m_chunkNb + m_chunkNb;
            else x = x % m_chunkNb;
            if (y < 0)
                y = y % m_chunkNb + m_chunkNb;
            else y = y % m_chunkNb;
        }
        Debug.Assert(x >= 0 && x < m_chunkNb && y >= 0 && y < m_chunkNb);

        return x * m_chunkNb + y;
    }

    public void PosToChunkPos(int x, int y, out int outX, out int outY)
    {
        if(m_bWorldLoop)
        {
            int worldSize = m_chunkNb * Chunk.chunkSize;
            if (x < 0)
                x = x % worldSize + worldSize;
            else x = x % worldSize;
            if (y < 0)
                y = y % worldSize + worldSize;
            else y = y % worldSize;
        }
        Debug.Assert(x >= 0 && y >= 0);

        x = x / Chunk.chunkSize;
        y = y / Chunk.chunkSize;

        Debug.Assert(x < m_chunkNb && y < m_chunkNb);

        outX = x;
        outY = y;
    }

    public void PosToBlocPosInChunk(int x, int y, out int outX, out int outY)
    {
        int worldSize = m_chunkNb * Chunk.chunkSize;

        if (m_bWorldLoop)
        {
            if (x < 0)
                x = x % worldSize + worldSize;
            else x = x % worldSize;
            if (y < 0)
                y = y % worldSize + worldSize;
            else y = y % worldSize;
        }
        Debug.Assert(x >= 0 && y >= 0 && x < worldSize && y < worldSize);

        x = x % Chunk.chunkSize;
        y = y % Chunk.chunkSize;

        Debug.Assert(x < Chunk.chunkSize && y < Chunk.chunkSize);

        outX = x;
        outY = y;
    }

    public void PosToBlockPosAndChunkPos(int x, int y, out int outBlockX, out int outBlockY, out int outChunkX, out int outChunkY)
    {
        int worldSize = m_chunkNb * Chunk.chunkSize;

        if (m_bWorldLoop)
        {
            if (x < 0)
                x = x % worldSize + worldSize;
            else x = x % worldSize;
            if (y < 0)
                y = y % worldSize + worldSize;
            else y = y % worldSize;
        }
        Debug.Assert(x >= 0 && y >= 0 && x < worldSize && y < worldSize);

        outChunkX = x / Chunk.chunkSize;
        outChunkY = y / Chunk.chunkSize;

        Debug.Assert(x < m_chunkNb && y < m_chunkNb);

        outBlockX = x % Chunk.chunkSize;
        outBlockY = y % Chunk.chunkSize;

        Debug.Assert(x < Chunk.chunkSize && y < Chunk.chunkSize);
    }
}