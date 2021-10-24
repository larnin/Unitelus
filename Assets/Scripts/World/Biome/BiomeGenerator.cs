using NRand;
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

        int gridSize = m_size - m_settings.biomeSize;
        if (gridSize < 1)
            gridSize = 1;

        var grid = GenerateFirstGrid(gridSize);

        while(gridSize < m_size - 2)
        {
            gridSize++;
            grid = IncreaseGridSize(grid, gridSize);
        }

        m_grid = GenerateFinalGrid(grid, gridSize);
    }

    Matrix<BiomeType> GenerateFirstGrid(int gridSize)
    {

        gridSize = 1 << gridSize;

        Matrix<BiomeType> grid = new Matrix<BiomeType>(gridSize, gridSize);
        grid.SetAll(m_settings.defaultBiome);

        MT19937 rand = new MT19937((uint)m_seed);

        int nbCell = gridSize * gridSize;
        float totalWeight = 0;
        List<BiomeType> biomes = new List<BiomeType>();
        foreach (var b in m_settings.initialBiomes)
        {
            int nb = Mathf.RoundToInt(nbCell * b.weight / 100);
            for(int i = 0; i < nb; i++)
                biomes.Add(b.biome);
        }
        biomes.Shuffle(rand);
        if (totalWeight > 100)
            totalWeight = 100;

        List<Vector2Int> usedPos = new List<Vector2Int>(nbCell);
        List<Vector2Int> validPos = new List<Vector2Int>(nbCell);
        List<Vector2Int> packedPos = new List<Vector2Int>(nbCell);

        for(int i = 0; i < gridSize; i++)
            for(int j = 0; j < gridSize; j++)
                validPos.Add(new Vector2Int(i, j));

        BernoulliDistribution dPack = new BernoulliDistribution(m_settings.packingProbabiity);
        UniformIntDistribution dIndex = new UniformIntDistribution();

        Action<Vector2Int> testPos = (Vector2Int pos) => 
        {
            if (pos.x < 0)
                pos.x = grid.width - 1;
            if (pos.x >= grid.width)
                pos.x = 0;
            if (pos.y < 0)
                pos.y = grid.depth - 1;
            if (pos.y >= grid.depth)
                pos.y = 0;

            if (usedPos.Contains(pos))
                return;
            if (packedPos.Contains(pos))
                return;
            packedPos.Add(pos);
        };

        int nbBiome = biomes.Count;
        for(int i = 0; i < nbBiome; i++)
        {
            Vector2Int pos;
            if(dPack.Next(rand) && packedPos.Count > 0)
            {
                dIndex.SetParams(packedPos.Count);
                int index = dIndex.Next(rand);
                pos = packedPos[index];
                packedPos.RemoveAt(index);
                validPos.Remove(pos);
            }
            else
            {
                dIndex.SetParams(validPos.Count);
                int index = dIndex.Next(rand);
                pos = validPos[index];
                validPos.RemoveAt(index);
            }

            usedPos.Add(pos);

            testPos(new Vector2Int(pos.x, pos.y - 1));
            testPos(new Vector2Int(pos.x, pos.y + 1));
            testPos(new Vector2Int(pos.x - 1, pos.y));
            testPos(new Vector2Int(pos.x + 1, pos.y));
        }

        //todo place similar biomes next to each other
        for(int i = 0; i < usedPos.Count; i++)
        {
            Vector2Int pos = usedPos[i];
            grid.Set(pos.x, pos.y, biomes[i]);
        }

        return grid;
    }

    Matrix<BiomeType> IncreaseGridSize(Matrix<BiomeType> grid, int gridSize)
    {
        Debug.Assert(grid.width * 2 == (1 << gridSize));

        Matrix<BiomeType> newGrid = new Matrix<BiomeType>(grid.width * 2, grid.depth * 2);

        for(int i = 0; i < grid.width; i++)
            for (int j = 0; j < grid.depth; j++)
            {
                var biome = grid.Get(i, j);

                newGrid.Set(i * 2, j * 2, biome);
                newGrid.Set(i * 2, j * 2 + 1, biome);
                newGrid.Set(i * 2 + 1, j * 2, biome);
                newGrid.Set(i * 2 + 1, j * 2 + 1, biome);
            }

        int sizeIndex = m_size - gridSize;

        List<Vector2Int> validPos = new List<Vector2Int>();
        MT19937 rand = new MT19937((uint)m_seed);

        foreach (var subBiome in m_settings.subBiomes)
        {
            if (subBiome.size != sizeIndex)
                continue;

            validPos.Clear();

            for(int i = 0; i < newGrid.width; i++)
                for(int j = 0; j < newGrid.depth; j++)
                {
                    var biome = newGrid.Get(i, j);
                    if (biome == subBiome.baseBiome)
                        validPos.Add(new Vector2Int(i, j));
                }

            int nb = Mathf.RoundToInt(validPos.Count * subBiome.weight / 100);
            if (nb == 0)
                continue;

            if(validPos.Count <= nb)
                nb = validPos.Count;
            else validPos.Shuffle(rand);

            for (int i = 0; i < nb; i++)
                newGrid.Set(validPos[i].x, validPos[i].y, subBiome.biome);
        }

        return newGrid;
    }

    Matrix<BiomeType> GenerateFinalGrid(Matrix<BiomeType> grid, int currentSize)
    {
        Debug.Assert(grid.width == (1 << currentSize));

        int multiplier = 1 << (m_size - currentSize);
        if (multiplier <= 1)
            return grid;

        Matrix<BiomeType> newGrid = new Matrix<BiomeType>(1 << m_size, 1 << m_size);
        for (int i = 0; i < grid.width; i++)
            for (int j = 0; j < grid.depth; j++)
            {
                var biome = grid.Get(i, j);
                
                for(int k = 0; k < multiplier; k++)
                    for (int l = 0; l < multiplier; l++)
                    {
                        newGrid.Set(i * multiplier + k, j * multiplier + l, biome);
                    }
            }

        return SmoothGrid(newGrid, m_settings.smoothSize);
    }

    Matrix<BiomeType> SmoothGrid(Matrix<BiomeType> grid, float radius)
    {
        if (radius < 1)
            return grid;

        Matrix<BiomeType> newGrid = new Matrix<BiomeType>(grid.width, grid.depth);

        List<float> weights = new List<float>();

        for(int i = 0; i < grid.width; i++)
        {
            for(int j = 0; j < grid.depth; j++)
            {
                GetBiomesAtRadius(grid, i, j, radius, weights);

                int best = 0;
                for(int k = 1; k < weights.Count; k++)
                {
                    if (weights[k] > weights[best])
                        best = k;
                }
                newGrid.Set(i, j, (BiomeType)best);
            }
        }
        return newGrid;
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