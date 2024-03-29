﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldRenderer : MonoBehaviour
{
    class ChunkInfos
    {
        public int x;
        public int z;
        public ChunkRenderer renderer;
    }

    [SerializeField] int m_renderSize = 10;
    [SerializeField] float m_moveUpdateDistance = 2;
    [SerializeField] [Layer] int m_collisionLayer = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    List<ChunkInfos> m_chunks = new List<ChunkInfos>();

    Vector3 m_lastPos = new Vector3(1000000, 1000000, 1000000);

    private void Awake()
    {
        m_subscriberList.Add(new Event<CenterUpdatedEvent>.Subscriber(OnCenterUpdate));
        m_subscriberList.Add(new Event<CenterUpdatedEventInstant>.Subscriber(OnCenterUpdate));
        m_subscriberList.Add(new Event<IsChunkRenderedEvent>.Subscriber(IsChunkRendered));
        m_subscriberList.Add(new Event<GetChunkRenderedCountEvent>.Subscriber(GetChunkRenderedCount));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnCenterUpdate(CenterUpdatedEventInstant e)
    {
        OnCenterUpdate(e.pos);
    }

    void OnCenterUpdate(CenterUpdatedEvent e)
    {
        OnCenterUpdate(e.pos);
    }

    void OnCenterUpdate(Vector3 pos)
    {
        if ((pos - m_lastPos).sqrMagnitude < m_moveUpdateDistance * m_moveUpdateDistance)
            return;
        m_lastPos = pos;

        List<ChunkInfos> updatedList = new List<ChunkInfos>();

        int minX, minZ, maxX, maxZ;
        GetVisibleChunksQuad(pos, out minX, out minZ, out maxX, out maxZ);

        for(int i = minX; i <= maxX; i++)
        {
            for(int j = minZ; j <= maxZ; j++)
            {
                var c = m_chunks.Find(x => { return x.x == i && x.z == j; });
                if (c == null)
                    updatedList.Add(CreateChunk(i, j));
                else updatedList.Add(c);
            }
        }

        foreach(var c in m_chunks)
        {
            bool isAdded = updatedList.Exists(x => { return c.x == x.x && c.z == x.z; });
            if (!isAdded)
                RemoveChunk(c);
        }

        m_chunks = updatedList;
    }

    void GetVisibleChunksQuad(Vector3 center, out int minChunkX, out int minChunkZ, out int maxChunkX, out int maxChunkZ)
    {
        var world = PlaceholderWorld.instance.world;

        Vector3 localCenter = transform.InverseTransformPoint(center);

        int minX = Mathf.FloorToInt(localCenter.x) - m_renderSize;
        int minZ = Mathf.FloorToInt(localCenter.z) - m_renderSize;
        int maxX = Mathf.FloorToInt(localCenter.x) + m_renderSize;
        int maxZ = Mathf.FloorToInt(localCenter.z) + m_renderSize;

        world.PosToUnclampedChunkPos(minX, minZ, out minChunkX, out minChunkZ);
        world.PosToUnclampedChunkPos(maxX, maxZ, out maxChunkX, out maxChunkZ);
    }

    ChunkInfos CreateChunk(int x, int z)
    {
        var world = PlaceholderWorld.instance.world;

        var obj = new GameObject("Chunk[" + x + " " + z + "]");
        obj.layer = m_collisionLayer;
        var renderer = obj.AddComponent<ChunkRenderer>();
        renderer.SetChunk(world.GetChunk(x, z));
        renderer.SetPosition(x, z);

        var transform = obj.transform;
        transform.parent = this.transform;
        transform.localScale = new Vector3(1, 1, 1);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(x * Chunk.chunkSize, 0, z * Chunk.chunkSize);

        ChunkInfos infos = new ChunkInfos();
        infos.x = x;
        infos.z = z;
        infos.renderer = renderer;

        return infos;
    }

    void RemoveChunk(ChunkInfos c)
    {
        Destroy(c.renderer.gameObject);
    }

    bool IsChunkRendered(int x, int z)
    {
        foreach (var c in m_chunks)
        {
            if (c.x == x && c.z == z)
                return c.renderer.AreAllRenderReady();
        }

        return false;
    }

    void IsChunkRendered(IsChunkRenderedEvent e)
    {
        e.rendered = IsChunkRendered(e.x, e.z);
    }

    void GetChunkRenderedCount(GetChunkRenderedCountEvent e)
    {
        e.totalChunkNb = m_chunks.Count;
        e.rederedChunkNb = 0;

        foreach(var c in m_chunks)
        {
            if (c.renderer.AreAllRenderReady())
                e.rederedChunkNb++;
        }
    }
}
