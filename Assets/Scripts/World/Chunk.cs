using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    World m_world;
    Dictionary<int, ChunkLayer> m_layers = new Dictionary<int, ChunkLayer>();

    public Chunk(World world)
    {
        m_world = world;
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        if(m_layers.ContainsKey(z))
        {
            return m_layers[z].GetBlock(x, y);
        }
        return BlockData.GetDefault();
    }

    public void SetBlock(int x, int y, int z, BlockData bloc)
    {
        if(m_layers.ContainsKey(z))
        {
            var layer = m_layers[z];

            layer.SetBlock(x, y, bloc);

            if(layer.IsEmpty())
                m_layers.Remove(z);
        }
        else
        {
            if (bloc == BlockData.GetDefault())
                return;
            var layer = new ChunkLayer();
            layer.SetBlock(x, y, bloc);
            m_layers.Add(z, layer);
        }
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
}
