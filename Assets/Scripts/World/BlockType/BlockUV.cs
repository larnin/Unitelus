using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BlockUVType
{
    SimpleFace,
    UpDownSide,
    FullUnfold,
}

[Serializable]
public class BlockUV
{
    public enum Face
    {
        Up,
        Down,
        Front,
        Back,
        Left,
        Right,
    }
    public BlockUVType uvType;
    public Rect[] uvs;

    public void FillBlockDataUV(BlockRendererData data)
    {
        if(uvs == null)
        {
            FillDefaultUV(data);
            return;
        }

        switch(uvType)
        {
            case BlockUVType.FullUnfold:
                if (uvs.Length < 6)
                    FillDefaultUV(data);
                else data.SetFaceUV(uvs);
                break;
            case BlockUVType.SimpleFace:
                if (uvs.Length < 1)
                    FillDefaultUV(data);
                else data.SetFaceUV(uvs[0]);
                break;
            case BlockUVType.UpDownSide:
                if (uvs.Length < 3)
                    FillDefaultUV(data);
                else data.SetFaceUV(uvs[(int)Face.Up], uvs[(int)Face.Down], uvs[(int)Face.Front]);
                break;
            default:
                FillDefaultUV(data);
                break;
        }
    }

    void FillDefaultUV(BlockRendererData data)
    {
        Debug.LogWarning("Invalid block data UV, use default UV");

        data.SetFaceUV(new Rect(0, 0, 1, 1));
    }
}