using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class WorldGeneratorSettingPerlin
{
    public float amplitude = 1;
    public int frequency = 1;
}

[Serializable]
public class WorldGeneratorSettings
{
    public int seed = 0;
    public int size = 1;

    public List<WorldGeneratorSettingPerlin> perlins = new List<WorldGeneratorSettingPerlin>();

    public VoronoiBiomesSettings m_biomes;
}

[Serializable]
public class VoronoiBiomesSettings
{
    public int seed = 0;

    public int nbBiome = 10;

    public int relaxingPower = 1;
    public float relaxingForce = 1;

    public int magnetPower = 1;
    public float magnetForce = 1;

    public List<BiomeType> validBiomes = new List<BiomeType>();
}