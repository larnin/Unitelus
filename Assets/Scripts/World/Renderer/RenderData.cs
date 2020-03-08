using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RendererData
{
    public Material material;
    public int[] triangles;
    public Vector3[] vertices;
    public Vector2[] UVs;
    public Vector3[] normals;

    public void Scale(float scale)
    {
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] *= scale;
    }

    public void Scale(Vector3 scale)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= scale.x;
            vertices[i].y *= scale.y;
            vertices[i].z *= scale.z;
        }
    }

    public void Move(Vector3 dir)
    {
        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += dir;
        }
    }

    public void Merge(RendererData data)
    {
        Debug.Assert(material == data.material);

        if(data.triangles.Length > 0)
        {
            int[] newTriangles = new int[triangles.Length + data.triangles.Length];

            triangles.CopyTo(newTriangles, 0);

            for (int i = 0; i < data.triangles.Length; i++)
                newTriangles[i + triangles.Length] = data.triangles[i] + vertices.Length;

            triangles = newTriangles;
        }

        if(data.vertices.Length > 0)
        {
            Vector3[] newVertices = new Vector3[vertices.Length + data.vertices.Length];

            vertices.CopyTo(newVertices, 0);
            data.vertices.CopyTo(newVertices, vertices.Length);

            vertices = newVertices;
        }

        if(data.UVs.Length > 0)
        {
            Vector2[] newUVs = new Vector2[UVs.Length + data.UVs.Length];

            UVs.CopyTo(newUVs, 0);
            data.UVs.CopyTo(newUVs, UVs.Length);

            UVs = newUVs;
        }

        if(data.normals.Length > 0)
        {
            Vector3[] newNormals = new Vector3[normals.Length + data.normals.Length];

            normals.CopyTo(newNormals, 0);
            data.normals.CopyTo(newNormals, normals.Length);

            normals = newNormals;
        }
    }

    public bool Validate()
    {
        if (material == null)
            return false;

        if (vertices.Length == 0)
            return false;

        if (UVs.Length != vertices.Length)
            return false;

        if (normals.Length != 0 && normals.Length != vertices.Length)
            return false;

        if (triangles.Length % 3 != 0)
            return false;

        for (int i = 0; i < triangles.Length; i++)
            if (triangles[i] < 0 || triangles[i] >= vertices.Length)
                return false;

        return true;
    }
}
