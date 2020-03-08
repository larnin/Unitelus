using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceholderBlockInfos : MonoBehaviour
{
    public List<BlockRendererBase> m_blockRenderer = new List<BlockRendererBase>();

    static PlaceholderBlockInfos m_instance = null;
    public static PlaceholderBlockInfos instance
    {
        get { return m_instance; }
        private set
        {
            if (m_instance != null)
                Debug.LogError("2 PlaceholderBlockInfos instancied");
            m_instance = value;
        }
    }

    private void Awake()
    {
        instance = this;
    }
}
