using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockRendererData
{
    public int id;
    public Material material;
    public Rotation rotation;
    public bool allowDrawSelfFaces;
    Rect[] facesUV;
    bool[] facesDraw;

    public BlockRendererData(int _id, Material mat, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        var rect = new Rect(0, 0, 1, 1);
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;

        facesUV = new Rect[Enum.GetValues(typeof(BlockFace)).Length];
        for (int i = 0; i < facesUV.Length; i++)
            facesUV[i] = rect;

        facesDraw = new bool[Enum.GetValues(typeof(BlockFace)).Length];
        for (int i = 0; i < facesDraw.Length; i++)
            facesDraw[i] = true;
    }

    public void SetFaceUV(Rect[] _facesUV)
    {
        for (int i = 0; i < facesUV.Length || i < _facesUV.Length; i++)
            facesUV[i] = _facesUV[i];
    }

    public void SetFaceUV(Rect faceUV)
    {
        for (int i = 0; i < facesUV.Length; i++)
            facesUV[i] = faceUV;
    }

    public void SetFaceUV(Rect topUV, Rect downUV, Rect sideUV)
    {
        facesUV[(int)BlockFace.Left] = sideUV;
        facesUV[(int)BlockFace.Right] = sideUV;
        facesUV[(int)BlockFace.Up] = topUV;
        facesUV[(int)BlockFace.Down] = downUV;
        facesUV[(int)BlockFace.Front] = sideUV;
        facesUV[(int)BlockFace.Back] = sideUV;
    }

    public void SetFaceUV(Rect topUV, Rect downUV, Rect leftUV, Rect rightUV, Rect frontUV, Rect backUV)
    {
        facesUV[(int)BlockFace.Left] = leftUV;
        facesUV[(int)BlockFace.Right] = rightUV;
        facesUV[(int)BlockFace.Up] = topUV;
        facesUV[(int)BlockFace.Down] = downUV;
        facesUV[(int)BlockFace.Front] = frontUV;
        facesUV[(int)BlockFace.Back] = backUV;
    }

    public void SetFaceUV(Rect faceUV, BlockFace face)
    {
        facesUV[(int)face] = faceUV;
    }

    public Rect GetFaceUV(BlockFace face)
    {
        return facesUV[(int)face];
    }

    public void SetFaceDraw(bool[] _draw)
    {
        for (int i = 0; i < facesDraw.Length || i < _draw.Length; i++)
            facesDraw[i] = _draw[i];
    }

    public void SetFaceDraw(bool faceDraw)
    {
        for (int i = 0; i < facesDraw.Length; i++)
            facesDraw[i] = faceDraw;
    }

    public void SetFaceDraw(bool topDraw, bool downDraw, bool sideDraw)
    {
        facesDraw[(int)BlockFace.Left] = sideDraw;
        facesDraw[(int)BlockFace.Right] = sideDraw;
        facesDraw[(int)BlockFace.Up] = topDraw;
        facesDraw[(int)BlockFace.Down] = downDraw;
        facesDraw[(int)BlockFace.Front] = sideDraw;
        facesDraw[(int)BlockFace.Back] = sideDraw;
    }

    public void SetFaceDraw(bool topDraw, bool downDraw, bool leftDraw, bool rightDraw, bool frontDraw, bool backDraw)
    {
        facesDraw[(int)BlockFace.Left] = leftDraw;
        facesDraw[(int)BlockFace.Right] = rightDraw;
        facesDraw[(int)BlockFace.Up] = topDraw;
        facesDraw[(int)BlockFace.Down] = downDraw;
        facesDraw[(int)BlockFace.Front] = frontDraw;
        facesDraw[(int)BlockFace.Back] = backDraw;
    }

    public void SetFaceDraw(bool faceDraw, BlockFace face)
    {
        facesDraw[(int)face] = faceDraw;
    }

    public bool GetFaceDraw(BlockFace face)
    {
        return facesDraw[(int)face];
    }
}
