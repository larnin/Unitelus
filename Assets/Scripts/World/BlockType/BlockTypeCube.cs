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

        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(1, 0, 0).id).IsFaceFull(BlockFace.Right), BlockFace.Left);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(-1, 0, 0).id).IsFaceFull(BlockFace.Left), BlockFace.Right);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(0, 1, 0).id).IsFaceFull(BlockFace.Down), BlockFace.Up);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(0, -1, 0).id).IsFaceFull(BlockFace.Up), BlockFace.Down);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(0, 0, 1).id).IsFaceFull(BlockFace.Back), BlockFace.Front);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(neighbors.Get(0, 0, -1).id).IsFaceFull(BlockFace.Front), BlockFace.Back);

        BlockRenderer.DrawCubic(pos, meshParams, m_data);
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        return neighbors.Get(0, 0, 0);
    }
}
