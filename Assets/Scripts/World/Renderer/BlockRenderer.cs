using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BlockRenderer
{
    public static void DrawCubic(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {
        var data = meshParams.Allocate(24, 36, blockData.material);

        int vertexIndex = 0;
        int nb = 0;

        if (blockData.GetFaceDraw(BlockFace.Left))
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Left, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 4;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFace.Up))
        {
            var rect = blockData.GetFaceUV(BlockFace.Up);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y);
            RotateUV(data, data.verticesSize + vertexIndex, 4, (int)blockData.rotation);
            vertexIndex += 4;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFace.Front))
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Front, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 4;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFace.Down))
        {
            var rect = blockData.GetFaceUV(BlockFace.Down);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            RotateUV(data, data.verticesSize + vertexIndex, 4, (int)blockData.rotation);
            vertexIndex += 4;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFace.Right))
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Right, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 4;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFace.Back))
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Back, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 4;
            nb++;
        }

        SetColor(data, data.verticesSize, vertexIndex, new Color32(255, 255, 255, 0));

        SetQuadsIndexs(data, data.verticesSize, data.indexesSize, nb);

        BakeNormals(data, data.indexesSize, nb * 2);
        BakeTangents(data, data.indexesSize, nb * 2);

        data.verticesSize += vertexIndex;
        data.indexesSize += nb * 6;
    }

    public static void DrawAntiTetrahedral(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {
        //3 square and 4 triangles
        // each square have 4 vertex and 6 index
        // each triangle have 3 vertex and 3 index
        var data = meshParams.Allocate(24, 30, blockData.material);

        var rot = blockData.rotation;

        int vertexIndex = 0;
        int nbSquare = 0;
        int nbTriangle = 0;


        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Left, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Left);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 4;
            nbSquare++;
        }

        

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Front, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Front);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 4;
            nbSquare++;
        }

        if (blockData.GetFaceDraw(BlockFace.Down))
        {
            var rect = blockData.GetFaceUV(BlockFace.Down);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 4;
            nbSquare++;
        }

        if (blockData.GetFaceDraw(BlockFace.Up))
        {
            var rect = blockData.GetFaceUV(BlockFace.Up);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 3;
            nbTriangle++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Right, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Right);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nbTriangle++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Back, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Back);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nbTriangle++;
        }

        //triangle drawn everytime
        {
            var rect = blockData.GetFaceUV(BlockFace.Up);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nbTriangle++;
        }


        SetColor(data, data.verticesSize, vertexIndex, new Color32(255, 255, 255, 0));

        SetQuadsIndexs(data, data.verticesSize, data.indexesSize, nbSquare);
        SetTrianglesIndexs(data, data.verticesSize + 4 * nbSquare, data.indexesSize + 6 * nbSquare, nbTriangle);

        RotatePos(data, data.verticesSize, vertexIndex, pos, rot);

        BakeNormals(data, data.indexesSize, nbSquare * 2 + nbTriangle);
        BakeTangents(data, data.indexesSize, nbSquare * 2 + nbTriangle);

        data.verticesSize += vertexIndex;
        data.indexesSize += nbSquare * 6 + nbTriangle * 3;

    }

    public static void DrawHalfCubic(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {
        //3 square and 2 triangles
        // each square have 4 vertex and 6 index
        // each triangle have 3 vertex and 3 index
        var data = meshParams.Allocate(18, 24, blockData.material);

        var rot = blockData.rotation;

        int vertexIndex = 0;
        int nbSquare = 0;
        int nbTriangle = 0;

        if (blockData.GetFaceDraw(BlockFace.Down))
        {
            var rect = blockData.GetFaceUV(BlockFace.Down);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 4;
            nbSquare++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Back, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Back);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 4;
            nbSquare++;
        }

        //top face is drawn in all cases
        {
            var rect = blockData.GetFaceUV(BlockFace.Up);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 4;
            nbSquare++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Right, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Right);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nbTriangle++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Left, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Left);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            vertexIndex += 3;
            nbTriangle++;
        }

        SetColor(data, data.verticesSize, vertexIndex, new Color32(255, 255, 255, 0));

        SetQuadsIndexs(data, data.verticesSize, data.indexesSize, nbSquare);
        SetTrianglesIndexs(data, data.verticesSize + 4 * nbSquare, data.indexesSize + 6 * nbSquare, nbTriangle);

        RotatePos(data, data.verticesSize, vertexIndex, pos, rot);

        BakeNormals(data, data.indexesSize, nbSquare * 2 + nbTriangle);
        BakeTangents(data, data.indexesSize, nbSquare * 2 + nbTriangle);

        data.verticesSize += vertexIndex;
        data.indexesSize += nbSquare * 6 + nbTriangle * 3;
    }

    public static void DrawHorizontalHalfCubic(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    public static void DrawThetrahedral(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {
        //4 triangles
        // each triangle have 3 vertex and 3 index
        var data = meshParams.Allocate(12, 12, blockData.material);

        var rot = blockData.rotation;

        int vertexIndex = 0;
        int nb = 0;

        if (blockData.GetFaceDraw(BlockFace.Down))
        {
            var rect = blockData.GetFaceUV(BlockFace.Down);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y);
            vertexIndex += 3;
            nb++;
        }

        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Back, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Back);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nb++;
        }
        
        if (blockData.GetFaceDraw(BlockFaceEx.Rotate(BlockFace.Right, rot)))
        {
            var rect = blockData.GetFaceUV(BlockFace.Right);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nb++;
        }
        
        //top face is drawn in all cases
        {
            var rect = blockData.GetFaceUV(BlockFace.Up);
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            vertexIndex += 3;
            nb++;
        }

        SetColor(data, data.verticesSize, vertexIndex, new Color32(255, 255, 255, 0));
        
        SetTrianglesIndexs(data, data.verticesSize, data.indexesSize, nb);

        RotatePos(data, data.verticesSize, vertexIndex, pos, rot);

        BakeNormals(data, data.indexesSize, nb);
        BakeTangents(data, data.indexesSize, nb);

        data.verticesSize += vertexIndex;
        data.indexesSize += nb * 3;
    }

    public static void DrawSmallPyramid(Vector3 pos, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    static void RotateUV(MeshParamData<WorldVertexDefinition> data, int index, int size, int nb)
    {
        while (nb > 0)
        {
            Vector2 uv = data.vertices[index].uv;
            for (int i = 0; i < size - 1; i++)
                data.vertices[index + i].uv = data.vertices[index + i + 1].uv;
            data.vertices[index + size - 1].uv = uv;

            nb--;
        }
    }

    static void RotatePos(MeshParamData<WorldVertexDefinition> data, int index, int size, Vector3 origin, Rotation rot)
    {
        for(int i = 0; i < size; i++)
        {
            var pos = data.vertices[index + i].pos - origin;
            var rotatedPos = RotationEx.RotateOffset(new Vector2(pos.x - 0.5f, pos.z - 0.5f), rot);
            pos.x = rotatedPos.x + 0.5f;
            pos.z = rotatedPos.y + 0.5f;
            data.vertices[index + i].pos = pos + origin;
        }
    }

    static void SetColor(MeshParamData<WorldVertexDefinition> data, int index, int size, Color32 color)
    {
        for (int i = 0; i < size; i++)
            data.vertices[index + i].color = color;
    }

    static void SetQuadsIndexs(MeshParamData<WorldVertexDefinition> data, int vertexIndex, int indexIndex, int quadNb)
    {
        for(int i = 0; i < quadNb; i++)
        {
            data.indexes[indexIndex + i * 6] = (ushort)(4 * i + vertexIndex);
            data.indexes[indexIndex + i * 6 + 1] = (ushort)(4 * i + 1 + vertexIndex);
            data.indexes[indexIndex + i * 6 + 2] = (ushort)(4 * i + 2 + vertexIndex);
            data.indexes[indexIndex + i * 6 + 3] = (ushort)(4 * i + vertexIndex);
            data.indexes[indexIndex + i * 6 + 4] = (ushort)(4 * i + 2 + vertexIndex);
            data.indexes[indexIndex + i * 6 + 5] = (ushort)(4 * i + 3 + vertexIndex);
        }
    }

    static void SetTrianglesIndexs(MeshParamData<WorldVertexDefinition> data, int vertexIndex, int indexIndex, int triangleNb)
    {
        for (int i = 0; i < triangleNb; i++)
        {
            data.indexes[indexIndex + i * 3] = (ushort)(3 * i + vertexIndex);
            data.indexes[indexIndex + i * 3 + 1] = (ushort)(3 * i + 1 + vertexIndex);
            data.indexes[indexIndex + i * 3 + 2] = (ushort)(3 * i + 2 + vertexIndex);
        }
    }

    static void BakeNormals(MeshParamData<WorldVertexDefinition> data, int index, int triangleNb)
    {
        //https://math.stackexchange.com/questions/305642/how-to-find-surface-normal-of-a-triangle

        for (int i = 0; i < triangleNb; i++)
        {
            int i1 = data.indexes[index + i * 3];
            int i2 = data.indexes[index + i * 3 + 1];
            int i3 = data.indexes[index + i * 3 + 2];

            var p1 = data.vertices[i1].pos;
            var p2 = data.vertices[i2].pos;
            var p3 = data.vertices[i3].pos;

            var v = p2 - p1;
            var w = p3 - p1;

            var n = new Vector3(v.y * w.z - v.z * w.y, v.z * w.x - v.x * w.z, v.x * w.y - v.y * w.x);

            data.vertices[i1].normal = n;
            data.vertices[i2].normal = n;
            data.vertices[i3].normal = n;
        }
    }

    static void BakeTangents(MeshParamData<WorldVertexDefinition> data, int index, int triangleNb)
    {
        //https://forum.unity.com/threads/how-to-calculate-mesh-tangents.38984/#post-285069
        //with a small simplification : 
        // Each vertex are linked to only one triangle. If one vertex is on multiple triangle, the combined surface is flat, we don't need to combine tangent

        for (int i = 0; i < triangleNb; i++)
        {
            int i1 = data.indexes[index + i * 3];
            int i2 = data.indexes[index + i * 3 + 1];
            int i3 = data.indexes[index + i * 3 + 2];

            var v1 = data.vertices[i1];
            var v2 = data.vertices[i2];
            var v3 = data.vertices[i3];

            float x1 = v2.pos.x - v1.pos.x;
            float x2 = v3.pos.x - v1.pos.x;
            float y1 = v2.pos.y - v1.pos.y;
            float y2 = v3.pos.y - v1.pos.y;
            float z1 = v2.pos.z - v1.pos.z;
            float z2 = v3.pos.z - v1.pos.z;
            float s1 = v2.uv.x - v1.uv.x;
            float s2 = v3.uv.x - v1.uv.x;
            float t1 = v2.uv.y - v1.uv.y;
            float t2 = v3.uv.y - v1.uv.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            var tmp = (sdir - v1.normal * Vector3.Dot(v1.normal, sdir)).normalized;
            var w = (Vector3.Dot(Vector3.Cross(v1.normal, sdir), tdir) < 0.0f) ? -1.0f : 1.0f;

            var tan = new Vector4(tmp.x, tmp.y, tmp.z, w);

            data.vertices[i1].tangent = tan;
            data.vertices[i2].tangent = tan;
            data.vertices[i3].tangent = tan;
        }
    }
}
