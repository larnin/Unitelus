using UnityEngine;
using System.Collections;

public class PlaceholderWorld : MonoBehaviour
{
    static PlaceholderWorld m_instance = null;
    public static PlaceholderWorld instance
    {
        get { return m_instance; }
        private set
        {
            if (m_instance != null)
                Debug.LogError("2 PlaceholderWorld instancied");
            m_instance = value;
        }
    }

    World m_world = null;
    public World world { get { return m_world; } }

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        instance = this;

        m_subscriberList.Add(new Event<WorldCreatedEvent>.Subscriber(OnWorldCreated));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnWorldCreated(WorldCreatedEvent e)
    {
        m_world = e.world;
    }

    private void Update()
    {
        if (m_world == null)
            return;

        var biomes = m_world.m_biomes;
        var size = m_world.size;

        float y = 3.1f;

        Debug.DrawLine(new Vector3(0, y, 0), new Vector3(0, y, size), Color.magenta);
        Debug.DrawLine(new Vector3(0, y, size), new Vector3(size, y, size), Color.magenta);
        Debug.DrawLine(new Vector3(size, y, size), new Vector3(size, y, 0), Color.magenta);
        Debug.DrawLine(new Vector3(size, y, 0), new Vector3(0, y, 0), Color.magenta);
        
        for (int i = 0; i < biomes.m_triangles.Count; i++)
        {
            var triangle = biomes.m_triangles[i];

            var p1 = biomes.m_localVertices[triangle.index1];
            var p2 = biomes.m_localVertices[triangle.index2];
            var p3 = biomes.m_localVertices[triangle.index3];

            var pos1 = biomes.GetLocalVertexPosition(p1);
            var pos2 = biomes.GetLocalVertexPosition(p2);
            var pos3 = biomes.GetLocalVertexPosition(p3);

            Debug.DrawLine(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), Color.red);
            Debug.DrawLine(new Vector3(pos2.x, y, pos2.y), new Vector3(pos3.x, y, pos3.y), Color.red);
            Debug.DrawLine(new Vector3(pos3.x, y, pos3.y), new Vector3(pos1.x, y, pos1.y), Color.red);
        }
    }

    void OnDrawGizmos()
    {
        if (m_world == null)
            return;

        var biomes = m_world.m_biomes;

        float y = 3;

        Gizmos.color = Color.red;

        for (int i = 0; i < biomes.m_vertices.Count; i++)
        {
            var pos = new Vector3(biomes.m_vertices[i].x, y, biomes.m_vertices[i].y);

            Gizmos.DrawSphere(pos, 1.5f);
        }
        
    }
}
