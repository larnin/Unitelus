using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkView
{
    Matrix<ChunkLayer> m_layers;
    Vector3Int m_position;

    public ChunkView(Vector3Int position, Vector3Int size)
    {
        m_layers = new Matrix<ChunkLayer>(size.x, size.y, size.y);
        m_position = position;
    }

    public void SetChunkLayer(Vector3Int pos, ChunkLayer chunk)
    {
        m_layers.Set(pos.x, pos.y, pos.z, chunk);
    }

    public void SetChunkLayer(int x, int y, int z, ChunkLayer chunk)
    {
        m_layers.Set(x, y, z, chunk);
    }

    public ChunkLayer GetChunkLayer(Vector3Int pos)
    {
        return m_layers.Get(pos.x, pos.y, pos.z);
    }

    public ChunkLayer GetChunkLayer(int x, int y, int z)
    {
        return m_layers.Get(x, y, z);
    }

    public BlockData GetBlock(Vector3Int pos)
    {
        return GetBlock(pos.x, pos.y, pos.z);
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        return GetBlockWithoutOffset(x - m_position.x, y - m_position.y, z - m_position.z);
    }

    BlockData GetBlockWithoutOffset(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
            return BlockData.GetDefault();

        int chunkSize = Chunk.chunkSize;

        Vector3Int chunkPos = new Vector3Int(x / chunkSize, y / chunkSize, z / chunkSize);
        if (chunkPos.x >= m_layers.width || chunkPos.y >= m_layers.height || chunkPos.z >= m_layers.depth)
            return BlockData.GetDefault();

        var layer = GetChunkLayer(chunkPos);
        if (layer == null)
            return BlockData.GetDefault();

        Vector3Int posInChunk = new Vector3Int(x - chunkPos.x * chunkSize, y - chunkPos.y * chunkSize, z - chunkPos.z * chunkSize);

        return layer.GetBlock(posInChunk.x, posInChunk.y, posInChunk.z);
    }

    public Vector3Int GetPos()
    {
        return m_position;
    }

    public Vector3Int GetChunkNb()
    {
        return m_layers.size;
    }

    public Vector3Int GetSize()
    {
        return m_layers.size * Chunk.chunkSize;
    }
}
