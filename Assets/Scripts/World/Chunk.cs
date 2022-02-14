using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkLayer
{
    BlockData[] m_blocks = new BlockData[Chunk.chunkSize * Chunk.chunkSize * Chunk.chunkSize];
    int m_blockNb = 0;
    float m_updateTime = 0;

    public float updateTime { get { return m_updateTime; } }
    public int blockNb { get { return m_blockNb; } }

    public ChunkLayer()
    {
        BlockData empty = BlockData.GetDefault();
        for (int i = 0; i < Chunk.chunkSize * Chunk.chunkSize * Chunk.chunkSize; i++)
            m_blocks[i] = empty;
    }

    public bool IsEmpty()
    {
        return m_blockNb <= 0;
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < Chunk.chunkSize && y >= 0 && y < Chunk.chunkSize && z >= 0 && z < Chunk.chunkSize);

        return m_blocks[PosToIndex(x, y, z)];
    }

    public void SetBlock(int x, int y, int z, BlockData bloc)
    {
        Debug.Assert(x >= 0 && x < Chunk.chunkSize && y >= 0 && y < Chunk.chunkSize && z >= 0 && z < Chunk.chunkSize);

        BlockData empty = BlockData.GetDefault();
        int index = PosToIndex(x, y, z);

        if (m_blocks[index] != empty)
            m_blockNb--;

        if (bloc != empty)
            m_blockNb++;

        m_blocks[index] = bloc;

        m_updateTime = TimeEx.GetTime();
    }

    int PosToIndex(int x, int y, int z)
    {
        return (x * Chunk.chunkSize + y) * Chunk.chunkSize + z;
    }
}

public class Chunk
{
    public const int chunkScale = 4;
    public const int chunkSize = 1 << chunkScale;

    World m_world = null;
    int m_x = 0;
    int m_z = 0;

    Dictionary<int, ChunkLayer> m_layers = new Dictionary<int, ChunkLayer>();
    BiomeType[] m_biomes = new BiomeType[chunkSize * chunkSize];

    public World world { get { return m_world; } }
    public int x { get { return m_x; } }
    public int z { get { return m_z; } }

    public Chunk(World world, int x, int z)
    {
        m_world = world;
        m_x = x;
        m_z = z;
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        int layer, block;
        HeightToLayerAndBlock(y, out layer, out block);

        if(m_layers.ContainsKey(layer))
        {
            return m_layers[layer].GetBlock(x, block, z);
        }
        return BlockData.GetDefault();
    }

    public void SetBlock(int x, int y, int z, BlockData block)
    {
        int l, b;
        HeightToLayerAndBlock(y, out l, out b);

        if (m_layers.ContainsKey(l))
        {
            var layer = m_layers[l];

            layer.SetBlock(x, b, z, block);

            if(layer.IsEmpty())
                m_layers.Remove(l);
        }
        else
        {
            if (block == BlockData.GetDefault())
                return;
            var layer = new ChunkLayer();
            layer.SetBlock(x, b, z, block);
            m_layers.Add(l, layer);
        }
    }

    public ChunkLayer GetLayerAt(int y)
    {
        return GetLayer(HeightToLayer(y));
    }

    public bool HaveLayerAt(int y)
    {
        return HaveLayer(HeightToLayer(y));
    }

    public ChunkLayer GetLayer(int layer)
    {
        if (m_layers.ContainsKey(layer))
            return m_layers[layer];
        return null;
    }

    public bool HaveLayer(int layer)
    {
        return m_layers.ContainsKey(layer);
    }

    public bool HaveLayer()
    {
        return m_layers.Count > 0;
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

    public int GetTopLayerIndex()
    {
        Debug.Assert(HaveLayer());

        int height = int.MinValue;

        foreach(var l in m_layers)
            height = Mathf.Max(l.Key, height);

        return height;
    }

    public int GetTopBlockHeight(int x, int z)
    {
        Debug.Assert(HaveLayer());

        if (!HaveLayer())
            return int.MinValue;

        int topLayerIndex = GetTopLayerIndex();
        int bottomLayerIndex = GetBottomLayerIndex();
        int layerIndex;
        int height = 0;
        bool found = false;
        for (layerIndex = topLayerIndex; layerIndex >= bottomLayerIndex; layerIndex--)
        {
            ChunkLayer layer = null;
            if (!m_layers.TryGetValue(layerIndex, out layer))
                continue;

            height = 0;
            for (height = chunkSize - 1; height >= 0; height--)
            {
                if (layer.GetBlock(x, height, z) != BlockData.GetDefault())
                {
                    found = true;
                    break;
                }
            }
            if (found)
                break;
        }

        Debug.Assert(height >= 0);

        return LayerToHeight(layerIndex, height);
    }

    public int GetBottomLayerIndex()
    {
        Debug.Assert(HaveLayer());

        int height = int.MaxValue;

        foreach (var l in m_layers)
            height = Mathf.Min(l.Key, height);

        return height;
    }

    public int GetBottomBlockHeight(int x, int z)
    {
        Debug.Assert(HaveLayer());

        if (!HaveLayer())
            return int.MaxValue;

        int topLayerIndex = GetTopLayerIndex();
        int bottomLayerIndex = GetBottomLayerIndex();
        int layerIndex;
        int height = 0;
        bool found = false;
        for (layerIndex = bottomLayerIndex; layerIndex <= topLayerIndex; layerIndex++)
        {
            var layer = m_layers[layerIndex];

            height = 0;
            for (height = 0; height < chunkSize; height++)
            {
                if (layer.GetBlock(x, height, z) != BlockData.GetDefault())
                {
                    found = true;
                    break;
                }
            }
            if (found)
                break;
        }

        Debug.Assert(height >= 0);

        return LayerToHeight(layerIndex, height);
    }

    public List<int> GetLayersUptatedAfter(float time)
    {
        List<int> layers = new List<int>();

        foreach(var l in m_layers)
        {
            if (l.Value.updateTime > time)
                layers.Add(l.Key);
        }

        return layers;
    }

    public int HeightToLayer(int height)
    {
        if (height < 0)
            return (height - chunkSize + 1) / chunkSize;
        return height / chunkSize;
    }
    
    public int HeightToBlockInLayer(int height)
    {
        if (height < 0)
            return (height % chunkSize + chunkSize) % chunkSize;
        return height % chunkSize;
    }

    public void HeightToLayerAndBlock(int height, out int layer, out int block)
    {
        if(height < 0)
        {
            layer = (height - chunkSize + 1) / chunkSize;
            block = (height % chunkSize + chunkSize) % chunkSize;
        }
        else
        {
            layer = height / chunkSize;
            block = height % chunkSize;
        }
    }

    public int LayerToHeight(int layer, int block = 0)
    {
        Debug.Assert(block >= 0 && block < chunkSize);

        return layer * chunkSize + block;
    }

    public void SetBiome(int x, int z, BiomeType biome)
    {
        m_biomes[PosToBiomeIndex(x, z)] = biome;
    }

    public BiomeType GetBiome(int x, int z)
    {
        return m_biomes[PosToBiomeIndex(x, z)];
    }

    int PosToBiomeIndex(int x, int z)
    {
        Debug.Assert(x >= 0 && z >= 0 && x < chunkSize && z < chunkSize);

        return x * chunkSize + z;
    }
}
