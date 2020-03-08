using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderPassBlocks : ChunkRendererPassBase
{
    public override RendererData[] Render(Chunk c, float scaleX, float scaleY, float scaleZ)
    {
        var renders = new List<RendererData>();
        var layers = c.GetLayers();

        int min = int.MinValue;
        int max = int.MaxValue;

        foreach(var l in layers)
        {
            min = Mathf.Min(min, l);
            max = Mathf.Max(max, l);
        }

        var world = c.world;

        int minX, minY;
        world.BlockPosInChunkToPos(Chunk.chunkSize - 1, Chunk.chunkSize - 1, c.x - 1, c.y - 1, out minX, out minY);

        var mat = world.GetLocalMatrix(minX, minY, min, Chunk.chunkSize + 2, Chunk.chunkSize + 2, max - min + 1);
        Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

        for(int i = 0; i < Chunk.chunkSize; i++)
            for(int j = 0; j < Chunk.chunkSize; j++)
                for(int k = 0; k < max - min; k++)
                {
                    BlockNeighbors b = BlockNeighbors.FromMatrix(mat, i + 1, j + 1, k, 1, 1);
                    int centerID = b.GetCurrent().id;
                    if (centerID == 0)
                        continue;

                    Vector3 pos = new Vector3(i * scaleX, j * scaleY, min + k * scaleZ);

                    foreach (var block in PlaceholderBlockInfos.instance.m_blockRenderer)
                    {
                        if(block.id == centerID)
                        {
                            var data = block.Render(pos, scale, b);

                            var r = renders.Find(value => { return value.material == data.material; });
                            if (r == null)
                                renders.Add(data);
                            else r.Merge(data);

                            break;
                        }
                    }
                }
        
        return renders.ToArray();
    }
}
