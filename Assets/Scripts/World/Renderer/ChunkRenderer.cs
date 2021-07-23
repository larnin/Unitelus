using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    bool m_chunkInitialized = false;

    class LayerObject
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
    }

    class LayerRender
    {
        public List<LayerObject> objects = new List<LayerObject>();
        public List<MeshCollider> colliders = new List<MeshCollider>();
        public int layerIndex = 0;
    }

    class WaitingJob
    {
        public int layerIndex = 0;
        public int jobID = 0;
    }

    List<LayerRender> m_layers = new List<LayerRender>();

    List<WaitingJob> m_waitingJobs = new List<WaitingJob>();

    private void OnDestroy()
    {
        foreach(var job in m_waitingJobs)
        {
            ChunkRendererPool.instance.FreeJob(job.jobID);
        }
    }

    public void Update()
    {
        if (m_chunk == null)
            return;

        for(int i = 0; i < m_waitingJobs.Count(); i++)
        {
            if(CheckLayerUpdated(m_waitingJobs[i]))
            {
                m_waitingJobs.RemoveAt(i);
                i--;
            }
        }

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

        m_fUpdateTime = TimeEx.GetTime();

        if (!m_chunkInitialized)
            m_chunkInitialized = m_waitingJobs.Count == 0;
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
        int jobID = ChunkRendererPool.instance.AddJob(m_x, m_z, layer.layerIndex, m_chunk.world);
        var item = m_waitingJobs.Find(x => { return x.jobID == jobID; });
        if(item == null)
        {
            WaitingJob job = new WaitingJob();
            job.jobID = jobID;
            job.layerIndex = layer.layerIndex;
            m_waitingJobs.Add(job);
        }
    }

    //return true if updated
    bool CheckLayerUpdated(WaitingJob job)
    {
        var layer = m_layers.Find(x => { return x.layerIndex == job.layerIndex; });
        if (layer == null)
        {
            ChunkRendererPool.instance.FreeJob(job.jobID);
            return true;
        }

        var meshParams = ChunkRendererPool.instance.GetJobData(job.jobID);
        if (meshParams == null)
        {
            //no job ? create it again !
            if (!ChunkRendererPool.instance.HaveJob(job.jobID))
                job.jobID = ChunkRendererPool.instance.AddJob(m_x, m_z, job.layerIndex, m_chunk.world);
            return false;
        }
        
        var materials = meshParams.GetNonEmptyMaterials();
        int nbMesh = 0;
        foreach (var m in materials)
            nbMesh += meshParams.GetMeshCount(m);
        int nbColliderMesh = meshParams.GetColliderMeshCount();

        //remove
        while (layer.objects.Count > nbMesh)
        {
            var l = layer.objects[layer.objects.Count - 1];
            if(l.meshFilter.mesh != null)
                Destroy(l.meshFilter.mesh);
            Destroy(l.meshFilter.gameObject);
            layer.objects.RemoveAt(layer.objects.Count - 1);
        }

        while(layer.colliders.Count > nbColliderMesh)
        {
            var l = layer.colliders[layer.colliders.Count - 1];
            if (l.sharedMesh != null)
                Destroy(l.sharedMesh);
            Destroy(l.gameObject);
            layer.colliders.RemoveAt(layer.colliders.Count - 1);
        }

        //add
        while(layer.objects.Count < nbMesh)
            layer.objects.Add(CreateNewLayerObject(layer.layerIndex));

        while (layer.colliders.Count < nbColliderMesh)
            layer.colliders.Add(CreateNewLayerCollider(layer.layerIndex));

        //set
        int meshIndex = 0;
        foreach(var m in materials)
        {
            nbMesh = meshParams.GetMeshCount(m);

            for(int i = 0; i < nbMesh; i++)
            {
                var obj = layer.objects[meshIndex];
                UpdateLayerObject(obj, m, i, meshParams);
            }
        }

        for(int i = 0; i < nbColliderMesh; i++)
        {
            var obj = layer.colliders[i];
            UpdateLayerCollider(obj, i, meshParams);
        }
        
        ChunkRendererPool.instance.FreeJob(job.jobID);

        return true;
    }

    void UpdateLayerObject(LayerObject obj, Material material, int index, MeshParams<WorldVertexDefinition> meshParams)
    {
        var data = meshParams.GetMesh(material, index);

        obj.meshRenderer.material = material;

        var mesh = obj.meshFilter.mesh;

        MeshEx.SetWorldMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        //full chunk layer
        mesh.bounds = new Bounds(new Vector3(Chunk.chunkSize, Chunk.chunkSize, Chunk.chunkSize) / 2, new Vector3(Chunk.chunkSize, Chunk.chunkSize, Chunk.chunkSize));
    }

    void UpdateLayerCollider(MeshCollider obj, int index, MeshParams<WorldVertexDefinition> meshParams)
    {
        var data = meshParams.GetColliderMesh(index);

        var mesh = obj.sharedMesh;

        MeshEx.SetColliderMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        //full chunk layer
        mesh.bounds = new Bounds(new Vector3(Chunk.chunkSize, Chunk.chunkSize, Chunk.chunkSize) / 2, new Vector3(Chunk.chunkSize, Chunk.chunkSize, Chunk.chunkSize));
    }

    LayerObject CreateNewLayerObject(int layer)
    {
        LayerObject obj = new LayerObject();

        GameObject o = new GameObject("Layer " + layer );
        o.layer = gameObject.layer;
        var transform = o.GetComponent<Transform>();
        obj.meshFilter = o.AddComponent<MeshFilter>();
        obj.meshRenderer = o.AddComponent<MeshRenderer>();
        obj.meshFilter.mesh = new Mesh();
        transform.parent = this.transform;
        transform.localPosition = new Vector3(0, layer * m_scaleY * Chunk.chunkSize, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(m_scaleX, m_scaleY, m_scaleZ);

        return obj;
    }

    MeshCollider CreateNewLayerCollider(int layer)
    {
        GameObject o = new GameObject("Collider " + layer);
        o.layer = gameObject.layer;
        var transform = o.GetComponent<Transform>();
        var collider = o.AddComponent<MeshCollider>();
        collider.sharedMesh = new Mesh();
        transform.parent = this.transform;
        transform.localPosition = new Vector3(0, layer * m_scaleY * Chunk.chunkSize, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(m_scaleX, m_scaleY, m_scaleZ);

        return collider;
    }

    public bool AreAllRenderReady()
    {
        return m_chunkInitialized;
    }
}
