using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class World
{
    readonly object dataLock = new object();

    bool m_worldLoop;
    int m_chunkNb;
    Chunk[] m_chunks;

    public World(int chunkNb, bool worldLoop)
    {
        m_worldLoop = worldLoop;
        m_chunkNb = chunkNb;

        m_chunks = new Chunk[m_chunkNb * m_chunkNb];

        for (int i = 0; i < m_chunkNb; i++)
            for (int j = 0; j < m_chunkNb; j++)
                m_chunks[ChunkPosToIndex(i, j)] = new Chunk(this, i, j);
    }

    public int size { get { return m_chunkNb * Chunk.chunkSize; } }
    public int chunkNb { get { return m_chunkNb; } }
    public bool worldLoop { get { return m_worldLoop; } }

    public Chunk GetChunk(int x, int z)
    {
        return m_chunks[ChunkPosToIndex(x, z)];
    }

    public Chunk GetChunkAt(int x, int z)
    {
        int chunkX;
        int chunkZ;
        PosToChunkPos(x, z, out chunkX, out chunkZ);

        return GetChunk(chunkX, chunkZ);
    }

    public BlockData GetBlock(int x, int y, int z)
    {
        int chunkX;
        int chunkZ;
        int blockX;
        int blockZ;
        PosToBlockPosAndChunkPos(x, z, out blockX, out blockZ, out chunkX, out chunkZ);

        var chunk = GetChunk(chunkX, chunkZ);
        return chunk.GetBlock(blockX, y, blockZ);
    }

    public void SetBlock(int x, int y, int z, BlockData block, bool updateData = true)
    {
        if (updateData)
        {
            //create a local 5*5*5 matrix
            var matrix = GetLocalMatrix(x - 2, y - 2, z - 2, 5, 5, 5);
            var view = new MatrixView<BlockData>(matrix, 2, 2, 2);
            view.Set(0, 0, 0, block);

            Vector3Int[] updatePositions = new Vector3Int[]
            {
                //center
                new Vector3Int(2, 2, 2),
                //cross
                new Vector3Int(2, 1, 2),
                new Vector3Int(1, 2, 2),
                new Vector3Int(2, 2, 1),
                new Vector3Int(3, 2, 2),
                new Vector3Int(2, 2, 3),
                new Vector3Int(2, 3, 2),
                //borders
                new Vector3Int(1, 1, 2), //down
                new Vector3Int(2, 1, 1),
                new Vector3Int(3, 1, 2),
                new Vector3Int(2, 1, 3),
                new Vector3Int(1, 2, 1), //midle
                new Vector3Int(3, 2, 1),
                new Vector3Int(1, 2, 3),
                new Vector3Int(3, 2, 3),
                new Vector3Int(1, 3, 2), //up
                new Vector3Int(2, 3, 1),
                new Vector3Int(3, 3, 2),
                new Vector3Int(2, 3, 3),
                //corners
                new Vector3Int(1, 1, 1), //down
                new Vector3Int(1, 1, 3),
                new Vector3Int(3, 1, 1),
                new Vector3Int(3, 1, 3),
                new Vector3Int(1, 3, 1), //up
                new Vector3Int(1, 3, 3),
                new Vector3Int(3, 3, 1),
                new Vector3Int(3, 3, 3),
            };
            
            foreach(var pos in updatePositions)
            {
                view.SetPos(pos.x, pos.y, pos.z);
                view.Set(0, 0, 0, BlockTypeList.instance.Get(view.GetCenter().id).UpdateBlock(view));
            }

            //only update the 3*3* center of the matrix
            SetBlocks(x - 1, y - 1, z - 1, matrix, 1, 1, 1, 3, 3, 3, false);
        }
        else
        {
            int chunkX;
            int chunkZ;
            int blockX;
            int blockZ;
            PosToBlockPosAndChunkPos(x, z, out blockX, out blockZ, out chunkX, out chunkZ);

            var chunk = GetChunk(chunkX, chunkZ);
            Debug.Assert(chunk != null);

            lock (dataLock)
            {
                chunk.SetBlock(blockX, y, blockZ, block);
            }
        }
    }

    public void SetBlocks(int x, int y, int z, Matrix<BlockData> blocks, bool updateData = true)
    {
        SetBlocks(x, y, z, blocks, 0, 0, 0, blocks.width, blocks.height, blocks.width, updateData);
    }

    public void SetBlocks(int x, int y, int z, Matrix<BlockData> blocks, int blocksX, int blocksY, int blocksZ, int blocksSizeX, int blocksSizeY, int blocksSizeZ, bool updateData = true)
    {
        Debug.Assert(blocksX + blocksSizeX <= blocks.width);
        Debug.Assert(blocksY + blocksSizeY <= blocks.height);
        Debug.Assert(blocksZ + blocksSizeZ <= blocks.depth);

        if (updateData)
        {
            Debug.Assert(false);
            //todo i'm too lasy to do that shit now
        }
        else
        {
            lock (dataLock)
            {
                int maxX = x + blocksSizeX - 1;
                int maxZ = z + blocksSizeZ - 1;

                int minChunkX;
                int minChunkZ;
                int maxChunkX;
                int maxChunkZ;

                PosToUnclampedChunkPos(x, z, out minChunkX, out minChunkZ);
                PosToUnclampedChunkPos(maxX, maxZ, out maxChunkX, out maxChunkZ);

                for (int i = minChunkX; i <= maxChunkX; i++)
                {
                    for (int j = minChunkZ; j <= maxChunkZ; j++)
                    {
                        int currentMinX;
                        int currentMinZ;
                        int currentMaxX;
                        int currentMaxZ;

                        BlockPosInChunkToPos(0, 0, i, j, out currentMinX, out currentMinZ);
                        BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, i, j, out currentMaxX, out currentMaxZ);

                        int localMinX = 0;
                        int localMinZ = 0;
                        int localMaxX = Chunk.chunkSize - 1;
                        int localMaxZ = Chunk.chunkSize - 1;

                        int tileMinX = 0;
                        int tileMinZ = 0;

                        if (currentMinX < x)
                            localMinX = x - currentMinX;
                        else tileMinX = currentMinX - x;
                        if (currentMinZ < z)
                            localMinZ = z - currentMinZ;
                        else tileMinZ = currentMinZ - z;

                        if (localMaxX - localMinX + 1 > blocksSizeX - tileMinX)
                            localMaxX = blocksSizeX + localMinX - 1 - tileMinX;
                        if (localMaxZ - localMinZ + 1 > blocksSizeZ - tileMinZ)
                            localMaxZ = blocksSizeZ + localMinZ - 1 - tileMinZ;

                        var chunk = GetChunk(i, j);

                        for (int m = 0; m < blocksSizeY; m++)
                            for (int k = 0; k <= localMaxX - localMinX; k++)
                                for (int l = 0; l <= localMaxZ - localMinZ; l++)
                                    chunk.SetBlock(localMinX + k, y + m, localMinZ + l, blocks.Get(blocksX + k + tileMinX, blocksY + m, blocksZ + l + tileMinZ));
                    }
                }
            }
        }
    }

    public int GetTopBlockHeight(int x, int z)
    {
        int chunkX;
        int chunkZ;
        int blockX;
        int blockZ;
        PosToBlockPosAndChunkPos(x, z, out blockX, out blockZ, out chunkX, out chunkZ);

        var chunk = GetChunk(chunkX, chunkZ);
        Debug.Assert(chunk != null);
        return chunk.GetTopBlockHeight(blockX, blockZ);
    }

    public int GetBottomBlockHeight(int x, int z)
    {
        int chunkX;
        int chunkZ;
        int blockX;
        int blockZ;
        PosToBlockPosAndChunkPos(x, z, out blockX, out blockZ, out chunkX, out chunkZ);

        var chunk = GetChunk(chunkX, chunkZ);
        Debug.Assert(chunk != null);
        return chunk.GetBottomBlockHeight(blockX, blockZ);
    }

    public void GetBlockNeighbors(int x, int y, int z, BlockNeighbors b)
    {
        int realSize = b.size * 2 + 1;

        int minX = x - b.size;
        int minZ = z - b.size;
        int maxX = x + b.size;
        int maxZ = z + b.size;

        int minChunkX;
        int minChunkZ;
        int maxChunkX;
        int maxChunkZ;

        PosToUnclampedChunkPos(minX, minZ, out minChunkX, out minChunkZ);
        PosToUnclampedChunkPos(maxX, maxZ, out maxChunkX, out maxChunkZ);

        for (int i = minChunkX; i <= maxChunkX; i++)
        {
            for (int j = minChunkZ; j <= maxChunkZ; j++)
            {
                int currentMinX;
                int currentMinZ;
                int currentMaxX;
                int currentMaxZ;

                BlockPosInChunkToPos(0, 0, i, j, out currentMinX, out currentMinZ);
                BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, i, j, out currentMaxX, out currentMaxZ);

                int localMinX = 0;
                int localMinZ = 0;
                int localMaxX = Chunk.chunkSize - 1;
                int localMaxZ = Chunk.chunkSize - 1;

                int tileMinX = 0;
                int tileMinZ = 0;

                if (currentMinX < minX)
                    localMinX = minX - currentMinX;
                else tileMinX = currentMinX - minX;
                if (currentMinZ < minZ)
                    localMinZ = minZ - currentMinZ;
                else tileMinZ = currentMinZ - minZ;

                if (localMaxX - localMinX + 1 > realSize - tileMinX)
                    localMaxX = realSize + localMinX - 1 - tileMinX;
                if (localMaxZ - localMinZ + 1 > realSize - tileMinZ)
                    localMaxZ = realSize + localMinZ - 1 - tileMinZ;

                var chunk = GetChunk(i, j);

                for (int m = -b.height; m <= b.height; m++)
                {
                    int layerIndex, blockY;
                    chunk.HeightToLayerAndBlock(y + m, out layerIndex, out blockY);

                    var layer = chunk.GetLayer(layerIndex);

                    for (int k = 0; k <= localMaxX - localMinX; k++)
                    {
                        for (int l = 0; l <= localMaxZ - localMinZ; l++)
                        {
                            if (layer == null)
                                b.SetBlock(k + tileMinX - b.size, m, l + tileMinZ - b.size, BlockData.GetDefault());
                            else b.SetBlock(k + tileMinX - b.size, m, l + tileMinZ - b.size, layer.GetBlock(localMinX + k, blockY, localMinZ + l));
                        }
                    }
                }
            }
        }
    }

    public BlockNeighbors GetBlockNeighbors(int x, int y, int z, int size, int height = 0)
    {
        //todo optimize layers to do the same than chunk (not get multiple time the same layer)

        BlockNeighbors b = new BlockNeighbors(size, height);

        GetBlockNeighbors(x, y, z, b);

        return b;
    }

    public void GetLocalMatrix(int x, int y, int z, Matrix<BlockData> mat)
    {
        lock (dataLock)
        {
            //todo optimize layers to do the same than chunk (not get multiple time the same layer)

            int maxX = x + mat.width - 1;
            int maxZ = z + mat.depth - 1;

            int minChunkX;
            int minChunkZ;
            int maxChunkX;
            int maxChunkZ;

            PosToUnclampedChunkPos(x, z, out minChunkX, out minChunkZ);
            PosToUnclampedChunkPos(maxX, maxZ, out maxChunkX, out maxChunkZ);

            for (int i = minChunkX; i <= maxChunkX; i++)
            {
                for (int j = minChunkZ; j <= maxChunkZ; j++)
                {
                    int currentMinX;
                    int currentMinZ;
                    int currentMaxX;
                    int currentMaxZ;

                    BlockPosInChunkToPos(0, 0, i, j, out currentMinX, out currentMinZ);
                    BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, i, j, out currentMaxX, out currentMaxZ);

                    int localMinX = 0;
                    int localMinZ = 0;
                    int localMaxX = Chunk.chunkSize - 1;
                    int localMaxZ = Chunk.chunkSize - 1;

                    int tileMinX = 0;
                    int tileMinZ = 0;

                    if (currentMinX < x)
                        localMinX = x - currentMinX;
                    else tileMinX = currentMinX - x;
                    if (currentMinZ < z)
                        localMinZ = z - currentMinZ;
                    else tileMinZ = currentMinZ - z;

                    if (localMaxX - localMinX + 1 > mat.width - tileMinX)
                        localMaxX = mat.width + localMinX - 1 - tileMinX;
                    if (localMaxZ - localMinZ + 1 > mat.depth - tileMinZ)
                        localMaxZ = mat.depth + localMinZ - 1 - tileMinZ;

                    var chunk = GetChunk(i, j);

                    for (int m = 0; m < mat.height; m++)
                    {
                        int layerIndex, blockY;
                        chunk.HeightToLayerAndBlock(y + m, out layerIndex, out blockY);

                        var layer = chunk.GetLayer(layerIndex);

                        for (int k = 0; k <= localMaxX - localMinX; k++)
                        {
                            for (int l = 0; l <= localMaxZ - localMinZ; l++)
                            {
                                if (layer == null)
                                    mat.Set(k + tileMinX, m, l + tileMinZ, BlockData.GetDefault());
                                else mat.Set(k + tileMinX, m, l + tileMinZ, layer.GetBlock(localMinX + k, blockY, localMinZ + l));
                            }
                        }
                    }
                }
            }
        }
    }

    public Matrix<BlockData> GetLocalMatrix(int x, int y, int z, int width, int depth)
    {
        return GetLocalMatrix(x, y, z, width, 1, depth);
    }

    public Matrix<BlockData> GetLocalMatrix(int x, int y, int z, int width, int height, int depth)
    {
        Matrix<BlockData> mat = new Matrix<BlockData>(width, height, depth);

        GetLocalMatrix(x, y, z, mat);

        return mat;
    }

    void ClampChunkPos(int x, int z, out int outX, out int outZ)
    {
        if(!m_worldLoop)
        {
            outX = x;
            outZ = z;
        }
        else
        {
            if (x < 0)
                outX = (x % m_chunkNb + m_chunkNb) % m_chunkNb;
            else outX = x % m_chunkNb;
            if (z < 0)
                outZ = (z % m_chunkNb + m_chunkNb) % m_chunkNb;
            else outZ = z % m_chunkNb;
        }
    }

    public int ChunkPosToIndex(int x, int z)
    {
        if (m_worldLoop)
        {
            if (x < 0)
                x = (x % m_chunkNb + m_chunkNb) % m_chunkNb;
            else x = x % m_chunkNb;
            if (z < 0)
                z = (z % m_chunkNb + m_chunkNb) % m_chunkNb;
            else z = z % m_chunkNb;

        }
        Debug.Assert(x >= 0 && x < m_chunkNb && z >= 0 && z < m_chunkNb);

        return x * m_chunkNb + z;
    }

    public void PosToUnclampedChunkPos(int x, int z, out int outX, out int outZ)
    {
        if (x < 0)
            outX = (x + 1) / Chunk.chunkSize - 1;
        else outX = x / Chunk.chunkSize;
        if (z < 0)
            outZ = (z + 1) / Chunk.chunkSize - 1;
        else outZ = z / Chunk.chunkSize;
    }

    public void PosToChunkPos(int x, int z, out int outX, out int outZ)
    {
        if(m_worldLoop)
        {
            int worldSize = size;
            if (x < 0)
                x = (x % worldSize + worldSize) % worldSize;
            else x = x % worldSize;
            if (z < 0)
                z = (z % worldSize + worldSize) % worldSize;
            else z = z % worldSize;
        }
        Debug.Assert(x >= 0 && z >= 0);

        x = x / Chunk.chunkSize;
        z = z / Chunk.chunkSize;

        Debug.Assert(x < m_chunkNb && z < m_chunkNb);

        outX = x;
        outZ = z;
    }

    public void PosToBlockPosInChunk(int x, int z, out int outX, out int outZ)
    {
        int worldSize = size;

        if (m_worldLoop)
        {
            if (x < 0)
                x = (x % worldSize + worldSize) % worldSize;
            else x = x % worldSize;
            if (z < 0)
                z = (z % worldSize + worldSize) % worldSize;
            else z = z % worldSize;
        }
        Debug.Assert(x >= 0 && z >= 0 && x < worldSize && z < worldSize);

        outX = x % Chunk.chunkSize;
        outZ = z % Chunk.chunkSize;
    }

    public void PosToBlockPosAndChunkPos(int x, int z, out int outBlockX, out int outBlockZ, out int outChunkX, out int outChunkZ)
    {
        int worldSize = m_chunkNb * Chunk.chunkSize;

        if (m_worldLoop)
        {
            if (x < 0)
                x = (x % worldSize + worldSize) % worldSize;
            else x = x % worldSize;
            if (z < 0)
                z = (z % worldSize + worldSize) % worldSize;
            else z = z % worldSize;
        }
        Debug.Assert(x >= 0 && z >= 0 && x < worldSize && z < worldSize);

        outChunkX = x / Chunk.chunkSize;
        outChunkZ = z / Chunk.chunkSize;

        Debug.Assert(outChunkX < m_chunkNb && outChunkZ < m_chunkNb);

        outBlockX = x % Chunk.chunkSize;
        outBlockZ = z % Chunk.chunkSize;
    }

    public void BlockPosInChunkToPos(int x, int z, int chunkX, int chunkZ, out int outX, out int outZ)
    {
        Debug.Assert(x >= 0 && x < Chunk.chunkSize && z >= 0 && z < Chunk.chunkSize);
        Debug.Assert(m_worldLoop || (chunkX >= 0 && chunkX < m_chunkNb && chunkZ >= 0 && chunkZ < m_chunkNb));

        outX = x + chunkX * Chunk.chunkSize;
        outZ = z + chunkZ * Chunk.chunkSize;
    }
}