﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerRenderPassBlocks : LayerRendererPassBase
{
    static Matrix<BlockData> m_matrix = new Matrix<BlockData>(Chunk.chunkSize + 2, Chunk.chunkSize + 2, Chunk.chunkSize + 2);
    static MatrixView<BlockData> m_view = new MatrixView<BlockData>();

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

        m_view.mat = m_matrix;

        for(int i = 0; i < Chunk.chunkSize; i++)
            for(int j = 0; j < Chunk.chunkSize; j++)
                for(int k = 0; k < Chunk.chunkSize; k++)
                {
                    m_view.SetPos(i + 1, j + 1, k + 1);
                    int centerID = m_matrix.Get(i + 1, j + 1, k + 1).id;
                    if (centerID == 0)
                        continue;

                    Vector3 pos = new Vector3(i, j, k);

                    if (PlaceholderBlockInfos.instance.m_blockRenderer.Count >= centerID)
                        PlaceholderBlockInfos.instance.m_blockRenderer[centerID - 1].Render(pos, m_view, meshParams);
                }
    }
}