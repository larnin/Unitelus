using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

class BiomeList
{
    static BiomeList m_instance = null;
    public static BiomeList instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new BiomeList();
            return m_instance;
        }
    }

    List<BiomeInfo> m_biomes = new List<BiomeInfo>();

    public BiomeList()
    {
        Set(new BiomeInfo(BiomeType.Invalid, 0, 0, 0, 0));
    }

    public void Set(BiomeInfo biome)
    {
        while ((int)biome.biomeType >= m_biomes.Count)
            m_biomes.Add(null);

        m_biomes[(int)biome.biomeType] = biome;
    }

    public BiomeInfo Get(BiomeType type)
    {
        Assert.IsTrue(m_biomes[0] != null);
        if ((int)type >= m_biomes.Count)
        {
            Assert.IsTrue(false);
            return m_biomes[0];
        }
        if (m_biomes[(int)type] == null)
        {
            Assert.IsTrue(false);
            return m_biomes[0];
        }
        return m_biomes[(int)type];
    }
}