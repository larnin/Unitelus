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
    int m_z = 0;
    float m_fUpdateTime = -1;

    //used to buffer the render of a mesh
    MeshParams<WorldVertexDefinition> m_meshParams = new MeshParams<WorldVertexDefinition>();

    List<LayerRendererPassBase> m_pass = new List<LayerRendererPassBase>();

    class LayerObject
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
    }

    class LayerRender
    {
        public List<LayerObject> objects = new List<LayerObject>();
        public int layerIndex = 0;
    }

    List<LayerRender> m_layers = new List<LayerRender>();

    private void Start()
    {
        m_pass.Add(new LayerRenderPassBlocks());
    }

    public void Update()
    {
        if (m_chunk == null)
            return;

        for (int i = 0; i < m_layers.Count; i++)
        {
            if (!m_chunk.HaveLayer(m_layers[i].layerIndex))
            {
                DestroyLayer(m_layers[i]);
                i--;
            }
        }

        var updatedLayers = m_chunk.GetLayersUptatedAfter(m_fUpdateTime);

        foreach(var index in updatedLayers)
        {
            var layer = m_layers.Find(x => { return x.layerIndex == index; });
            if(layer == null)
            {
                layer = new LayerRender();
                layer.layerIndex = index;
                m_layers.Add(layer);
            }
            UpdateLayer(layer);
        }

        m_fUpdateTime = Time.time;
    }

    public void SetChunk(Chunk c)
    {
        m_chunk = c;

        foreach (var l in m_layers)
            DestroyLayer(l);
        m_layers.Clear();

        m_fUpdateTime = -1;
    }

    public void SetScale(float x, float y, float z)
    {
        m_scaleX = x;
        m_scaleY = y;
        m_scaleZ = z;
    }

    public void SetPosition(int x, int z)
    {
        m_x = x;
        m_z = z;
    }

    void DestroyLayer(LayerRender layer)
    {
        foreach (var o in layer.objects)
        {
            if (o.meshFilter.mesh != null)
                Destroy(o.meshFilter.mesh);
            Destroy(o.meshFilter.gameObject);
        }
    }

    void UpdateLayer(LayerRender layer)
    {
        m_meshParams.ResetSize();

        foreach(var pass in m_pass)
            pass.Render(m_chunk, m_x, layer.layerIndex, m_z, 1, 1, 1, m_meshParams);

        var materials = m_meshParams.GetNonEmptyMaterials();
        //remove
        for(int i = 0; i < layer.objects.Count; i++)
        {
            var m = layer.objects[i].meshRenderer.material;
            if (!materials.Exists(x => { return x == m; }))
            {
                if (layer.objects[i].meshFilter.mesh != null)
                    Destroy(layer.objects[i].meshFilter.mesh);
                Destroy(layer.objects[i].meshFilter.gameObject);
                layer.objects.RemoveAt(i);
                i--;
            }
        }

        //add
        foreach(var m in materials)
        {
            int nbMesh = m_meshParams.GetMeshCount(m);
            int meshIndex = 0;
            
            for(int i = 0; i < layer.objects.Count; i++)
            {
                var obj = layer.objects[i];
                if (obj.meshRenderer.material != m)
                    continue;

                if(meshIndex >= nbMesh)
                {
                    if (obj.meshFilter.mesh != null)
                        Destroy(obj.meshFilter.mesh);
                    Destroy(obj.meshFilter.gameObject);
                    layer.objects.RemoveAt(i);
                    i--;
                }
                else
                {
                    UpdateLayerObject(obj, m, meshIndex);
                    meshIndex++;
                }
            }

            for (; meshIndex < nbMesh; meshIndex++)
                layer.objects.Add(CreateNewLayerObject(m, meshIndex, layer.layerIndex));
        }

        m_meshParams.ResetSize();
    }

    void UpdateLayerObject(LayerObject obj, Material material, int index)
    {
        var data = m_meshParams.GetMesh(material, index);

        var mesh = obj.meshFilter.mesh;
        MeshEx.SetWorldMeshParams(mesh, data.verticesSize, data.indexesSize);
        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    LayerObject CreateNewLayerObject(Material material, int index, int layer)
    {
        LayerObject obj = new LayerObject();

        GameObject o = new GameObject();
        var transform = o.GetComponent<Transform>();
        obj.meshFilter = o.AddComponent<MeshFilter>();
        obj.meshRenderer = o.AddComponent<MeshRenderer>();
        obj.meshRenderer.material = material;
        obj.meshFilter.mesh = new Mesh();
        transform.parent = this.transform;
        transform.localPosition = new Vector3(0, layer * m_scaleY * Chunk.chunkSize, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(m_scaleX, m_scaleY, m_scaleZ);

        return obj;
    }
}
