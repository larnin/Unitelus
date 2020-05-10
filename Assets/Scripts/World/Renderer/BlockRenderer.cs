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
    public Rect[] facesUV;
    public bool allowDrawSelfFaces;

    public BlockRendererData(int _id, Material mat, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        facesUV = new Rect[] { new Rect(0, 0, 1, 1) };
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;
    }

    public BlockRendererData(int _id, Material mat, Rect[] _facesUV, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        facesUV = _facesUV;
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;
    }

    public BlockRendererData(int _id, Material mat, Rect faceUV, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        facesUV = new Rect[1] { faceUV };
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;
    }

    public BlockRendererData(int _id, Material mat, Rect topUV, Rect downUV, Rect sideUV, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        facesUV = new Rect[3] { topUV, downUV, sideUV };
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;
    }

    public BlockRendererData(int _id, Material mat, Rect topUV, Rect downUV, Rect leftUV, Rect rightUV, Rect frontUV, Rect backUV, Rotation rot = Rotation.Rot0, bool _allowDrawSelfFaces = false)
    {
        id = _id;
        material = mat;
        facesUV = new Rect[6] { topUV, downUV, leftUV, rightUV, frontUV, backUV };
        rotation = rot;
        allowDrawSelfFaces = _allowDrawSelfFaces;
    }

    public Rect GetFaceUV(BlockFace face)
    {
        if(facesUV.Length == 0)
            return new Rect(0, 0, 1, 1);
        if (facesUV.Length < 3)
            return facesUV[0];
        if(facesUV.Length < 6)
        {
            switch(face)
            {
                case BlockFace.Up:
                    return facesUV[0];
                case BlockFace.Down:
                    return facesUV[1];
                default:
                    return facesUV[2];
            }
        }

        switch(face)
        {
            case BlockFace.Up:
                return facesUV[0];
            case BlockFace.Down:
                return facesUV[1];
            case BlockFace.Left:
                return facesUV[2];
            case BlockFace.Right:
                return facesUV[3];
            case BlockFace.Front:
                return facesUV[4];
            case BlockFace.Back:
                return facesUV[5];
            default:
                break;
        }

        Debug.Assert(false);
        return new Rect(0, 0, 1, 1);
    }
}

public static class BlockRenderer
{
    public static void DrawCubic(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {
        var data = meshParams.Allocate(24, 36, blockData.material);

        bool left = !BlockTypeList.instance.Get(neighbors.Get(1, 0, 0).id).IsFaceFull(BlockFace.Right);
        bool right = !BlockTypeList.instance.Get(neighbors.Get(-1, 0, 0).id).IsFaceFull(BlockFace.Left);
        bool up = !BlockTypeList.instance.Get(neighbors.Get(0, 1, 0).id).IsFaceFull(BlockFace.Down);
        bool down = !BlockTypeList.instance.Get(neighbors.Get(0, -1, 0).id).IsFaceFull(BlockFace.Up);
        bool front = !BlockTypeList.instance.Get(neighbors.Get(0, 0, 1).id).IsFaceFull(BlockFace.Back);
        bool back = !BlockTypeList.instance.Get(neighbors.Get(0, 0, -1).id).IsFaceFull(BlockFace.Front);

        if(!blockData.allowDrawSelfFaces)
        {
            left &= neighbors.Get(1, 0, 0).id != blockData.id;
            right &= neighbors.Get(-1, 0, 0).id != blockData.id;
            up &= neighbors.Get(0, 1, 0).id != blockData.id;
            down &= neighbors.Get(0, -1, 0).id != blockData.id;
            front &= neighbors.Get(0, 0, 1).id != blockData.id;
            back &= neighbors.Get(0, 0, -1).id != blockData.id;
        }

        int vertexIndex = 0;
        int nb = 0;

        if (right)
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Right, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(-1, 0, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(0, 1, 0, -1);
            }
            vertexIndex += 4;
            nb++;
        }

        if (up)
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
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, 1, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, -1);
            }
            RotateUV(data, data.verticesSize + vertexIndex, 4, (int)blockData.rotation);
            vertexIndex += 4;
            nb++;
        }

        if (front)
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
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, 0, 1);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, 1);
            }
            vertexIndex += 4;
            nb++;
        }

        if (down)
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
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, -1, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, 1);
            }
            RotateUV(data, data.verticesSize + vertexIndex, 4, (int)blockData.rotation);
            vertexIndex += 4;
            nb++;
        }

        if (left)
        {
            var rect = blockData.GetFaceUV(BlockFaceEx.Rotate(BlockFace.Left, blockData.rotation));
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(rect.x, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(rect.x + rect.width, rect.y + rect.height);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(rect.x + rect.width, rect.y);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(rect.x, rect.y);
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(1, 0, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(0, 1, 0, 1);
            }
            vertexIndex += 4;
            nb++;
        }

        if (back)
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
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, 0, -1);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, -1);
            }
            vertexIndex += 4;
            nb++;
        }

        for (int i = 0; i < vertexIndex; i++)
            data.vertices[data.verticesSize + i].color = new Color32(255, 255, 255, 0);

        for (int i = 0; i < nb; i++)
        {
            data.indexes[data.indexesSize + i * 6] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 1] = (ushort)(4 * i + 1 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 2] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 3] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 4] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 5] = (ushort)(4 * i + 3 + data.verticesSize);
        }

        data.verticesSize += vertexIndex;
        data.indexesSize += nb * 6;
    }

    public static void DrawAntiTetrahedral(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    public static void DrawHalfCubic(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    public static void DrawHorizontalHalfCubic(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    public static void DrawThetrahedral(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
    {

    }

    public static void DrawSmallPyramid(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams, BlockRendererData blockData)
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
}
