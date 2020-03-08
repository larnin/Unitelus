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
}