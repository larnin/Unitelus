using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class BiomeGenerator
{
    public class BiomeDistance
    {
        public BiomeType biome;
        public float distance;

        public BiomeDistance(BiomeType _biome, float _distance = 0)
        {
            biome = _biome;
            distance = _distance;
        }
    }

    class BorderData
    {
        public BiomeType current;
        public BiomeType right;
        public BiomeType down;
        public BorderData(BiomeType _current, BiomeType _right, BiomeType _down)
        {
            current = _current;
            right = _right;
            down = _down;
        }
    }

    int m_seed;
    int m_size;
    BiomesSettings m_settings;

    MT19937 m_rand;

    Matrix<BiomeType> m_grid;
    QuadTreeInt<BorderData> m_borders;

    public int GetSize() { return 1 << m_size; }

    public void Generate(BiomesSettings settings, int size, int seed)
    {
        m_seed = seed;
        m_rand = new MT19937((uint)seed);
        m_size = size;
        m_settings = settings;

        int gridSize = m_size - m_settings.biomeSize;
        if (gridSize < 1)
            gridSize = 1;

        var grid = GenerateFirstGrid(gridSize);

        while(gridSize < m_size)
        {
            gridSize++;
            grid = IncreaseGridSize(grid, gridSize);
            RandomizePoints(grid);
        }

        m_grid = GenerateFinalGrid(grid, gridSize);

        DetectBiomesBorders(m_grid);
    }

    Matrix<BiomeType> GenerateFirstGrid(int gridSize)
    {

        gridSize = 1 << gridSize;

        Matrix<BiomeType> grid = new Matrix<BiomeType>(gridSize, gridSize);
        grid.SetAll(m_settings.defaultBiome);
        
        int nbCell = gridSize * gridSize;
        float totalWeight = 0;
        List<BiomeType> biomes = new List<BiomeType>();
        foreach (var b in m_settings.initialBiomes)
        {
            int nb = Mathf.RoundToInt(nbCell * b.weight / 100);
            for(int i = 0; i < nb; i++)
                biomes.Add(b.biome);
        }
        biomes.Shuffle(m_rand);
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
            if(dPack.Next(m_rand) && packedPos.Count > 0)
            {
                dIndex.SetParams(packedPos.Count);
                int index = dIndex.Next(m_rand);
                pos = packedPos[index];
                packedPos.RemoveAt(index);
                validPos.Remove(pos);
            }
            else
            {
                dIndex.SetParams(validPos.Count);
                int index = dIndex.Next(m_rand);
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
        Assert.IsTrue(grid.width * 2 == (1 << gridSize));

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
            else validPos.Shuffle(m_rand);

            for (int i = 0; i < nb; i++)
                newGrid.Set(validPos[i].x, validPos[i].y, subBiome.biome);
        }

        return newGrid;
    }

    void RandomizePoints(Matrix<BiomeType> grid)
    {
        for(int i = 0; i < grid.width / 2; i++)
            for(int j = 0; j < grid.depth / 2; j++)
                RandomizePoint(grid, i * 2 + 1, j * 2 + 1, m_rand);
    }

    BernoulliDistribution dRandPoint = new BernoulliDistribution();
    BernoulliDistribution dRandPointDir = new BernoulliDistribution();

    void RandomizePoint(Matrix<BiomeType> grid, int x, int y, IRandomGenerator rand)
    {
        Vector2Int[] pos = new Vector2Int[4] { new Vector2Int(x, y), new Vector2Int(x + 1, y), new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1) };
        for(int i = 1; i < pos.Length; i++)
        {
            if (pos[i].x >= grid.width)
                pos[i].x = 0;
            if (pos[i].y >= grid.depth)
                pos[i].y = 0;
        }
        var originalPos = pos.ToArray(); //copy
        pos.Shuffle(rand);
        BiomeType[] biomes = new BiomeType[4] { grid.Get(pos[0].x, pos[0].y), grid.Get(pos[1].x, pos[1].y), grid.Get(pos[2].x, pos[2].y), grid.Get(pos[3].x, pos[3].y) };
        dRandPoint.SetParams(m_settings.randomizeWeight);

        for(int i = 0; i < pos.Length; i++)
        {
            if (!dRandPoint.Next(rand))
                continue;
            Vector2Int p1, p2;
            var p = pos[i];
            if((p.x == x && p.y == y) || (p.x != x && p.y != y))
            {
                p1 = originalPos[1];
                p2 = originalPos[2];
            }
            else
            {
                p1 = originalPos[0];
                p2 = originalPos[3];
            }

            bool dir = dRandPointDir.Next(rand);
            var pCopy = dir ? p1 : p2;

            var newBiome = grid.Get(pCopy.x, pCopy.y);
            if (biomes[i] == newBiome)
                continue;

            int index = -1;
            for(int j = 0; j < pos.Length; j++)
            {
                if(pos[j] == pCopy)
                {
                    index = j;
                    break;
                }
            }

            if (index < 0)
                continue;
            if (biomes[index] != newBiome)
                continue;

            biomes[i] = newBiome;
        }

        for(int i = 0; i < pos.Length; i++)
            grid.Set(pos[i].x, pos[i].y, biomes[i]);
    }

    Matrix<BiomeType> GenerateFinalGrid(Matrix<BiomeType> grid, int currentSize)
    {
        Assert.IsTrue(grid.width == (1 << currentSize));

        int multiplier = 1 << (m_size - currentSize);
        if (multiplier <= 1)
            return SmoothGrid(grid, m_settings.smoothSize);

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
        
        for(int i = 0; i < grid.width; i++)
        {
            for(int j = 0; j < grid.depth; j++)
            {
                BiomeType best = GetBestBiomeAtRadius(grid, i, j, radius);

                newGrid.Set(i, j, best);
            }
        }
        return newGrid;
    }

    void DetectBiomesBorders(Matrix<BiomeType> grid)
    {
        QuadTreeInt<BorderData> borders = new QuadTreeInt<BorderData>(grid.width, grid.depth, 16);
        for(int i = 0; i < grid.width; i++)
        {
            int nextI = i + 1;
            if (nextI >= grid.width)
                nextI = 0;
            for(int j = 0; j < grid.depth; j++)
            {
                int nextJ = j + 1;
                if (nextJ >= grid.depth)
                    nextJ = 0;
                BiomeType current = grid.Get(i, j);
                BiomeType right = grid.Get(nextI, j);
                BiomeType down = grid.Get(i, nextJ);

                if (current == right && current == down)
                    continue;

                borders.AddElement(i, j, new BorderData(current, right, down));
            }
        }
        m_borders = borders;
    }

    float[] biomesWeights = new float[Enum.GetValues(typeof(BiomeType)).Length];

    BiomeType GetBestBiomeAtRadius(Matrix<BiomeType> grid, int x, int y, float radius)
    {
        for (int i = 0; i < biomesWeights.Length; i++)
            biomesWeights[i] = 0;

        int iRadius = Mathf.CeilToInt(radius);

        for (int k = -iRadius; k <= iRadius; k++)
        {
            for (int l = -iRadius; l <= iRadius; l++)
            {
                float normalizedDist = (k * k + l * l) / (radius * radius);
                if (normalizedDist > 1)
                    continue;

                float weight = Lerp.Linear(1, 0, normalizedDist);

                int pX = x + k;
                if (pX < 0) pX += grid.width;
                else if (pX >= grid.width) pX -= grid.width;
                int pY = y + l;
                if (pY < 0) pY += grid.depth;
                else if (pY >= grid.depth) pY -= grid.depth;

                var biome = grid.Get(pX, pY);
                biomesWeights[(int)biome] += weight;
            }
        }

        int maxIndex = 0;
        for (int i = 1; i < biomesWeights.Length; i++)
            if (biomesWeights[i] > biomesWeights[maxIndex])
                maxIndex = i;

        return (BiomeType)maxIndex;
    }
    
    public List<BiomeDistance> GetBiomeDistances(int x, int y, float radius)
    {
        List<BiomeDistance> biomes = new List<BiomeDistance>();
        GetBiomesDistanceNoAlloc(x, y, radius, biomes);
        return biomes;
    }

    static List<QuadTreeInt<BorderData>> m_regionsTemp = new List<QuadTreeInt<BorderData>>();
    static float[] m_biomesDistance = new float[Enum.GetValues(typeof(BiomeType)).Length];

    public void GetBiomesDistanceNoAlloc(int x, int y, float radius, List<BiomeDistance> biomes)
    {
        float offset = -0.5f;
        Vector2 pos = new Vector2(x + offset, y + offset);
        m_borders.GetRegionsInCircleNoAlloc(pos.x, pos.y, radius, m_regionsTemp);

        for (int i = 0; i < m_biomesDistance.Length; i++)
            m_biomesDistance[i] = -1;

        BiomeType current = m_grid.Get(x, y);

        foreach(var r in m_regionsTemp)
        {
            int nbBlock = r.GetNbLocalElement();

            for(int i = 0; i < nbBlock; i++)
            {
                var elemPos = r.GetLocalElementPosition(i);
                float dist = (pos - elemPos).magnitude;
                if (dist > radius)
                    continue;
                var elem = r.GetLocalElement(i);
                if(elem.current != elem.right && (current == elem.current || current == elem.right))
                {
                    int other = (int)(elem.current == current ? elem.right : elem.current);
                    if (m_biomesDistance[i] < 0 || m_biomesDistance[i] > dist)
                        m_biomesDistance[i] = dist;
                }
                if(elem.current != elem.down && (current == elem.current || current == elem.down))
                {
                    int other = (int)(elem.current == current ? elem.down : elem.current);
                    if (m_biomesDistance[i] < 0 || m_biomesDistance[i] > dist)
                        m_biomesDistance[i] = dist;
                }
            }
        }

        biomes.Clear();

        for(int i = 0; i < m_biomesDistance.Length; i++)
        {
            if (m_biomesDistance[i] >= 0)
                biomes.Add(new BiomeDistance((BiomeType)i, m_biomesDistance[i]));
        }
    }

    public BiomeType GetBiome(int x, int y)
    {
        Assert.IsTrue(x >= 0 && x < m_grid.width);
        Assert.IsTrue(y >= 0 && y < m_grid.depth);

        return m_grid.Get(x, y);
    }
}