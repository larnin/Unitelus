using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceholderPass : MonoBehaviour
{
    public List<LayerRendererPassBase> m_pass = new List<LayerRendererPassBase>();

    static PlaceholderPass m_instance = null;
    public static PlaceholderPass instance
    {
        get { return m_instance; }
        private set
        {
            if (m_instance != null)
                Debug.LogError("2 PlaceholderPass instancied");
            m_instance = value;
        }
    }

    private void Awake()
    {
        instance = this;
        m_pass.Add(new LayerRenderPassBlocks());
    }
}
