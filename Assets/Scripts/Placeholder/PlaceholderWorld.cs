using UnityEngine;
using System.Collections;

public class PlaceholderWorld : MonoBehaviour
{
    [SerializeField] WorldGeneratorSettings m_settings = new WorldGeneratorSettings();

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
        m_world = WorldGenerator.Generate(m_settings);
    }
}
