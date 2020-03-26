using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerRenderPassBlocks : LayerRendererPassBase
{
    static Matrix<BlockData> m_matrix = new Matrix<BlockData>(Chunk.chunkSize + 2, Chunk.chunkSize + 2, Chunk.chunkSize + 2);
    static BlockNeighbors m_blockNeighbors = new BlockNeighbors(1, 1);

    public override void Render(Chunk c, int x, int y, int z, MeshParams<WorldVertexDefinition> meshParams)
    {
        var layer = c.GetLayer(y);

        if (layer == null)
            return;

        var world = c.world;

        int minX, minY, minZ;
        world.BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, c.x - 1, c.z - 1, out minX, out minZ);
        minY = c.LayerToHeight(y - 1, Chunk.chunkSize - 1);

        world.GetLocalMatrix(minX, minY, minZ, m_matrix);

        for(int i = 0; i < Chunk.chunkSize; i++)
            for(int j = 0; j < Chunk.chunkSize; j++)
                for(int k = 0; k < Chunk.chunkSize; k++)
                {
                    BlockNeighbors.FromMatrix(m_matrix, i + 1, j + 1, k, m_blockNeighbors);
                    int centerID = m_blockNeighbors.GetCurrent().id;
                    if (centerID == 0)
                        continue;

                    Vector3 pos = new Vector3(i, j, k);

                    if (PlaceholderBlockInfos.instance.m_blockRenderer.Count >= centerID)
                        PlaceholderBlockInfos.instance.m_blockRenderer[centerID - 1].Render(pos, m_blockNeighbors, meshParams);
                }
    }
}
