using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PlaceholderBlockInfos : SerializedMonoBehaviour
{
    public List<BlockTypeBase> m_blockRenderer = new List<BlockTypeBase>();

    private void Awake()
    {
        foreach(var b in m_blockRenderer)
        {
            BlockTypeList.instance.Set(b.id, b);
        }
    }
}
