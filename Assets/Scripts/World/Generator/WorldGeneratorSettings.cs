using Sirenix.OdinInspector;
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
    [InfoBox("@\"Real world Size \" + GetWorldSize()")]
    public int size = 1;

    public int GetChunkNb()
    {
        return 1 << size;
    }

    public int GetWorldSize()
    {
        return GetChunkNb() * Chunk.chunkSize;
    }

    public PerlinSettings perlin2D;
    public PerlinSettings perlin3D;
}

[Serializable]
public class PerlinSettings
{
    public int baseFrequency = 1;
    public float baseAmplitude = 1;
    public int nbLayers = 1;
    public float layerAmplitudeScale = 0.5f;
}

[Serializable]
public class OneBiomeSettings
{
    public BiomeType biome;
    public float weight;
    public float temperature;
    public float humidity;
}

[Serializable]
public class OneSubBiomeSettings
{
    public BiomeType biome;
    public BiomeType baseBiome;
    public float weight;
    public int size;
}

[Serializable]
public class BiomesSettings
{
    public BiomeType defaultBiome;
    
    public List<OneBiomeSettings> initialBiomes;
    public List<OneSubBiomeSettings> subBiomes;
    public int smoothSize;
    public int borderSize;
    public int biomeSize;
    public float randomizeWeight;

    public float packingProbabiity;
}

[Serializable]
public class PlainBiomeSettings
{
    public int baseLevel;
    public int amplitude2D;
    public int amplitude3D;
}

[Serializable]
public class OceanBiomeSettings
{
    public int waterLevel;
    public int groundLevel;
    public int groundAmplitude;
}

[Serializable]
public class DesertBiomeSettings
{
    public int amplitude2D;
    public int amplitude3D;
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