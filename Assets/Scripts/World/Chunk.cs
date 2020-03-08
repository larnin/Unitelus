using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkLayer
{
    BlockData[] m_blocks = new BlockData[Chunk.chunkSize * Chunk.chunkSize];
    int m_blockNb = 0;

    public int GetBlockNb()
    {
        return m_blockNb;
    }

    public bool IsEmpty()
    {
        return m_blockNb <= 0;
    }

    public BlockData GetBlock(int x, int y)
    {
        Debug.Assert(x >= 0 && x < Chunk.chunkSize && y >= 0 && y < Chunk.chunkSize);

        return m_blocks[PosToIndex(x, y)];
    }

    public void SetBlock(int x, int y, BlockData bloc)
    {
        Debug.Assert(x >= 0 && x < Chunk.chunkSize && y >= 0 && y < Chunk.chunkSize);

        BlockData empty = BlockData.GetDefault();
        int index = PosToIndex(x, y);

        if (m_blocks[index] != empty)
            m_blockNb--;

        if (bloc != empty)
            m_blockNb++;

        m_blocks[PosToIndex(x, y)] = bloc;
    }

    int PosToIndex(int x, int y)
    {
        return x * Chunk.chunkSize + y;
    }
}

public class Chunk
{
    public const int chunkSize = 16;

    World m_world = null;
    int m_x = 0;
    int m_y = 0;
    bool m_updated = false;

    Dictionary<int, ChunkLayer> m_layers = new Dictionary<int, ChunkLayer>();

    public World world { get { return m_world; } }
    public int x { get { return m_x; } }
    public int y { get { return m_y; } }
    public bool updated { get { return m_updated; } }

    public Chunk(World world, int x, int y)
    {
        m_world = world;
        m_x = x;
        m_y = y;
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        if(m_layers.ContainsKey(z))
        {
            return m_layers[z].GetBlock(x, y);
        }
        return BlockData.GetDefault();
    }

    public void SetBlock(int x, int y, int z, BlockData block)
    {
        if(m_layers.ContainsKey(z))
        {
            var layer = m_layers[z];

            layer.SetBlock(x, y, block);

            if(layer.IsEmpty())
                m_layers.Remove(z);
        }
        else
        {
            if (block == BlockData.GetDefault())
                return;
            var layer = new ChunkLayer();
            layer.SetBlock(x, y, block);
            m_layers.Add(z, layer);
        }

        m_updated = true;
    }

    public int GetHeight(int x, int y)
    {
        int z = int.MinValue;
        foreach(var l in m_layers)
        {
            if (l.Key > z && l.Value.GetBlock(x, y) != BlockData.GetDefault())
                z = l.Key;
        }

        return z;
    }

    public ChunkLayer GetLayer(int z)
    {
        if (m_layers.ContainsKey(z))
            return m_layers[z];
        return null;
    }

    public bool HaveLayer(int z)
    {
        return m_layers.ContainsKey(z);
    }

    public int[] GetLayers()
    {
        int[] layers = new int[m_layers.Count];
        int index = 0;

        foreach(var l in m_layers)
        {
            layers[index] = l.Key;
            index++;
        }

        return layers;
    }

    public void Rendered()
    {
        m_updated = false;
    }
}
