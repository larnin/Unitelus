using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceholderBiomeInfos : MonoBehaviour
{
    public List<BiomeInfo> m_biomes = new List<BiomeInfo>();

    private void Awake()
    {
        foreach (var b in m_biomes)
        {
            BiomeList.instance.Set(b);
        }
    }
}
