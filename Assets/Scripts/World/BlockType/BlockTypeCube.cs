using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockTypeCube : BlockTypeBase
{
    [SerializeField]
    public Material m_material;

    BlockRendererData m_data;

    public BlockTypeCube(int id) : base(id)
    {
    }

    public override bool IsFaceFull(BlockFace face)
    {
        //full all the times
        return true;
    }

    public override bool IsFull()
    {
        return true;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        if(m_data == null)
            m_data = new BlockRendererData(m_material);

        BlockRenderer.DrawCubic(pos, neighbors, meshParams, m_data);
    }
}
