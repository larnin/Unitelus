using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    Chunk m_chunk;
    float m_scaleX;
    float m_scaleY;
    float m_scaleZ;

    MeshRenderer m_meshRenderer;
    MeshFilter m_meshFilter;
    MeshCollider m_collider;

    List<ChunkRendererPassBase> m_pass = new List<ChunkRendererPassBase>();

    private void Start()
    {
        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_collider = gameObject.AddComponent<MeshCollider>();

        if (m_meshFilter.mesh == null)
            m_meshFilter.mesh = new Mesh();

        m_pass.Add(new ChunkRenderPassBlocks());
    }

    public void Update()
    {
        if(m_chunk.updated)
        {
            List<RendererData> m_renders = new List<RendererData>();

            foreach(var pass in m_pass)
            {
                var data = pass.Render(m_chunk, m_scaleX, m_scaleY, m_scaleZ);

                foreach(var d in data)
                {
                    var render = m_renders.Find(x => { return x.material == d.material; });
                    if (render == null)
                        m_renders.Add(d);
                    else render.Merge(d);
                }
            }

            var mesh = m_meshFilter.mesh;
            mesh.Clear();

            m_chunk.Rendered();

            if (m_renders.Count <= 0)
                return;

            RendererData bigData = m_renders[0];

            for (int i = 1; i < m_renders.Count; i++)
                bigData.Merge(m_renders[i]);

            mesh.subMeshCount = m_renders.Count();

            mesh.vertices = bigData.vertices;
            mesh.uv = bigData.UVs;
            mesh.normals = bigData.normals;

            Material[] materials = new Material[m_renders.Count];
            int triangleIndex = 0;
            for(int i = 0; i < m_renders.Count; i++)
            {
                materials[i] = m_renders[i].material;
                int[] triangles = new int[m_renders[i].triangles.Length];
                Array.Copy(bigData.triangles, triangleIndex, triangles, 0, m_renders[i].triangles.Length);
                mesh.SetTriangles(triangles, i);
            }

            m_meshRenderer.materials = materials;
        }
    }

    public void SetChunk(Chunk c)
    {
        m_chunk = c;
    }

    public void SetScale(float x, float y, float z)
    {
        m_scaleX = x;
        m_scaleY = y;
        m_scaleZ = z;
    }
}
