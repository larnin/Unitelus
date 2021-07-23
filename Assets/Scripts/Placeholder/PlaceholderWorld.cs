using UnityEngine;
using System.Collections;

public class PlaceholderWorld : MonoBehaviour
{
    [SerializeField] WorldGeneratorSettings m_settings = new WorldGeneratorSettings();

    WorldGenerator m_generator = null;

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

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        m_generator = new WorldGenerator();
        m_generator.Generate(m_settings);
    }

    private void Update()
    {
        if (m_world == null && m_generator.state == WorldGenerator.State.finished)
            m_world = m_generator.world;
    }
}
