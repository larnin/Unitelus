using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BlockType
{
    NoType,
    Cube,
    Empty,
    Smoothed,
}

public abstract class BlockTypeBase
{
    [SerializeField] int m_id;
    [SerializeField] BlockType m_type;

    public int id { get { return m_id; } }
    public BlockType type { get { return m_type; } protected set { m_type = value; } }

    public BlockTypeBase()
    {
        m_id = 0;
        m_type = BlockType.NoType;
    }

    public abstract void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams);

    public abstract bool IsFaceFull(BlockFace face, byte data = 0);
    public abstract bool IsFull();
    public abstract bool IsEmpty(); //no collision

    public abstract BlockData UpdateBlock(MatrixView<BlockData> neighbors);
}
