using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockTypeWater : BlockTypeBase
{
    [SerializeField]
    public Material m_material;
    [SerializeField]
    public Rect m_UV;

    BlockRendererData m_data;

    public BlockTypeWater() : base()
    {
        type = BlockType.Water;
        canWalkOn = false;
        canWalkThrough = true;
        canFloatTurough = true;
    }

    public override bool IsFaceFull(BlockFace face, byte data = 0)
    {
        return false;
    }

    public override bool IsFull()
    {
        return false;
    }

    public override bool IsEmpty()
    {
        return true;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        float waterHeight = -1 / 8.0f;
        pos.y += waterHeight;

        if (m_data == null)
        {
            m_data = new BlockRendererData(id, m_material);
            m_data.SetFaceUV(m_UV);
            m_data.rotation = Rotation.Rot0;
            m_data.SetFaceDraw(true, false, false);
        }

        if (neighbors.Get(0, 1, 0).id != neighbors.GetCenter().id)
            BlockRenderer.DrawCubic(pos, meshParams, m_data);
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        return neighbors.GetCenter();
    }
}