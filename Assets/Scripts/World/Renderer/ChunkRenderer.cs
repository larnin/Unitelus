using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    Chunk m_chunk = null;
    float m_scaleX = 1;
    float m_scaleY = 1;
    float m_scaleZ = 1;
    int m_x = 0;
    int m_y = 0;
    float m_updateTime = -1;

    MeshRenderer m_meshRenderer;
    MeshFilter m_meshFilter;
    MeshCollider m_collider;

    List<LayerRendererPassBase> m_pass = new List<LayerRendererPassBase>();

    private void Start()
    {
        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_collider = gameObject.AddComponent<MeshCollider>();

        if (m_meshFilter.mesh == null)
            m_meshFilter.mesh = new Mesh();

        m_pass.Add(new LayerRenderPassBlocks());
    }

    public void Update()
    {
        if(m_chunk.updateTime > m_updateTime)
        {
            List<RendererData> m_renders = new List<RendererData>();

            foreach(var pass in m_pass)
            {
                var data = pass.Render(m_chunk, m_x, m_y, m_scaleX, m_scaleY, m_scaleZ);

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

            m_updateTime = m_chunk.updateTime;

            if (m_renders.Count <= 0)
                return;

            RendererData bigData = m_renders[0];

            for (int i = 1; i < m_renders.Count; i++)
                bigData.Merge(m_renders[i]);

            mesh.subMeshCount = m_renders.Count();

            mesh.vertices = bigData.vertices;
            mesh.uv = bigData.UVs;
            //mesh.normals = bigData.normals;
            
            Material[] materials = new Material[m_renders.Count];
            int triangleIndex = 0;
            for(int i = 0; i < m_renders.Count; i++)
            {
                materials[i] = m_renders[i].material;
                int[] triangles = new int[m_renders[i].triangles.Length];
                Array.Copy(bigData.triangles, triangleIndex, triangles, 0, m_renders[i].triangles.Length);
                mesh.SetTriangles(triangles, i);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

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

    public void SetPosition(int x, int y)
    {
        m_x = x;
        m_y = y;
    }
}
