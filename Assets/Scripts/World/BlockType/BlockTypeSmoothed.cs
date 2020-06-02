using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/* Data : 8bits
 * [0-1] - Rotation
 * [2-6] - Shape type : up to 32 type
 * [7] - Don't override
 * */

public class BlockTypeSmoothed : BlockTypeBase
{
    public enum ShapeType
    {
        Cubic,
        AntiTetrahedral,
        HalfCubic,
        HorizontalHalfCubic,
        Tetrahedral,
        SmallPyramid,
    }

    class BlockShape
    {
        //0 == empty / 1 == full / 2 == any
        public int[] states;
        public ShapeType shape;
        public Rotation rotation;

        public BlockShape(int[] _states, ShapeType _shape, Rotation _rotation = Rotation.Rot0)
        {
            states = new int[27];
            for (int i = 0; i < states.Length && i < _states.Length; i++)
                states[i] = _states[i];

            shape = _shape;
            rotation = _rotation;
        }

        public int GetState(int x, int y, int z, Rotation rot = Rotation.Rot0)
        {
            if (Mathf.Abs(x) > 1 || Mathf.Abs(y) > 1 || Mathf.Abs(z) > 1)
                return 2;

            var pos = RotationEx.RotateOffset(new Vector2Int(x, z), rot);

            return states[PosToIndex(pos.x, y, pos.y)];
        }

        int PosToIndex(int x, int y, int z)
        {
            x++;
            y++;
            z++;
            return x + z * 3 + y * 9;
        }
    }

    [SerializeField]
    public Material m_material;

    BlockRendererData m_data;

    static List<BlockShape> m_shapes = GenerateShapes();

    public BlockTypeSmoothed(int id) : base(id)
    {

    }
    
    public override bool IsFaceFull(BlockFace face, byte data = 0)
    {
        ShapeType shape = GetShapeTypeData(data);
        Rotation rotation = GetRotationData(data);

        face = BlockFaceEx.RotateInv(face, rotation);

        if (shape == ShapeType.Cubic)
            return true;

        if (shape == ShapeType.AntiTetrahedral)
            return face == BlockFace.Down || face == BlockFace.Front || face == BlockFace.Left;

        if(shape == ShapeType.HalfCubic)
            return face == BlockFace.Down || face == BlockFace.Back;

        return false;
    }

    public override bool IsFull()
    {
        return false;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        var block = neighbors.GetCenter();
        ShapeType shape = GetShapeTypeData(block.data);
        Rotation rotation = GetRotationData(block.data);

        if (m_data == null)
            m_data = new BlockRendererData(id, m_material);
            m_data.SetFaceUV(new Rect(0.25f, 0, 0.25f, 1)
                , new Rect(0.5f, 0, 0.25f, 1)
                , new Rect(0.0f, 0, 0.25f, 1));
        m_data.rotation = rotation;

        SetDrawFacesFromNeighbors(m_data, neighbors);

        switch (shape)
        {
            case ShapeType.AntiTetrahedral:
                BlockRenderer.DrawAntiTetrahedral(pos, meshParams, m_data);
                break;
            case ShapeType.Cubic:
                BlockRenderer.DrawCubic(pos, meshParams, m_data);
                break;
            case ShapeType.HalfCubic:
                BlockRenderer.DrawHalfCubic(pos, meshParams, m_data);
                break;
            case ShapeType.HorizontalHalfCubic:
                BlockRenderer.DrawHorizontalHalfCubic(pos, meshParams, m_data);
                break;
            case ShapeType.SmallPyramid:
                BlockRenderer.DrawSmallPyramid(pos, meshParams, m_data);
                break;
            case ShapeType.Tetrahedral:
                BlockRenderer.DrawThetrahedral(pos, meshParams, m_data);
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    void SetDrawFacesFromNeighbors(BlockRendererData data, MatrixView<BlockData> neighbors)
    {
        var left = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Left));
        var right = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Right));
        var up = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Up));
        var down = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Down));
        var front = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Front));
        var back = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Back));
        var current = neighbors.GetCenter();

        bool drawLeft = !BlockTypeList.instance.Get(left.id).IsFaceFull(BlockFace.Right, left.data);
        bool drawRight = !BlockTypeList.instance.Get(right.id).IsFaceFull(BlockFace.Left, right.data);
        bool drawUp = !BlockTypeList.instance.Get(up.id).IsFaceFull(BlockFace.Down, up.data);
        bool drawDown = !BlockTypeList.instance.Get(down.id).IsFaceFull(BlockFace.Up, down.data);
        bool drawFront = !BlockTypeList.instance.Get(front.id).IsFaceFull(BlockFace.Back, front.data);
        bool drawBack = !BlockTypeList.instance.Get(back.id).IsFaceFull(BlockFace.Front, back.data);

        m_data.SetFaceDraw(drawLeft, BlockFace.Left);
        m_data.SetFaceDraw(drawRight, BlockFace.Right);
        m_data.SetFaceDraw(drawUp, BlockFace.Up);
        m_data.SetFaceDraw(drawDown, BlockFace.Down);
        m_data.SetFaceDraw(drawFront, BlockFace.Front);
        m_data.SetFaceDraw(drawBack, BlockFace.Back);

        ShapeType shape = GetShapeTypeData(current.data);
        Rotation rotation = GetRotationData(current.data);
        
        if(shape == ShapeType.HalfCubic && false)
        {
            var leftFace = BlockFaceEx.Rotate(BlockFace.Left, rotation);
            var rightFace = BlockFaceEx.Rotate(BlockFace.Right, rotation);

            if(m_data.GetFaceDraw(leftFace))
            {
                var leftDir = BlockFaceEx.FaceToDirInt(leftFace);
                var leftBlock = neighbors.Get(leftDir.x, leftDir.y, leftDir.z);

                ShapeType leftShape = GetShapeTypeData(leftBlock.data);
                Rotation leftRotation = GetRotationData(leftBlock.data);

                if (leftShape == ShapeType.HalfCubic && leftRotation == rotation)
                    m_data.SetFaceDraw(false, leftFace);

                if (leftShape == ShapeType.Tetrahedral && leftRotation == rotation)
                    m_data.SetFaceDraw(false, leftFace);
            }

            if(m_data.GetFaceDraw(rightFace))
            {
                var rightDir = BlockFaceEx.FaceToDirInt(rightFace);
                var rightBlock = neighbors.Get(rightDir.x, rightDir.y, rightDir.z);

                ShapeType rightShape = GetShapeTypeData(rightBlock.data);
                Rotation rightRotation = GetRotationData(rightBlock.data);

                if (rightShape == ShapeType.HalfCubic && rightRotation == rotation)
                    m_data.SetFaceDraw(false, rightFace);

                if (rightShape == ShapeType.Tetrahedral && RotationEx.SubRotations(rightRotation, Rotation.Rot90) == rotation)
                    m_data.SetFaceDraw(false, rightFace);
            }
        }
    }

    public static void GetBlockType(MatrixView<BlockData> neighbors, out ShapeType shape, out Rotation rotation)
    {
        ushort id = neighbors.GetCenter().id;

        Matrix<bool> blocks = new Matrix<bool>(3, 3, 3);

        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    var b = neighbors.Get(i, j, k);

                    blocks.Set(i + 1, j + 1, k + 1, b.id == id || BlockTypeList.instance.Get(b.id).IsFull());
                }

        foreach (var b in m_shapes)
        {
            foreach (var rot in Enum.GetValues(typeof(Rotation)))
            {
                bool validBlock = true;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            bool block = blocks.Get(i + 1, j + 1, k + 1);
                            int state = b.GetState(i, j, k, (Rotation)rot);
                            if (state == 2)
                                continue;

                            if ((state == 0 && !block) || (state == 1 && block))
                                continue;

                            validBlock = false;
                            break;
                        }
                        if (!validBlock)
                            break;
                    }
                    if (!validBlock)
                        break;
                }

                if (validBlock)
                {
                    shape = b.shape;
                    rotation = RotationEx.SubRotations(b.rotation, (Rotation)rot);
                    return;
                }
            }
        }
        
        Debug.Assert(false);
        shape = ShapeType.Cubic;
        rotation = Rotation.Rot0;
    }

    public override BlockData UpdateBlock(MatrixView<BlockData> neighbors)
    {
        var data = neighbors.GetCenter();

        if(IsNoOverrideData(data.data))
            return neighbors.GetCenter();

        ShapeType shape;
        Rotation rotation;

        GetBlockType(neighbors, out shape, out rotation);
        data.data = MakeData(rotation, shape, false);
        return data;
    }

    public static byte MakeData(Rotation rot, ShapeType shape, bool noOverride)
    {
        byte b = 0;
        b |= (byte)rot;
        b |= (byte)((byte)shape << 2);
        if (noOverride)
            b |= 1 << 7;

        return b;
    }

    public static Rotation GetRotationData(byte data)
    {
        return (Rotation)(data & 0b00000011);
    }

    public static ShapeType GetShapeTypeData(byte data)
    {
        return (ShapeType)((data & 0b01111100) >> 2);
    }

    public static bool IsNoOverrideData(byte data)
    {
        return (data & 0b10000000) != 0;
    }


    static List<BlockShape> GenerateShapes()
    {
        List<BlockShape> shapes = new List<BlockShape>();

        //shapes.Add(new BlockShape(new int[]
        //    {2,2,2,2,2,2,2,2,2
        //    ,2,0,2,0,2,0,2,0,2
        //    ,2,2,2,2,0,2,2,2,2}
        //    , ShapeType.SmallPyramid));

        shapes.Add(new BlockShape(new int[]
            {2,2,2,2,2,2,2,2,2
            ,2,1,2,1,2,1,2,0,2
            ,2,2,2,2,0,2,2,2,2}
            , ShapeType.HalfCubic));

        shapes.Add(new BlockShape(new int[]
            {2,2,2,2,2,2,2,2,2
            ,2,0,2,1,2,0,2,1,2
            ,2,2,2,2,1,2,2,2,2}
            , ShapeType.HorizontalHalfCubic));

        shapes.Add(new BlockShape(new int[]
            {2,2,2,2,2,2,2,2,2
            ,2,1,2,0,2,1,2,0,2
            ,2,2,2,2,0,2,2,2,2}
            , ShapeType.Tetrahedral));

        shapes.Add(new BlockShape(new int[]
            {2,2,2,2,2,2,2,2,2
            ,1,1,0,1,2,1,1,1,1
            ,2,0,2,2,2,0,2,2,2}
            , ShapeType.AntiTetrahedral));

        shapes.Add(new BlockShape(new int[] 
            {2,2,2,2,2,2,2,2,2
            ,2,2,2,2,2,2,2,2,2
            ,2,2,2,2,2,2,2,2,2}
            , ShapeType.Cubic));

        return shapes;
    }
}
