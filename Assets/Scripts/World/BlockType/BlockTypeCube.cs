using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockTypeCube : BlockTypeBase
{
    [SerializeField]
    public Material m_material;

    public BlockTypeCube(int id) : base(id)
    {
    }

    public override bool IsFaceFull(BlockFace face)
    {
        //full all the times
        return true;
    }

    public override void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        var data = meshParams.Allocate(24, 36, m_material);

        bool left = !BlockTypeList.instance.Get(neighbors.Get(1, 0, 0).id).IsFaceFull(BlockFace.Right);
        bool right = !BlockTypeList.instance.Get(neighbors.Get(-1, 0, 0).id).IsFaceFull(BlockFace.Left);
        bool up = !BlockTypeList.instance.Get(neighbors.Get(0, 1, 0).id).IsFaceFull(BlockFace.Down);
        bool down = !BlockTypeList.instance.Get(neighbors.Get(0, -1, 0).id).IsFaceFull(BlockFace.Up);
        bool front = !BlockTypeList.instance.Get(neighbors.Get(0, 0, 1).id).IsFaceFull(BlockFace.Back);
        bool back = !BlockTypeList.instance.Get(neighbors.Get(0, 0, -1).id).IsFaceFull(BlockFace.Front);

        int vertexIndex = 0;
        int nb = 0;

        if (right)
        {
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
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
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, 1, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, -1);
            }
            vertexIndex += 4;
            nb++;
        }

        if (front)
        {
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
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
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex ].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(0, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
            for (int i = 0; i < 4; i++)
            {
                data.vertices[data.verticesSize + vertexIndex + i].normal = new Vector3(0, -1, 0);
                data.vertices[data.verticesSize + vertexIndex + i].tangent = new Vector4(1, 0, 0, 1);
            }
            vertexIndex += 4;
            nb++;
        }

        if (left)
        {
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 1) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
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
            data.vertices[data.verticesSize + vertexIndex].pos = new Vector3(0, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex].uv = new Vector2(0, 0);
            data.vertices[data.verticesSize + vertexIndex + 1].pos = new Vector3(1, 1, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 1].uv = new Vector2(0, 1);
            data.vertices[data.verticesSize + vertexIndex + 2].pos = new Vector3(1, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 2].uv = new Vector2(1, 1);
            data.vertices[data.verticesSize + vertexIndex + 3].pos = new Vector3(0, 0, 0) + pos;
            data.vertices[data.verticesSize + vertexIndex + 3].uv = new Vector2(1, 0);
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

        for(int i = 0; i < nb; i++)
        {
            data.indexes[data.indexesSize + i * 6] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 1] = (ushort)(4 * i + 1 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 2] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 3] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 4] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 5] = (ushort)(4 * i + 3 + data.verticesSize);
        }
        
        data.verticesSize += vertexIndex;
        data.indexesSize += nb*6;
    }
}
