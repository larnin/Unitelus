using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public class MeshParamData<T> where T : struct
{
    public const int maxVertexSize = ushort.MaxValue - 5;

    public NativeArray<T> vertices;
    public int verticesSize = 0;
    public ushort[] indexes = null;
    public int indexesSize = 0;

    public bool CanAllocate(int vertexSize)
    {
        return vertexSize + verticesSize <= maxVertexSize;
    }
}

public class MeshParams<T> where T : struct
{
    const int allocSize = 1000;

    public Dictionary<Material, List<MeshParamData<T>>> m_data = new Dictionary<Material, List<MeshParamData<T>>>();

    public MeshParamData<T> Allocate(int vertexSize, int indexSize, Material material)
    {
        Debug.Assert(vertexSize <= MeshParamData<T>.maxVertexSize);

        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);

        if(data == null)
        {
            data = new List<MeshParamData<T>>();
            m_data[material] = data;
            data.Add(new MeshParamData<T>());
            AllocateVerticesArray(data[0], vertexSize);
            AllocateIndexArray(data[0], indexSize);
        }

        var element = data[data.Count - 1];

        if(!element.CanAllocate(vertexSize))
        {
            var newData = new MeshParamData<T>();
            data.Add(newData);
            AllocateVerticesArray(newData, vertexSize);
            AllocateIndexArray(newData, indexSize);
            element = newData;
        }

        if (element.verticesSize + vertexSize > element.vertices.Length)
            IncreaseVerticesArray(element, vertexSize);
        if (element.indexesSize + indexSize > element.indexes.Length)
            IncreaseIndexArray(element, indexSize);

        return element;
    }

    //only clean index but not resize the buffers
    //only remove the sub buffers if there are more than one MeshParamData for one material
    public void ResetSize()
    {
        foreach(var d in m_data)
        {
            while(d.Value.Count > 1)
                d.Value.RemoveAt(1);

            d.Value[0].indexesSize = 0;
            d.Value[0].verticesSize = 0;
        }
    }

    //clear all buffers
    public void Reset()
    {
        foreach(var d in m_data)
            foreach (var e in d.Value)
                e.vertices.Dispose();

        m_data.Clear();
    }

    public List<Material> GetNonEmptyMaterials()
    {
        List<Material> materials = new List<Material>();

        foreach(var d in m_data)
        {
            if (d.Value[0].vertices.Count() > 0 && d.Value[0].indexes.Count() > 0)
                materials.Add(d.Key);
        }

        return materials;
    }

    public int GetMeshCount(Material material)
    {
        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);

        if (data == null)
            return 0;

        int nb = 0;
        foreach (var d in data)
            if (d.verticesSize > 0 && d.indexesSize > 0)
                nb++;

        return nb;
    }

    public MeshParamData<T> GetMesh(Material material, int index)
    {
        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);
        if (data == null)
            return null;
        if (data.Count <= index)
            return null;
        return data[index];
    }

    void AllocateVerticesArray(MeshParamData<T> data, int addVertices)
    {
        Debug.Assert(!data.vertices.IsCreated);
        Debug.Assert(addVertices <= MeshParamData<T>.maxVertexSize);

        int newSize = addVertices + allocSize;
        if (newSize > MeshParamData<T>.maxVertexSize)
            newSize = MeshParamData<T>.maxVertexSize;

        data.vertices = new NativeArray<T>(newSize, Allocator.Persistent);
    }

    void IncreaseVerticesArray(MeshParamData<T> data, int addVertices)
    {
        Debug.Assert(data.vertices.IsCreated);
        Debug.Assert(data.vertices.Length + addVertices <= MeshParamData<T>.maxVertexSize);

        int newSize = data.vertices.Length + addVertices + allocSize;
        if (newSize > MeshParamData<T>.maxVertexSize)
            newSize = MeshParamData<T>.maxVertexSize;

        var newArray = new NativeArray<T>(newSize, Allocator.Persistent);

        for (int i = 0; i < data.verticesSize; i++)
            newArray[i] = data.vertices[i];
        data.vertices.Dispose();
        data.vertices = newArray;
    }

    void AllocateIndexArray(MeshParamData<T> data, int addIndexes)
    {
        Debug.Assert(data.indexes == null);

        data.indexes = new ushort[addIndexes + allocSize];
    }

    void IncreaseIndexArray(MeshParamData<T> data, int addIndexes)
    {
        Debug.Assert(data.indexes != null);

        int newSize = data.indexes.Length + addIndexes + allocSize;

        var newIndex = new ushort[newSize];

        for (int i = 0; i < data.indexesSize; i++)
            newIndex[i] = data.indexes[i];
        data.indexes = newIndex;
    }
}
