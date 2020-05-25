using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class BlockTypeCube : BlockTypeBase
{
    [SerializeField]
    public Material m_material;

    BlockRendererData m_data;

    public BlockTypeCube(int id) : base(id)
    {
    }

    public override bool IsFaceFull(BlockFace face, byte data = 0)
    {
        return true;
    }

    public override bool IsFull()
    {
        return true;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        if (m_data == null)
        {
            m_data = new BlockRendererData(id, m_material);
            m_data.SetFaceUV(new Rect(0.5f, 0, 0.25f, 1)
                , new Rect(0.5f, 0, 0.25f, 1)
                , new Rect(0.25f, 0, 0.25f, 1)
                , new Rect(0, 0, 0.25f, 1)
                , new Rect(0, 0, 0.25f, 1)
                , new Rect(0, 0, 0.25f, 1));
        }

        m_data.rotation = (Rotation)(new UniformIntDistribution(0, 4).Next(new StaticRandomGenerator<DefaultRandomGenerator>()));

        BlockRenderer.DrawCubic(pos, neighbors, meshParams, m_data);
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        return neighbors.Get(0, 0, 0);
    }
}
