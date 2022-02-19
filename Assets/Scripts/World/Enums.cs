using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public enum Rotation
{
    Rot0,
    Rot90,
    Rot180,
    Rot270
}

public static class RotationEx
{
    public static Vector2Int RotationToDir(Rotation rot)
    {
        switch(rot)
        {
            case Rotation.Rot0:
                return new Vector2Int(0, 1);
            case Rotation.Rot90:
                return new Vector2Int(-1, 0);
            case Rotation.Rot180:
                return new Vector2Int(-0, -1);
            case Rotation.Rot270:
                return new Vector2Int(1, 0);
            default:
                break;
        }

        Assert.IsTrue(false);
        return new Vector2Int(0, 0);
    }

    static int rotationCount = Enum.GetValues(typeof(Rotation)).Length;
    public static Rotation AddRotations(Rotation a, Rotation b)
    {
        int rot = (int)a + (int)b;
        if (rot >= rotationCount)
            rot -= rotationCount;
        return (Rotation)rot;
    }

    public static Rotation SubRotations(Rotation a, Rotation b)
    {
        int rot = (int)a - (int)b;
        if (rot < 0)
            rot += rotationCount;
        return (Rotation)rot;
    }

    public static Vector2Int RotateOffset(Vector2Int offset, Rotation rot)
    {
        if (rot == Rotation.Rot0)
            return offset;
        else if(rot == Rotation.Rot90)
        {
            var temp = offset.x;
            offset.x = -offset.y;
            offset.y = temp;
        }
        else if(rot == Rotation.Rot180)
        {
            offset.x *= -1;
            offset.y *= -1;
        }
        else // Rot270
        {
            var temp = -offset.x;
            offset.x = offset.y;
            offset.y = temp;
        }

        return offset;
    }

    public static Vector2 RotateOffset(Vector2 offset, Rotation rot)
    {
        if (rot == Rotation.Rot0)
            return offset;
        else if (rot == Rotation.Rot90)
        {
            var temp = offset.x;
            offset.x = -offset.y;
            offset.y = temp;
        }
        else if (rot == Rotation.Rot180)
        {
            offset.x *= -1;
            offset.y *= -1;
        }
        else // Rot270
        {
            var temp = -offset.x;
            offset.x = offset.y;
            offset.y = temp;
        }

        return offset;
    }
}

public enum BlockFace
{
    Left,   //x-
    Right,  //x+
    Up,     //y+
    Down,   //y-
    Front,  //z+
    Back,   //z-
}

public static class BlockFaceEx
{
    static public Vector3 FaceToDir(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.Left:
                return new Vector3(-1, 0, 0);
            case BlockFace.Right:
                return new Vector3(1, 0, 0);
            case BlockFace.Up:
                return new Vector3(0, 1, 0);
            case BlockFace.Down:
                return new Vector3(0, -1, 0);
            case BlockFace.Front:
                return new Vector3(0, 0, 1);
            case BlockFace.Back:
                return new Vector3(0, 0, -1);
        }
        Assert.IsTrue(false);
        return new Vector3(0, 0, 0);
    }

    static public Vector3Int FaceToDirInt(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.Left:
                return new Vector3Int(-1, 0, 0);
            case BlockFace.Right:
                return new Vector3Int(1, 0, 0);
            case BlockFace.Up:
                return new Vector3Int(0, 1, 0);
            case BlockFace.Down:
                return new Vector3Int(0, -1, 0);
            case BlockFace.Front:
                return new Vector3Int(0, 0, 1);
            case BlockFace.Back:
                return new Vector3Int(0, 0, -1);
        }
        Assert.IsTrue(false);
        return new Vector3Int(0, 0, 0);
    }

    public static BlockFace DirToFace(Vector3 dir)
    {
        //fix that shit whith new orientations if needed

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

        Assert.IsTrue(false);
        return BlockFace.Front;
    }

    public static BlockFace Rotate(BlockFace face, Rotation rot)
    {
        if (face == BlockFace.Up || face == BlockFace.Down)
            return face;
        while (rot > Rotation.Rot0)
        {
            switch(face)
            {
                case BlockFace.Back:
                    face = BlockFace.Right;
                    break;
                case BlockFace.Right:
                    face = BlockFace.Front;
                    break;
                case BlockFace.Front:
                    face = BlockFace.Left;
                    break;
                case BlockFace.Left:
                    face = BlockFace.Back;
                    break;
                default:
                    Assert.IsTrue(false);
                    break;
            }

            rot--;
        }

        return face;
    }

    //rotate in the other direction
    public static BlockFace RotateInv(BlockFace face, Rotation rot)
    {
        if (face == BlockFace.Up || face == BlockFace.Down)
            return face;
        while (rot > Rotation.Rot0)
        {
            switch (face)
            {
                case BlockFace.Back:
                    face = BlockFace.Left;
                    break;
                case BlockFace.Right:
                    face = BlockFace.Back;
                    break;
                case BlockFace.Front:
                    face = BlockFace.Right;
                    break;
                case BlockFace.Left:
                    face = BlockFace.Front;
                    break;
                default:
                    Assert.IsTrue(false);
                    break;
            }

            rot--;
        }

        return face;
    }
}