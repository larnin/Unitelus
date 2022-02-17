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

    class BlockShapeBit
    {
        //x and z are rotation dependant
        public int[] x = new int[Enum.GetValues(typeof(Rotation)).Length];
        public int y;
        public int[] z = new int[Enum.GetValues(typeof(Rotation)).Length];
        public bool full;
    }

    class BlockShape
    {
       public  List<BlockShapeBit> m_bits = new List<BlockShapeBit>();
        
        public ShapeType shape;

        // _states : 0 == empty / 1 == full / 2 == any
        public BlockShape(short[] _states, ShapeType _shape, Rotation _rotation = Rotation.Rot0)
        {
            int rotNb = Enum.GetValues(typeof(Rotation)).Length;
            
            FillBits(_states, _rotation);
            SetBitsFromRotation(_rotation);

            shape = _shape;
        }

        void FillBits(short[] _states, Rotation rot)
        {
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i] > 1 || _states[i] < 0)
                    continue;
                int x = (i % 3) - 1;
                int z = ((i / 3) % 3) - 1;
                int y = (i / 9) - 1;

                BlockShapeBit bit = new BlockShapeBit();
                bit.x[(int)rot] = x;
                bit.y = y;
                bit.z[(int)rot] = z;
                bit.full = _states[i] == 1;

                m_bits.Add(bit);
            }
        }

        void SetBitsFromRotation(Rotation rot)
        {
            int rotNb = Enum.GetValues(typeof(Rotation)).Length;

            for(int i = 0; i < rotNb; i++)
            {
                if (i == (int)rot)
                    continue;

                Rotation offsetRot = RotationEx.SubRotations((Rotation)i, rot);

                foreach(var b in m_bits)
                {
                    var pos = RotationEx.RotateOffset(new Vector2Int(b.x[(int)rot], b.z[(int)rot]), offsetRot);
                    b.x[i] = pos.x;
                    b.z[i] = pos.y;
                }
            }
        }
    }

    [SerializeField]
    public Material m_material;
    [SerializeField]
    public BlockUV m_UV;

    BlockRendererData m_data;

    static List<BlockShape> m_shapes = GenerateShapes();

    public BlockTypeSmoothed() : base()
    {
        type = BlockType.Smoothed;
        canWalkOn = true;
        canWalkThrough = false;
        canFloatTurough = false;
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

        if (shape == ShapeType.HalfCubic)
            return face == BlockFace.Down || face == BlockFace.Back;

        if (shape == ShapeType.HorizontalHalfCubic)
            return face == BlockFace.Back || face == BlockFace.Right;

        return false;
    }

    public override bool IsFull()
    {
        return false;
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        var block = neighbors.GetCenter();
        ShapeType shape = GetShapeTypeData(block.data);
        Rotation rotation = GetRotationData(block.data);

        if (m_data == null)
        {
            m_data = new BlockRendererData(id, m_material);
            m_UV.FillBlockDataUV(m_data);
        }
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

        if (shape == ShapeType.HalfCubic)
        {
            var leftFace = BlockFaceEx.Rotate(BlockFace.Left, rotation);
            var rightFace = BlockFaceEx.Rotate(BlockFace.Right, rotation);

            if (m_data.GetFaceDraw(leftFace))
            {
                var leftBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(leftFace));

                if (BlockTypeList.instance.Get(leftBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType leftShape = GetShapeTypeData(leftBlock.data);
                    Rotation leftRotation = GetRotationData(leftBlock.data);

                    if (leftShape == ShapeType.HalfCubic && leftRotation == rotation)
                        m_data.SetFaceDraw(false, leftFace);

                    if (leftShape == ShapeType.Tetrahedral && leftRotation == rotation)
                        m_data.SetFaceDraw(false, leftFace);

                    if (leftShape == ShapeType.AntiTetrahedral && RotationEx.SubRotations(leftRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, leftFace);
                }
            }

            if (m_data.GetFaceDraw(rightFace))
            {
                var rightBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(rightFace));

                if (BlockTypeList.instance.Get(rightBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType rightShape = GetShapeTypeData(rightBlock.data);
                    Rotation rightRotation = GetRotationData(rightBlock.data);

                    if (rightShape == ShapeType.HalfCubic && rightRotation == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.Tetrahedral && RotationEx.AddRotations(rightRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(rightRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, rightFace);
                }
            }
        }

        if (shape == ShapeType.Tetrahedral)
        {
            var backFace = BlockFaceEx.Rotate(BlockFace.Back, rotation);
            var rightFace = BlockFaceEx.Rotate(BlockFace.Right, rotation);

            if (m_data.GetFaceDraw(BlockFace.Down))
            {
                var downBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Down));

                if (BlockTypeList.instance.Get(downBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType downShape = GetShapeTypeData(downBlock.data);
                    Rotation downRotation = GetRotationData(downBlock.data);

                    if (downShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(downRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Down);

                    if (downShape == ShapeType.HorizontalHalfCubic && downRotation == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Down);
                }
            }

            if (m_data.GetFaceDraw(backFace))
            {
                var backBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(backFace));

                if (BlockTypeList.instance.Get(backBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType backShape = GetShapeTypeData(backBlock.data);
                    Rotation backRotation = GetRotationData(backBlock.data);

                    if (backShape == ShapeType.HalfCubic && RotationEx.SubRotations(backRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, backFace);

                    if (backShape == ShapeType.Tetrahedral && RotationEx.SubRotations(backRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, backFace);

                    if (backShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(backRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, backFace);
                }
            }

            if (m_data.GetFaceDraw(rightFace))
            {
                var rightBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(rightFace));

                if (BlockTypeList.instance.Get(rightBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType rightShape = GetShapeTypeData(rightBlock.data);
                    Rotation rightRotation = GetRotationData(rightBlock.data);

                    if (rightShape == ShapeType.HalfCubic && rightRotation == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.Tetrahedral && RotationEx.AddRotations(rightRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(rightRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, rightFace);
                }
            }
        }

        if (shape == ShapeType.AntiTetrahedral)
        {
            var backFace = BlockFaceEx.Rotate(BlockFace.Back, rotation);
            var rightFace = BlockFaceEx.Rotate(BlockFace.Right, rotation);

            if (m_data.GetFaceDraw(BlockFace.Up))
            {
                var upBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Up));

                if (BlockTypeList.instance.Get(upBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType upShape = GetShapeTypeData(upBlock.data);
                    Rotation upRotation = GetRotationData(upBlock.data);

                    if (upShape == ShapeType.Tetrahedral && RotationEx.AddRotations(upRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Up);

                    if (upShape == ShapeType.HorizontalHalfCubic && RotationEx.AddRotations(upRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Up);
                }
            }

            if (m_data.GetFaceDraw(backFace))
            {
                var backBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(backFace));

                if (BlockTypeList.instance.Get(backBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType backShape = GetShapeTypeData(backBlock.data);
                    Rotation backRotation = GetRotationData(backBlock.data);

                    if (backShape == ShapeType.HalfCubic && RotationEx.AddRotations(backRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, backFace);

                    if (backShape == ShapeType.Tetrahedral && RotationEx.AddRotations(backRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, backFace);

                    if (backShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(backRotation, Rotation.Rot90) == rotation)
                        m_data.SetFaceDraw(false, backFace);
                }
            }

            if (m_data.GetFaceDraw(rightFace))
            {
                var rightBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(rightFace));

                if (BlockTypeList.instance.Get(rightBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType rightShape = GetShapeTypeData(rightBlock.data);
                    Rotation rightRotation = GetRotationData(rightBlock.data);

                    if (rightShape == ShapeType.HalfCubic && RotationEx.AddRotations(rightRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.Tetrahedral && RotationEx.AddRotations(rightRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, rightFace);

                    if (rightShape == ShapeType.AntiTetrahedral && RotationEx.SubRotations(rightRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, rightFace);
                }
            }
        }

        if (shape == ShapeType.HorizontalHalfCubic)
        {
            if (m_data.GetFaceDraw(BlockFace.Up))
            {
                var upBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Up));

                if (BlockTypeList.instance.Get(upBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType upShape = GetShapeTypeData(upBlock.data);
                    Rotation upRotation = GetRotationData(upBlock.data);

                    if (upShape == ShapeType.HorizontalHalfCubic && upRotation == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Up);

                    if (upShape == ShapeType.Tetrahedral && upRotation == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Up);
                }
            }

            if (m_data.GetFaceDraw(BlockFace.Down))
            {
                var downBlock = neighbors.Get(BlockFaceEx.FaceToDirInt(BlockFace.Down));

                if (BlockTypeList.instance.Get(downBlock.id).type == BlockType.Smoothed)
                {
                    ShapeType downShape = GetShapeTypeData(downBlock.data);
                    Rotation downRotation = GetRotationData(downBlock.data);

                    if (downShape == ShapeType.HorizontalHalfCubic && downRotation == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Down);

                    if (downShape == ShapeType.AntiTetrahedral && RotationEx.AddRotations(downRotation, Rotation.Rot180) == rotation)
                        m_data.SetFaceDraw(false, BlockFace.Down);
                }
            }
        }
    }

    static Matrix<bool> tempBlocks = new Matrix<bool>(3, 3, 3);
    static int nbRot = Enum.GetValues(typeof(Rotation)).Length;

    void GetBlockType(MatrixView<BlockData> neighbors, out ShapeType shape, out Rotation rotation)
    {
        BlockID id = neighbors.GetCenter().id;
        var type = BlockTypeList.instance.Get(id);

        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    var b = neighbors.Get(i, j, k);
                    var testType = BlockTypeList.instance.Get(b.id);

                    tempBlocks.Set(i + 1, j + 1, k + 1, b.id == id || testType.type == type.type || testType.IsFull());
                }

        int nbShape = m_shapes.Count();
        for (int shapeIndex = 0; shapeIndex < nbShape; shapeIndex++)
        {
            var s = m_shapes[shapeIndex];
            int nbBit = s.m_bits.Count;
            for(int rot = 0; rot < nbRot; rot++)
            {
                bool validBlock = true;

                for (int i = 0; i < nbBit; i++)
                {
                    var b = s.m_bits[i];
                    bool block = tempBlocks.Get(b.x[rot] + 1, b.y + 1, b.z[rot] + 1);
                    if(block != b.full)
                    {
                        validBlock = false;
                        break;
                    }
                }

                if (validBlock)
                {
                    shape = s.shape;
                    rotation = (Rotation)rot;
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

        if (IsNoOverrideData(data.data))
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

        shapes.Add(new BlockShape(new short[]
            {2,2,2,2,2,2,2,2,2
            ,2,1,2,1,2,1,2,0,2
            ,2,2,2,2,0,2,2,2,2}
            , ShapeType.HalfCubic));

        shapes.Add(new BlockShape(new short[]
            {2,2,2,2,2,2,2,2,2
            ,2,1,2,0,2,1,2,0,2
            ,2,2,2,2,1,2,2,2,2}
            , ShapeType.HorizontalHalfCubic));

        shapes.Add(new BlockShape(new short[]
            {2,2,2,2,2,2,2,2,2
            ,2,1,2,0,2,1,2,0,2
            ,2,2,2,2,0,2,2,2,2}
            , ShapeType.Tetrahedral));

        shapes.Add(new BlockShape(new short[]
            {2,2,2,2,2,2,2,2,2
            ,1,1,0,1,2,1,1,1,1
            ,2,0,2,2,2,0,2,2,2}
            , ShapeType.AntiTetrahedral));

        shapes.Add(new BlockShape(new short[]
            {2,2,2,2,2,2,2,2,2
            ,2,2,2,2,2,2,2,2,2
            ,2,2,2,2,2,2,2,2,2}
            , ShapeType.Cubic));

        return shapes;
    }
}
