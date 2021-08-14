using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;
using UnityEngine;

public class VoronoiBiomes
{
    class Vertex
    {
        public float x;
        public float y;
        public BiomeType biome;
        public Vertex(float _x, float _y, BiomeType _biome)
        {
            x = _x;
            y = _y;
            biome = _biome;
        }
    }

    VoronoiBiomesSettings m_settings;
    float m_moistureMin;
    float m_moistureMax;
    float m_themperatureMin;
    float m_themperatureMax;
    int m_size;

    List<Vertex> m_vertices = new List<Vertex>();

    MT19937 m_rand;

    //size in block
    public void Generate(VoronoiBiomesSettings settings, int size)
    {
        m_settings = settings;
        m_size = size;

        m_rand = new MT19937((uint)settings.seed);

        DevelopeSettings();

        GenerateVertices();
        MagnetizeVertices();
        RelaxeVertices();
    }

    void DevelopeSettings()
    {
        m_moistureMin = float.MaxValue;
        m_moistureMax = float.MinValue;
        m_themperatureMin = float.MaxValue;
        m_themperatureMax = float.MinValue;

        foreach(var b in m_settings.validBiomes)
        {
            var biome = BiomeList.instance.Get(b);
            if (biome.moisture < m_moistureMin)
                m_moistureMin = biome.moisture;
            if (biome.moisture > m_moistureMax)
                m_moistureMax = biome.moisture;
            if (biome.themperature < m_themperatureMin)
                m_themperatureMin = biome.themperature;
            if (biome.themperature > m_themperatureMax)
                m_themperatureMax = biome.themperature;
        }
    }

    void GenerateVertices()
    {
        //generate list of biome
        List<float> weights = new List<float>();
        List<int> biomeNbByType = new List<int>();
        foreach(var b in m_settings.validBiomes)
        {
            var biome = BiomeList.instance.Get(b);
            weights.Add(biome.weight);
            biomeNbByType.Add(0);
        }

        DiscreteDistribution d = new DiscreteDistribution(weights);

        for(int i = 0; i < m_settings.nbBiome; i++)
            biomeNbByType[d.Next(m_rand)]++;

        UniformFloatDistribution dPos = new UniformFloatDistribution(m_size);

        for(int i = 0; i < biomeNbByType.Count; i++)
        {
            BiomeType b = m_settings.validBiomes[i];
            for(int j = 0; j < biomeNbByType[i]; j++)
            {
                float x = dPos.Next(m_rand);
                float y = dPos.Next(m_rand);

                var vertex = new Vertex(x, y, b);
                m_vertices.Add(vertex);
            }
        }
    }

    //push the biomes that are not compatible (desert/snow)
    void MagnetizeVertices()
    {
        for(int i = 0; i < m_settings.magnetPower; i++)
        {
            List<Vertex> newVertices = new List<Vertex>();
            for(int j = 0; j < m_vertices.Count; j++)
            {
                Vertex newVertex = new Vertex(m_vertices[j].x, m_vertices[j].y, m_vertices[j].biome);

                BiomeInfo biome = BiomeList.instance.Get(m_vertices[j].biome);
                float normalizedMoisture = NormalizeMoisture(biome.moisture);
                float normalizedThemperature = NormalizeThemperature(biome.themperature);
                Vector2 pos = new Vector2(m_vertices[j].x, m_vertices[j].y);

                for (int k = 0; k < m_vertices.Count; k++)
                {
                    if (j == k) continue;

                    BiomeInfo otherBiome = BiomeList.instance.Get(m_vertices[k].biome);
                    float otherNormalizedMoisture = NormalizeMoisture(otherBiome.moisture);
                    float otherNormalizedThemperature = NormalizeThemperature(otherBiome.themperature);

                    var offset = GetOffset(pos, new Vector2(m_vertices[k].x, m_vertices[k].y));

                    float distance = offset.magnitude;

                    float parameter = (new Vector2(normalizedMoisture, normalizedThemperature) - new Vector2(otherNormalizedMoisture, otherNormalizedThemperature)).magnitude;

                    parameter *= m_settings.magnetForce / (distance + 1);

                    var dir = offset / distance;
                    dir *= parameter;
                    newVertex.x += dir.x;
                    newVertex.y += dir.y;
                }

                newVertices.Add(newVertex);
            }

            m_vertices = newVertices;
        }
    }

    //push biomes to simulate rain drop
    void RelaxeVertices()
    {
        for (int i = 0; i < m_settings.relaxingPower; i++)
        {
            List<Vertex> newVertices = new List<Vertex>();
            for (int j = 0; j < m_vertices.Count; j++)
            {
                Vertex newVertex = new Vertex(m_vertices[j].x, m_vertices[j].y, m_vertices[j].biome);

                BiomeInfo biome = BiomeList.instance.Get(m_vertices[j].biome);

                Vector2 pos = new Vector2(m_vertices[j].x, m_vertices[j].y);

                for (int k = 0; k < m_vertices.Count; k++)
                {
                    if (j == k) continue;

                    BiomeInfo otherBiome = BiomeList.instance.Get(m_vertices[k].biome);

                    float size = biome.size + otherBiome.size;

                    var offset = GetOffset(new Vector2(m_vertices[k].x, m_vertices[k].y), pos);

                    float distance = offset.magnitude;
                    
                    float parameter = m_settings.relaxingForce * size / (distance + 1);

                    var dir = offset / distance;
                    dir *= parameter;
                    newVertex.x += dir.x;
                    newVertex.y += dir.y;
                }

                newVertices.Add(newVertex);
            }

            m_vertices = newVertices;
        }
    }

    float NormalizeMoisture(float moisture)
    {
        if (m_moistureMax - m_moistureMin <= 0)
            return 0;

        return (moisture - m_moistureMin) / (m_moistureMax - m_moistureMin);
    }

    float NormalizeThemperature(float themperature)
    {
        if (m_themperatureMax - m_themperatureMin <= 0)
            return 0;

        return (themperature - m_themperatureMin) / (m_themperatureMax - m_themperatureMin);
    }

    Vector2 GetOffset(Vector2 pos1, Vector2 pos2)
    {
        pos1 = ClampPos(pos1);
        pos2 = ClampPos(pos2);

        if (pos2.x < pos1.x)
            pos2.x += m_size;
        if (pos2.y < pos1.y)
            pos2.y += m_size;

        Vector2[] offsets = new Vector2[4];
        offsets[0] = pos2 - pos1;
        offsets[1] = pos2 - new Vector2(pos1.x, pos1.y + m_size);
        offsets[2] = pos2 - new Vector2(pos1.x + m_size, pos1.y);
        offsets[3] = pos2 - new Vector2(pos1.x + m_size, pos1.y + m_size);

        float minDist = offsets[0].sqrMagnitude;
        int minIndex = 0;
        for(int i = 1; i < 4; i++)
        {
            float dist = offsets[i].sqrMagnitude;
            if(dist < minDist)
            {
                minDist = dist;
                minIndex = i;
            }
        }

        return offsets[minIndex];
    }

    float GetDistance(Vector2 pos1, Vector2 pos2)
    {
        var offset = GetOffset(pos1, pos2);
        return offset.magnitude;
    }

    Vector2 ClampPos(Vector2 pos)
    {
        if (pos.x < 0)
            pos.x = (pos.x % m_size + m_size) % m_size;
        else pos.x = pos.x % m_size;
        if (pos.y < 0)
            pos.y = (pos.y % m_size + m_size) % m_size;
        else pos.y = pos.y % m_size;

        return pos;
    }

    public BiomeType GetNearestBiome(Vector2 pos)
    {
        float minDistance = float.MaxValue;
        BiomeType bestBiome = BiomeType.Invalid;

        foreach(var v in m_vertices)
        {
            var biome = BiomeList.instance.Get(v.biome);
            var offset = GetDistance(pos, new Vector2(v.x, v.y)) / biome.size;
            if(offset < minDistance)
            {
                minDistance = offset;
                bestBiome = v.biome;
            }
        }

        return bestBiome;
    }
}
