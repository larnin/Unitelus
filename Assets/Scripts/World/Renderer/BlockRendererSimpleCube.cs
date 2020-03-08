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

    public override RendererData Render(Vector3 pos, Vector3 scale, BlockNeighbors neighbors)
    {
        Debug.Assert(neighbors.size == 1);

        RendererData data = new RendererData();

        data.material = m_material;

        //to not have the same uv on each face
        data.vertices = new Vector3[24];
        data.vertices[0]  = new Vector3(0, 0, 0);
        data.vertices[1]  = new Vector3(0, 0, 1);
        data.vertices[2]  = new Vector3(0, 1, 1);
        data.vertices[3]  = new Vector3(0, 1, 0);
                         
        data.vertices[4]  = new Vector3(0, 1, 0);
        data.vertices[5]  = new Vector3(0, 1, 1);
        data.vertices[6]  = new Vector3(1, 1, 1);
        data.vertices[7]  = new Vector3(1, 1, 0);
                         
        data.vertices[8]  = new Vector3(0, 0, 1);
        data.vertices[9]  = new Vector3(1, 0, 1);
        data.vertices[10] = new Vector3(1, 1, 1);
        data.vertices[11] = new Vector3(0, 1, 1);

        data.vertices[12] = new Vector3(1, 0, 0);
        data.vertices[13] = new Vector3(1, 0, 1);
        data.vertices[14] = new Vector3(0, 0, 1);
        data.vertices[15] = new Vector3(0, 0, 0);

        data.vertices[16] = new Vector3(1, 1, 0);
        data.vertices[17] = new Vector3(1, 1, 1);
        data.vertices[18] = new Vector3(1, 0, 1);
        data.vertices[19] = new Vector3(1, 0, 0);

        data.vertices[20] = new Vector3(0, 1, 0);
        data.vertices[21] = new Vector3(1, 1, 0);
        data.vertices[22] = new Vector3(1, 0, 0);
        data.vertices[23] = new Vector3(0, 0, 0);

        data.triangles = new int[36];
        for(int i = 0; i < 6; i++)
        {
            data.triangles[i * 6] = 4 * i;
            data.triangles[i * 6 + 1] = 4 * i + 1;
            data.triangles[i * 6 + 2] = 4 * i + 2;
            data.triangles[i * 6 + 3] = 4 * i;
            data.triangles[i * 6 + 4] = 4 * i + 2;
            data.triangles[i * 6 + 5] = 4 * i + 3;
        }

        data.UVs = new Vector2[24];
        for(int i = 0; i < 6; i++)
        {
            data.UVs[i * 4] = new Vector2(0, 0);
            data.UVs[i * 4 + 1] = new Vector2(0, 1);
            data.UVs[i * 4 + 2] = new Vector2(1, 1);
            data.UVs[i * 4 + 3] = new Vector2(1, 0);
        }

        Vector3[] normals = new Vector3[6];
        normals[0] = new Vector3(1, 0, 0);
        normals[1] = new Vector3(0, -1, 0);
        normals[2] = new Vector3(0, 0, -1);
        normals[3] = new Vector3(0, 1, 0);
        normals[4] = new Vector3(-1, 0, 0);
        normals[5] = new Vector3(0, 0, 1);

        data.normals = new Vector3[24];
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 4; j++)
                data.normals[i * 4 + j] = data.normals[i];

        data.Scale(scale);
        data.Move(pos);

        return data;
    }
}
