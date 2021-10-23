using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum BiomeType
{
    Invalid,

    Plain,
    Ocean,
    Desert,
    Snow,
    Mountain,
}

[Serializable]
public class BiomeInfo
{
    public BiomeType biomeType;
    public float themperature;
    public float moisture;
    public float weight;
    public float size;

    public BiomeInfo(BiomeType _type, float _themperature, float _moisture, float _weight, float _size)
    {
        biomeType = _type;
        themperature = _themperature;
        moisture = _moisture;
        weight = _weight;
        size = _size;
    }
}
