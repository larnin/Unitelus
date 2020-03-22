using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerRenderPassBlocks : LayerRendererPassBase
{
    public override void Render(Chunk c, int x, int y, int z, float scaleX, float scaleY, float scaleZ, MeshParams<WorldVertexDefinition> meshParams)
    {
        var layer = c.GetLayer(y);

        if (layer == null)
            return;

        var world = c.world;

        int minX, minY, minZ;
        world.BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, c.x - 1, c.z - 1, out minX, out minZ);
        minY = c.LayerToHeight(y - 1, Chunk.chunkSize - 1);

        var mat = world.GetLocalMatrix(minX, minY, minZ, Chunk.chunkSize + 2, Chunk.chunkSize + 2, Chunk.chunkSize + 2);
        Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

        for(int i = 0; i < Chunk.chunkSize; i++)
            for(int j = 0; j < Chunk.chunkSize; j++)
                for(int k = 0; k < Chunk.chunkSize; k++)
                {
                    BlockNeighbors b = BlockNeighbors.FromMatrix(mat, i + 1, j + 1, k, 1, 1);
                    int centerID = b.GetCurrent().id;
                    if (centerID == 0)
                        continue;

                    Vector3 pos = new Vector3(i * scaleX, j * scaleY, k * scaleZ);

                    foreach (var block in PlaceholderBlockInfos.instance.m_blockRenderer)
                    {
                        if(block.id == centerID)
                        {
                            block.Render(pos, scale, b, meshParams);
                            break;
                        }
                    }
                }
    }
}
