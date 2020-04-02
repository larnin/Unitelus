using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BlockFace
{
    Left,   //x+
    Right,  //x-
    Up,     //y+
    Down,   //y-
    Front,  //z+
    Back,   //z-
}

public abstract class BlockTypeBase
{
    [SerializeField] int m_id;

    public int id { get { return m_id; } }

    public BlockTypeBase(int id)
    {
        m_id = id;
    }

    public abstract void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams);

    public abstract bool IsFaceFull(BlockFace face);

    static public Vector3 FaceToDir(BlockFace face)
    {
        switch(face)
        {
            case BlockFace.Left:
                return new Vector3(1, 0, 0);
            case BlockFace.Right:
                return new Vector3(-1, 0, 0);
            case BlockFace.Up:
                return new Vector3(0, 1, 0);
            case BlockFace.Down:
                return new Vector3(0, -1, 0);
            case BlockFace.Front:
                return new Vector3(0, 0, 1);
            case BlockFace.Back:
                return new Vector3(0, 0, -1);
        }
        Debug.Assert(false);
        return new Vector3(0, 0, 0);
    }

    public static BlockFace DirToFace(Vector3 dir)
    {
        bool xpy = dir.x + dir.y > 0;
        bool xmy = dir.x - dir.y > 0;
        bool xpz = dir.x + dir.z > 0;
        bool xmz = dir.x - dir.z > 0;
        bool zpy = dir.z + dir.y > 0;
        bool zmy = dir.z - dir.y > 0;

        if (xpy && xmy && xpz && xmz)
            return BlockFace.Left;
        if (!xpy && !xmy && !xpz && !xmz)
            return BlockFace.Right;
        if (xpy && !xmy && zpy && !zmy)
            return BlockFace.Up;
        if (!xpy && xmy && !zpy && zmy)
            return BlockFace.Down;
        if (xpz && !xmz && zpy && zmy)
            return BlockFace.Front;
        if (!xpz && xmz && !zpy && !zmy)
            return BlockFace.Back;

        Debug.Assert(false);
        return BlockFace.Front;

    }
}
