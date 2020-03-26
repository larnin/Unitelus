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


    public override void Render(Vector3 pos, Vector3 scale, BlockNeighbors neighbors, MeshParams<WorldVertexDefinition> meshParams)
    {
        Debug.Assert(neighbors.size == 1);
        var data = meshParams.Allocate(24, 36, m_material);

        WorldVertexDefinition vertex = new WorldVertexDefinition();
        vertex.pos = new Vector3(0, 0, 0);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize] = vertex;
        vertex.pos = new Vector3(0, 0, 1);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 1] = vertex;
        vertex.pos = new Vector3(0, 1, 1);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 2] = vertex;
        vertex.pos = new Vector3(0, 1, 0);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 3] = vertex;

        vertex.pos = new Vector3(0, 1, 0);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 4] = vertex;
        vertex.pos = new Vector3(0, 1, 1);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 5] = vertex;
        vertex.pos = new Vector3(1, 1, 1);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 6] = vertex;
        vertex.pos = new Vector3(1, 1, 0);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 7] = vertex;

        vertex.pos = new Vector3(0, 0, 1);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 8] = vertex;
        vertex.pos = new Vector3(1, 0, 1);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 9] = vertex;
        vertex.pos = new Vector3(1, 1, 1);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 10] = vertex;
        vertex.pos = new Vector3(0, 1, 1);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 11] = vertex;

        vertex.pos = new Vector3(1, 0, 0);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 12] = vertex;
        vertex.pos = new Vector3(1, 0, 1);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 13] = vertex;
        vertex.pos = new Vector3(0, 0, 1);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 14] = vertex;
        vertex.pos = new Vector3(0, 0, 0);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 15] = vertex;

        vertex.pos = new Vector3(1, 1, 0);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 16] = vertex;
        vertex.pos = new Vector3(1, 1, 1);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 17] = vertex;
        vertex.pos = new Vector3(1, 0, 1);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 18] = vertex;
        vertex.pos = new Vector3(1, 0, 0);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 19] = vertex;

        vertex.pos = new Vector3(0, 1, 0);
        vertex.uv = new Vector2(0, 0);
        data.vertices[data.verticesSize + 20] = vertex;
        vertex.pos = new Vector3(1, 1, 0);
        vertex.uv = new Vector2(0, 1);
        data.vertices[data.verticesSize + 21] = vertex;
        vertex.pos = new Vector3(1, 0, 0);
        vertex.uv = new Vector2(1, 1);
        data.vertices[data.verticesSize + 22] = vertex;
        vertex.pos = new Vector3(0, 0, 0);
        vertex.uv = new Vector2(1, 0);
        data.vertices[data.verticesSize + 23] = vertex;

        for(ushort i = 0; i < 6; i++)
        {
            data.indexes[data.indexesSize + i * 6] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 1] = (ushort)(4 * i + 1 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 2] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 3] = (ushort)(4 * i + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 4] = (ushort)(4 * i + 2 + data.verticesSize);
            data.indexes[data.indexesSize + i * 6 + 5] = (ushort)(4 * i + 3 + data.verticesSize);
        }

        MeshEx.Scale(ref data.vertices, data.verticesSize, 24, scale);
        MeshEx.Move(ref data.vertices, data.verticesSize, 24, pos);

        data.verticesSize += 24;
        data.indexesSize += 36;
    }
}
