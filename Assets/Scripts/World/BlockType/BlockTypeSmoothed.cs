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
        Thetrahedral,
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
        //todo
        return false;
    }

    public override bool IsFull()
    {
        return false;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        ShapeType shape = ShapeType.Cubic;
        Rotation rotation = Rotation.Rot0;
        GetBlockType(neighbors, out shape, out rotation);

        if (m_data == null)
            m_data = new BlockRendererData(id, m_material);
            m_data.SetFaceUV(new Rect(0.25f, 0, 0.25f, 1)
                , new Rect(0.5f, 0, 0.25f, 1)
                , new Rect(0.0f, 0, 0.25f, 1));
        m_data.rotation = rotation;

        switch(shape)
        {
            case ShapeType.AntiTetrahedral:
                BlockRenderer.DrawAntiTetrahedral(pos, neighbors, meshParams, m_data);
                break;
            case ShapeType.Cubic:
                BlockRenderer.DrawCubic(pos, neighbors, meshParams, m_data);
                break;
            case ShapeType.HalfCubic:
                BlockRenderer.DrawHalfCubic(pos, neighbors, meshParams, m_data);
                break;
            case ShapeType.HorizontalHalfCubic:
                BlockRenderer.DrawHorizontalHalfCubic(pos, neighbors, meshParams, m_data);
                break;
            case ShapeType.SmallPyramid:
                BlockRenderer.DrawSmallPyramid(pos, neighbors, meshParams, m_data);
                break;
            case ShapeType.Thetrahedral:
                BlockRenderer.DrawThetrahedral(pos, neighbors, meshParams, m_data);
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    public static void GetBlockType(MatrixView<BlockData> neighbors, out ShapeType shape, out Rotation rotation)
    {
        ushort id = neighbors.Get(0, 0, 0).id;

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
        var data = neighbors.Get(0, 0, 0);

        if(IsNoOverrideData(data.data))
            return neighbors.Get(0, 0, 0);

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
            , ShapeType.Thetrahedral));

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
