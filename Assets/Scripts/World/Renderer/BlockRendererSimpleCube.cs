using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockRendererSimpleCube : BlockRendererBase
{
    [SerializeField]
    public Material m_material;

    public BlockRendererSimpleCube(int id) : base(id)
    {
    }


    public override void Render(Vector3 pos, BlockNeighbors neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        Debug.Assert(neighbors.size == 1);
        var data = meshParams.Allocate(24, 36, m_material);

        data.vertices[data.verticesSize].pos = new Vector3(0, 0, 0) + pos;
        data.vertices[data.verticesSize].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 1].pos = new Vector3(0, 0, 1) + pos;
        data.vertices[data.verticesSize + 1].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 2].pos = new Vector3(0, 1, 1) + pos;
        data.vertices[data.verticesSize + 2].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 3].pos = new Vector3(0, 1, 0) + pos;
        data.vertices[data.verticesSize + 3].uv = new Vector2(1, 0);

        data.vertices[data.verticesSize + 4].pos = new Vector3(0, 1, 0) + pos;
        data.vertices[data.verticesSize + 4].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 5].pos = new Vector3(0, 1, 1) + pos;
        data.vertices[data.verticesSize + 5].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 6].pos = new Vector3(1, 1, 1) + pos;
        data.vertices[data.verticesSize + 6].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 7].pos = new Vector3(1, 1, 0) + pos;
        data.vertices[data.verticesSize + 7].uv = new Vector2(1, 0);

        data.vertices[data.verticesSize + 8].pos = new Vector3(0, 0, 1) + pos;
        data.vertices[data.verticesSize + 8].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 9].pos = new Vector3(1, 0, 1) + pos;
        data.vertices[data.verticesSize + 9].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 10].pos = new Vector3(1, 1, 1) + pos;
        data.vertices[data.verticesSize + 10].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 11].pos = new Vector3(0, 1, 1) + pos;
        data.vertices[data.verticesSize + 11].uv = new Vector2(1, 0);

        data.vertices[data.verticesSize + 12].pos = new Vector3(1, 0, 0) + pos;
        data.vertices[data.verticesSize + 12].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 13].pos = new Vector3(1, 0, 1) + pos;
        data.vertices[data.verticesSize + 13].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 14].pos = new Vector3(0, 0, 1) + pos;
        data.vertices[data.verticesSize + 14].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 15].pos = new Vector3(0, 0, 0) + pos;
        data.vertices[data.verticesSize + 15].uv = new Vector2(1, 0);

        data.vertices[data.verticesSize + 16].pos = new Vector3(1, 1, 0) + pos;
        data.vertices[data.verticesSize + 16].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 17].pos = new Vector3(1, 1, 1) + pos;
        data.vertices[data.verticesSize + 17].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 18].pos = new Vector3(1, 0, 1) + pos;
        data.vertices[data.verticesSize + 18].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 19].pos = new Vector3(1, 0, 0) + pos;
        data.vertices[data.verticesSize + 19].uv = new Vector2(1, 0);

        data.vertices[data.verticesSize + 20].pos = new Vector3(0, 1, 0) + pos;
        data.vertices[data.verticesSize + 20].uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 21].pos = new Vector3(1, 1, 0) + pos;
        data.vertices[data.verticesSize + 21].uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 22].pos = new Vector3(1, 0, 0) + pos;
        data.vertices[data.verticesSize + 22].uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 23].pos = new Vector3(0, 0, 0) + pos;
        data.vertices[data.verticesSize + 23].uv = new Vector2(1, 0);

        for(int i = 0; i < 4; i++)
        {
            data.vertices[data.verticesSize + i].normal = new Vector3(-1, 0, 0);
            data.vertices[data.verticesSize + i + 4].normal = new Vector3(0, 1, 0);
            data.vertices[data.verticesSize + i + 8].normal = new Vector3(0, 0, 1);
            data.vertices[data.verticesSize + i + 12].normal = new Vector3(0, -1, 0);
            data.vertices[data.verticesSize + i + 16].normal = new Vector3(1, 0, 0);
            data.vertices[data.verticesSize + i + 20].normal = new Vector3(0, 0, -1);

            data.vertices[data.verticesSize + i].tangent = new Vector4(0, 1, 0, -1);
            data.vertices[data.verticesSize + i + 4].tangent = new Vector4(1, 0, 0, -1);
            data.vertices[data.verticesSize + i + 8].tangent = new Vector4(1, 0, 0, 1);
            data.vertices[data.verticesSize + i + 12].tangent = new Vector4(1, 0, 0, -1);
            data.vertices[data.verticesSize + i + 16].tangent = new Vector4(0, 1, 0, 1);
            data.vertices[data.verticesSize + i + 20].tangent = new Vector4(0, 1, 0, -1);
        }

        for (int i = 0; i < 24; i++)
            data.vertices[data.verticesSize + i].color = new Color32(255, 255, 255, 0);

        for(int i = 0; i < 6; i++)
        {
            data.indexes[data.indexesSize + i * 6] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 1] = (ushort)(4 * i + 1 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 2] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 3] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 4] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 5] = (ushort)(4 * i + 3 + data.verticesSize);
        }
        
        data.verticesSize += 24;
        data.indexesSize += 36;
    }
}
