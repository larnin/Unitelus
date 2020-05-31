using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockTypeEmpty : BlockTypeBase
{
    public BlockTypeEmpty(int id) : base(id)
    {

    }

    public override bool IsFaceFull(BlockFace face, byte data = 0)
    {
        return false;
    }

    public override bool IsFull()
    {
        return false;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        //do nothing here, it's an empty block
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        return neighbors.GetCenter();
    }
}
