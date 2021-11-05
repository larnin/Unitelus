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
        if(m_world != null && m_world.m_borders != null)
            m_world.m_borders.Draw();
    }
}
