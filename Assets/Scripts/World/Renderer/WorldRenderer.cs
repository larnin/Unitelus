using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldRenderer : MonoBehaviour
{
    class ChunkInfos
    {
        public int x;
        public int y;
        public ChunkRenderer renderer;
    }

    [SerializeField] int m_renderSize = 10;
    [SerializeField] float m_moveUpdateDistance = 2;

    SubscriberList m_subscriberList = new SubscriberList();

    List<ChunkInfos> m_chunks = new List<ChunkInfos>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<CenterUpdatedEvent>.Subscriber(OnCenterUpdate));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnCenterUpdate(CenterUpdatedEvent e)
    {
        List<ChunkInfos> updatedList = new List<ChunkInfos>();

        int minX, minY, maxX, maxY;
        GetVisibleChunksQuad(e.pos, out minX, out minY, out maxX, out maxY);

        for(int i = minX; i <= maxX; i++)
        {
            for(int j = minY; j <= maxY; j++)
            {
                var c = m_chunks.Find(x => { return x.x == i && x.y == j; });
                if (c == null)
                    updatedList.Add(CreateChunk(i, j));
                else updatedList.Add(c);
            }
        }

        foreach(var c in m_chunks)
        {
            bool isAdded = updatedList.Exists(x => { return c.x == x.x && c.y == x.y; });
            if (!isAdded)
                RemoveChunk(c);
        }

        m_chunks = updatedList;
    }

    void GetVisibleChunksQuad(Vector3 center, out int minChunkX, out int minChunkY, out int maxChunkX, out int maxChunkY)
    {
        var world = PlaceholderWorld.instance.world;

        Vector3 localCenter = transform.InverseTransformPoint(center);

        int minX = Mathf.FloorToInt(localCenter.x) - m_renderSize;
        int minY = Mathf.FloorToInt(localCenter.y) - m_renderSize;
        int maxX = Mathf.FloorToInt(localCenter.x) + m_renderSize;
        int maxY = Mathf.FloorToInt(localCenter.y) + m_renderSize;

        world.PosToUnclampedChunkPos(minX, minY, out minChunkX, out minChunkY);
        world.PosToUnclampedChunkPos(maxX, maxY, out maxChunkX, out maxChunkY);
    }

    ChunkInfos CreateChunk(int x, int y)
    {
        var world = PlaceholderWorld.instance.world;

        var obj = new GameObject("Chunk[" + x + " " + y + "]");
        var renderer = obj.AddComponent<ChunkRenderer>();
        renderer.SetChunk(world.GetChunk(x, y));
        renderer.SetPosition(x, y);

        var transform = obj.transform;
        transform.parent = this.transform;
        transform.localScale = new Vector3(1, 1, 1);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(x * Chunk.chunkSize, y * Chunk.chunkSize, 0);

        ChunkInfos infos = new ChunkInfos();
        infos.x = x;
        infos.y = y;
        infos.renderer = renderer;

        return infos;
    }

    void RemoveChunk(ChunkInfos c)
    {
        Destroy(c.renderer.gameObject);
    }
}
