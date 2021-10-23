using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WorldGeneratorSettings : ScriptableObject
{
    public MainGeneratorSettings main;

    public BiomesSettings biomes;

    public PlainBiomeSettings plain;
    public OceanBiomeSettings ocean;
    public DesertBiomeSettings desert;
    public SnowBiomeSettings snow;
    public MountainBiomeSettings mountain;
}

[Serializable]
public class MainGeneratorSettings
{
    [HideInInspector]
    public int seed = 0;
    public int size = 1;

    public List<WorldGeneratorSettingPerlin> base2DPerlin = new List<WorldGeneratorSettingPerlin>();
    public List<WorldGeneratorSettingPerlin> base3DPerlin = new List<WorldGeneratorSettingPerlin>();
}

[Serializable]
public class OneBiomeSettings
{
    public BiomeType biome;
    public float weight;
    public Bounds bounds;
}

[Serializable]
public class BiomesSettings
{
    public BiomeType defaultBiome;
    public List<OneBiomeSettings> biomes;
    public float smoothSize;
    public float borderSize;

    public List<WorldGeneratorSettingPerlin> noiseX = new List<WorldGeneratorSettingPerlin>();
    public List<WorldGeneratorSettingPerlin> noiseY = new List<WorldGeneratorSettingPerlin>();
    public List<WorldGeneratorSettingPerlin> noiseZ = new List<WorldGeneratorSettingPerlin>();
}

[Serializable]
public class PlainBiomeSettings
{

}

[Serializable]
public class OceanBiomeSettings
{
    public int waterLevel;
}

[Serializable]
public class DesertBiomeSettings
{
    
}

[Serializable]
public class SnowBiomeSettings
{

}

[Serializable]
public class MountainBiomeSettings
{

}

[Serializable]
public class WorldGeneratorSettingPerlin
{
    public float amplitude = 1;
    public int frequency = 1;
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