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
    Water,
}

public abstract class BlockTypeBase : ScriptableObject
{
    [SerializeField] BlockID m_id;
    [SerializeField] BlockType m_type;
    [SerializeField] float m_pathWeight = 1;
    [SerializeField] bool m_canWalkThrough = false;
    [SerializeField] bool m_canWalkOn = false;
    [SerializeField] bool m_canFloatThrough = false;

    public BlockID id { get { return m_id; } set { m_id = value; } }
    public BlockType type { get { return m_type; } protected set { m_type = value; } }

    public float pathWeight { get { return m_pathWeight; } set { m_pathWeight = value; } }
    public bool canWalkThrough { get { return m_canWalkThrough; } protected set { m_canWalkThrough = value; } }
    public bool canWalkOn { get { return m_canWalkOn; } protected set { m_canWalkOn = value; } }
    public bool canFloatTurough { get { return m_canFloatThrough; } protected set { m_canFloatThrough = value; } }

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
