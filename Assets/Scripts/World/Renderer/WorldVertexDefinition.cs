using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public struct WorldVertexDefinition
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector4 tangent;
    public Color32 color;
    public Vector2 uv;
}

public static class MeshEx
{
    static public void SetWorldMeshParams(Mesh mesh, int vertexNb, int indexNb)
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        mesh.SetVertexBufferParams(vertexNb, layout);

        mesh.SetIndexBufferParams(indexNb, IndexFormat.UInt16);
    }

    public static void Scale(ref NativeArray<WorldVertexDefinition> vertices, int startIndex, int size, Vector3 scale)
    {
        for (int i = 0; i < size; i++)
        {
            var vertex = vertices[startIndex + i];
            vertex.pos.x *= scale.x;
            vertex.pos.y *= scale.y;
            vertex.pos.z *= scale.z;
            vertices[startIndex + i] = vertex;
        }
    }

    public static void Move(ref NativeArray<WorldVertexDefinition> vertices, int startIndex, int size, Vector3 dir)
    {
        for (int i = 0; i < size; i++)
        {
            var vertex = vertices[startIndex + i];
            vertex.pos += dir;
            vertices[startIndex + i] = vertex;
        }
    }
}

