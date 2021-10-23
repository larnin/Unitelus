using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BiomeGenerator
{
    int m_seed;
    int m_size;
    BiomesSettings m_settings;

    Matrix<BiomeType> m_grid;

    public void Generate(BiomesSettings settings, int size, int seed)
    {
        m_seed = seed;
        m_size = size;
        m_settings = settings;

        var grid = GenerateGrid();
        if (m_settings.smoothSize < 0.5f)
            m_grid = grid;
        else m_grid = SmoothGrid(grid, m_settings.smoothSize);
    }

    Matrix<BiomeType> GenerateGrid()
    {
        Matrix<Vector3> values = new Matrix<Vector3>(m_size, m_size);
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        List<Perlin> xPerlin = new List<Perlin>();
        foreach (var p in m_settings.noiseX)
            xPerlin.Add(new Perlin(m_size, p.amplitude, p.frequency, m_seed++));
        List<Perlin> yPerlin = new List<Perlin>();
        foreach (var p in m_settings.noiseY)
            yPerlin.Add(new Perlin(m_size, p.amplitude, p.frequency, m_seed++));
        List<Perlin> zPerlin = new List<Perlin>();
        foreach (var p in m_settings.noiseZ)
            zPerlin.Add(new Perlin(m_size, p.amplitude, p.frequency, m_seed++));

        for(int i = 0; i < m_size; i++)
        {
            for(int j = 0; j < m_size; j++)
            {
                Vector3 value = Vector3.zero;
                foreach (var p in xPerlin)
                    value.x += p.Get(i, j);
                foreach (var p in yPerlin)
                    value.y += p.Get(i, j);
                foreach (var p in zPerlin)
                    value.z += p.Get(i, j);
                values.Set(i, j, value);

                min.x = Mathf.Min(min.x, value.x);
                min.y = Mathf.Min(min.y, value.y);
                min.z = Mathf.Min(min.z, value.z);

                max.x = Mathf.Max(max.x, value.x);
                max.y = Mathf.Max(max.y, value.y);
                max.z = Mathf.Max(max.z, value.z);
            }
        }

        Vector3 offset = -min;
        Vector3 multiplier = max - min;
        if (multiplier.x < 0.0001f)
            multiplier.x = 1;
        else multiplier.x = 1 / multiplier.x;
        if (multiplier.y < 0.0001f)
            multiplier.y = 1;
        else multiplier.y = 1 / multiplier.y;
        if (multiplier.z < 0.0001f)
            multiplier.z = 1;
        else multiplier.z = 1 / multiplier.z;

        Matrix<BiomeType> grid = new Matrix<BiomeType>(m_size, m_size);
        grid.SetAll(BiomeType.Invalid);

        for (int i = 0; i < m_size; i++)
        {
            for (int j = 0; j < m_size; j++)
            {
                Vector3 value = values.Get(i, j);
                value += offset;
                value.x *= multiplier.x;
                value.y *= multiplier.y;
                value.z *= multiplier.z;

                grid.Set(i, j, GetBiomeAt(value));
            }
        }

        return grid;
    }

    Matrix<BiomeType> SmoothGrid(Matrix<BiomeType> biomes, float smoothRadius)
    {
        List<float> weights = new List<float>();
        Matrix<BiomeType> newGrid = new Matrix<BiomeType>(biomes.width, biomes.depth); 

        int radius = Mathf.CeilToInt(smoothRadius);
        for(int i = 0; i < biomes.width; i++)
        {
            for(int j = 0; j < biomes.depth; j++)
            {
                GetBiomesAtRadius(biomes, i, j, smoothRadius, weights);

                int maxIndex = 0;
                for (int k = 1; k < weights.Count; k++)
                    if (weights[k] > weights[maxIndex])
                        maxIndex = k;

                newGrid.Set(i, j, (BiomeType)maxIndex);
            }
        }

        return newGrid;
    }

    BiomeType GetBiomeAt(Vector3 pos)
    {
        BiomeType bestBiome = m_settings.defaultBiome;
        float bestWeight = float.MinValue;

        foreach(var b in m_settings.biomes)
        {
            if (b.weight > bestWeight && b.bounds.Contains(pos))
            {
                bestBiome = b.biome;
                bestWeight = b.weight;
            }
        }

        return bestBiome;
    }

    void GetBiomesAtRadius(Matrix<BiomeType> biomes, int x, int y, float radius, List<float> weights)
    {
        int nbBiome = Enum.GetValues(typeof(BiomeType)).Length;
        while (weights.Count < nbBiome)
            weights.Add(0);
        while (weights.Count > nbBiome)
            weights.RemoveAt(weights.Count - 1);
        for (int i = 0; i < weights.Count; i++)
            weights[i] = 0;

        int iRadius = Mathf.CeilToInt(radius);

        for (int k = -iRadius; k <= iRadius; k++)
        {
            for (int l = -iRadius; l <= iRadius; l++)
            {
                float normalizedDist = Mathf.Sqrt(k * k + l * l) / radius;
                if (normalizedDist > 1)
                    continue;

                float weight = Lerp.Square(1, 0, normalizedDist);

                int pX = x + k;
                if (pX < 0) pX += biomes.width;
                else if (pX >= biomes.width) pX -= biomes.width;
                int pY = y + l;
                if (pY < 0) pY += biomes.depth;
                else if (pY >= biomes.depth) pY -= biomes.depth;

                var biome = biomes.Get(pX, pY);
                weights[(int)biome] += weight;
            }
        }
    }

    public BiomeType GetBiome(int x, int y)
    {
        Debug.Assert(x >= 0 && x < m_grid.width);
        Debug.Assert(y >= 0 && y < m_grid.depth);

        return m_grid.Get(x, y);
    }

    public List<float> GetBiomesWeights(int x, int y, float radius)
    {
        List<float> weights = new List<float>();
        GetBiomesWeightsNoAlloc(x, y, radius, weights);
        return weights;
    }

    public void GetBiomesWeightsNoAlloc(int x, int y, float radius, List<float> weights)
    {
        if (weights == null)
            return;

        GetBiomesAtRadius(m_grid, x, y, radius, weights);
    }
}