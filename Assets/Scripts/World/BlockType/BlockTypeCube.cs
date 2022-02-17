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
    [SerializeField]
    public BlockUV m_UV;

    BlockRendererData m_data;

    public BlockTypeCube()
    {
        type = BlockType.Cube;
        canWalkOn = true;
        canWalkThrough = false;
        canFloatTurough = false;
    }

    public override bool IsFaceFull(BlockFace face, byte data = 0)
    {
        return true;
    }

    public override bool IsFull()
    {
        return true;
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        if (m_data == null)
        {
            m_data = new BlockRendererData(id, m_material);
            m_UV.FillBlockDataUV(m_data);
        }

        m_data.rotation = (Rotation)(new UniformIntDistribution(0, 4).Next(new StaticRandomGenerator<DefaultRandomGenerator>()));

        var left = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Left));
        var right = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Right));
        var up = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Up));
        var down = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Down));
        var front = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Front));
        var back = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Back));

        m_data.SetFaceDraw(!BlockTypeList.instance.Get(left.id).IsFaceFull(BlockFace.Right, left.data), BlockFace.Left);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(right.id).IsFaceFull(BlockFace.Left, right.data), BlockFace.Right);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(up.id).IsFaceFull(BlockFace.Down, up.data), BlockFace.Up);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(down.id).IsFaceFull(BlockFace.Up, down.data), BlockFace.Down);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(front.id).IsFaceFull(BlockFace.Back, front.data), BlockFace.Front);
        m_data.SetFaceDraw(!BlockTypeList.instance.Get(back.id).IsFaceFull(BlockFace.Front, back.data), BlockFace.Back);

        BlockRenderer.DrawCubic(pos, meshParams, m_data);
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        return neighbors.GetCenter();
    }
}
