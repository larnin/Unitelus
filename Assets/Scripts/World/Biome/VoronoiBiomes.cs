using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;
using UnityEngine;

public class VoronoiBiomes
{
    public class Vertex
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

    public class Triangle
    {
        public int index1;
        public int index2;
        public int index3;
        public Triangle(int _index1, int _index2, int _index3)
        {
            index1 = _index1;
            index2 = _index2;
            index3 = _index3;
        }
    }

    VoronoiBiomesSettings m_settings;
    float m_moistureMin;
    float m_moistureMax;
    float m_themperatureMin;
    float m_themperatureMax;
    int m_size;

    public List<Vertex> m_vertices = new List<Vertex>();
    public List<Triangle> m_triangles = new List<Triangle>();

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
        GenerateTriangles();
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

                Vector2 vertexPos = new Vector2(newVertex.x, newVertex.y);
                vertexPos = ClampPos(vertexPos);
                newVertex.x = vertexPos.x;
                newVertex.y = vertexPos.y;

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

                Vector2 vertexPos = new Vector2(newVertex.x, newVertex.y);
                vertexPos = ClampPos(vertexPos);
                newVertex.x = vertexPos.x;
                newVertex.y = vertexPos.y;

                newVertices.Add(newVertex);
            }

            m_vertices = newVertices;
        }
    }

    void GenerateTriangles()
    {
        if (m_vertices.Count < 4)
            return;

        //first triangle
        int index1 = -1;
        int index2 = -1;
        Vector2 pos = new Vector2(m_vertices[0].x, m_vertices[0].y);
        for(int i = 1; i < m_vertices.Count; i++)
        {
            Vector2 pos1 = new Vector2(m_vertices[i].x, m_vertices[i].y);
            //pos1 = GetNearerPoint(pos1, pos);

            for(int j = 1; j < m_vertices.Count; j++)
            {
                if (i == j)
                    continue;
                Vector2 pos2 = new Vector2(m_vertices[j].x, m_vertices[j].y);
                //pos2 = GetNearerPoint(pos2, pos);

                Vector2 omega = Utility.TriangleOmega(pos, pos1, pos2);
                //float radius = GetDistance(omega, pos);
                float radiusSqr = (omega - pos).sqrMagnitude;
                //omega = ClampPos(omega);

                bool collision = false;
                for(int k = 1; k < m_vertices.Count; k++)
                {
                    if (k == i || k == j)
                        continue;

                    Vector2 posTest = new Vector2(m_vertices[k].x, m_vertices[k].y);
                    //posTest = GetNearerPoint(posTest, omega);

                    if ((posTest - omega).sqrMagnitude < radiusSqr)
                    //if(GetDistance(posTest, omega) < radius)
                    {
                        collision = true;
                        break;
                    }
                }
                if (collision)
                    continue;

                index1 = i;
                index2 = j;
                break;
            }
            if (index1 >= 0 || index2 >= 0)
                break;
        }

        //must have found a triangle
        if (index1 < 0 || index2 < 0)
            return;

        //now check all the vertices to make a Delaunay triangulation
        List<int> validPoints = new List<int>();
        for (int i = 0; i < m_vertices.Count; i++)
            validPoints.Add(i);
        List<int> border = new List<int>();
        border.Add(0);
        border.Add(index1);
        border.Add(index2);
        m_triangles.Add(new Triangle(0, index1, index2));

        int skipCount = 0;

        bool isValid = true;
        while(border.Count >= 3)
        {
            int index = skipCount + 1;
            if (index == border.Count) 
                index = 0;
            Vector2 pos1 = new Vector2(m_vertices[border[skipCount]].x, m_vertices[border[skipCount]].y);
            Vector2 pos2 = new Vector2(m_vertices[border[index]].x, m_vertices[border[index]].y);
            //pos2 = GetNearerPoint(pos2, pos1);

            bool found = false;
            for(int i = 0; i < validPoints.Count; i++)
            {
                int verticeIndex = validPoints[i];
                if (verticeIndex == border[skipCount] || verticeIndex == border[index])
                    continue;

                bool isTrangleValid = true;
                //check if this triangle is already validated
                for(int j = 0; j < m_triangles.Count; j++)
                {
                    if(AreSameTriangle(m_triangles[j].index1, m_triangles[j].index2, m_triangles[j].index3, border[skipCount], border[index], verticeIndex))
                    {
                        isTrangleValid = false;
                        break;
                    }
                }

                if (!isTrangleValid)
                    continue;

                int previousIndex = skipCount == 0 ? border.Count - 1 : skipCount - 1;
                int nextIndex = index == border.Count - 1 ? 0 : index + 1;

                bool isPrevious = verticeIndex == border[previousIndex];
                bool isNext = verticeIndex == border[nextIndex];
                //check if tested point is on border to not cut the current border loop in multiple loop
                if (!isPrevious && !isNext)
                {
                    bool isOnBorder = false;
                    for (int j = 0; j < border.Count; j++)
                    {
                        if (border[j] == verticeIndex)
                        {
                            isOnBorder = true;
                            break;
                        }
                    }
                    if (isOnBorder)
                        continue;
                }

                Vector2 pos3 = new Vector2(m_vertices[verticeIndex].x, m_vertices[verticeIndex].y);
                //pos3 = GetNearerPoint(pos3, pos1);

                Vector2 omega = Utility.TriangleOmega(pos1, pos2, pos3);
                //float radius = GetDistance(omega, pos1);
                float radiusSqr = (omega - pos1).sqrMagnitude;
                //omega = ClampPos(omega);

                bool isTriangleValid = true;
                for (int j = 0; j < m_vertices.Count; j++)
                {
                    if (j == border[skipCount] || j == border[index] || j == verticeIndex)
                        continue;

                    Vector2 posTest = new Vector2(m_vertices[j].x, m_vertices[j].y);
                    //posTest = GetNearerPoint(posTest, omega);

                    if((omega - posTest).sqrMagnitude < radiusSqr)
                    //if(GetDistance(omega, posTest) < radius)
                    {
                        isTriangleValid = false;
                        break;
                    }
                }

                if (!isTriangleValid)
                    continue;

                found = true;

                m_triangles.Add(new Triangle(border[skipCount], border[index], verticeIndex));

                if(isPrevious)
                {
                    validPoints.Remove(border[skipCount]);
                    border.RemoveAt(skipCount);
                }
                else if(isNext)
                {
                    validPoints.Remove(border[index]);
                    border.RemoveAt(index);
                }
                else border.Insert(skipCount + 1, verticeIndex);
                break;
            }

            if (!found)
            {
                skipCount++;
                if (skipCount == border.Count)
                {
                    isValid = false;
                    break;
                }
            }
            else skipCount = 0;
        }

        //todo something here, this must not happen
        if (!isValid)
            return;
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

    public Vector2 GetNearerPoint(Vector2 pos, Vector2 origin)
    {
        Vector2 delta = origin - pos;
        if(Mathf.Abs(delta.x) > m_size / 2)
        {
            if (pos.x < origin.x)
                pos.x += m_size;
            else pos.x -= m_size;
        }
        if(Mathf.Abs(delta.y) > m_size / 2)
        {
            if (pos.y < origin.y)
                pos.y += m_size;
            else pos.y -= m_size;
        }
        return pos;
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

    bool AreSameTriangle(int t1a, int t1b, int t1c, int t2a, int t2b, int t2c)
    {
        int[] indexs1 = new int[] { t1a, t1b, t1c };
        int[] indexs2 = new int[] { t2a, t2b, t2c };

        Array.Sort(indexs1);
        Array.Sort(indexs2);

        return indexs1[0] == indexs2[0] && indexs1[1] == indexs2[1] && indexs1[2] == indexs2[2];
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
