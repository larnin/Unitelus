using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class WorldGenerator
{
    public static World Generate(WorldGeneratorSettings settings)
    {
        List<Perlin> perlins = new List<Perlin>();
        foreach (var p in settings.perlins)
            perlins.Add(new Perlin(settings.size, p.amplitude, p.frequency, settings.seed + perlins.Count));

        World world = new World(settings.size, true);

        BlockData b;
        b.id = 1;

        int minHeight = int.MaxValue;

        for (int x = 0; x < settings.size * Chunk.chunkSize; x++)
        {
            for (int y = 0; y < settings.size * Chunk.chunkSize; y++)
            {
                float z = 0;
                foreach (var p in perlins)
                    z += p.Get(x, y);

                int zInt = Mathf.FloorToInt(z);
                world.SetBlock(x, y, zInt, b);

                minHeight = Mathf.Min(zInt - 1, minHeight);
            }
        }

        for (int x = 0; x < settings.size * Chunk.chunkSize; x++)
        {
            for (int y = 0; y < settings.size * Chunk.chunkSize; y++)
            {
                int height = world.GetHeight(x, y);

                if (height <= minHeight)
                    continue;
                for(int z = height - 1; z >= minHeight; z--)
                    world.SetBlock(x, y, z, b);
            }
        }

        return world;
    }
}
