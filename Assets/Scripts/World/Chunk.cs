﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkLayer
{
    BlockData[] m_blocks = new BlockData[Chunk.chunkSize * Chunk.chunkSize * Chunk.chunkSize];
    int m_blockNb = 0;

    public int GetBlockNb()
    {
        return m_blockNb;
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
    }

    int PosToIndex(int x, int y, int z)
    {
        return (x * Chunk.chunkSize + y) * Chunk.chunkSize + z;
    }
}

public class Chunk
{
    public const int chunkSize = 16;

    World m_world = null;
    int m_x = 0;
    int m_z = 0;
    float m_updateTime = 0;

    Dictionary<int, ChunkLayer> m_layers = new Dictionary<int, ChunkLayer>();

    public World world { get { return m_world; } }
    public int x { get { return m_x; } }
    public int z { get { return m_z; } }
    public float updateTime { get { return m_updateTime; } }

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

        m_updateTime = Time.time;
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

        int layerIndex = GetTopLayerIndex();
        var layer = m_layers[layerIndex];

        int i = 0;
        for (i = chunkSize - 1; i >= 0; i--)
        {
            if (layer.GetBlock(x, i, z) != BlockData.GetDefault())
                break;
        }

        Debug.Assert(i >= 0);

        return LayerToHeight(layerIndex, i);
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

        int layerIndex = GetBottomLayerIndex();
        var layer = m_layers[layerIndex];

        int i = 0;
        for (i = 0; i < chunkSize; i++)
        {
            if (layer.GetBlock(x, i, z) != BlockData.GetDefault())
                break;
        }

        Debug.Assert(i < chunkSize);

        return LayerToHeight(layerIndex, i);
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
            return height % chunkSize + chunkSize - 1;
        return height % chunkSize;
    }

    public void HeightToLayerAndBlock(int height, out int layer, out int block)
    {
        if(height < 0)
        {
            layer = (height - chunkSize + 1) / chunkSize;
            block = height % chunkSize + chunkSize - 1;
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
}
