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
        World world = new World(settings.size, true);

        List<Perlin> perlins = new List<Perlin>();
        foreach (var p in settings.perlins)
            perlins.Add(new Perlin(world.size, p.amplitude, p.frequency, settings.seed + perlins.Count));

        BlockData b;
        b.id = 1;

        int minHeight = int.MaxValue;

        for (int x = 0; x < settings.size * Chunk.chunkSize; x++)
        {
            for (int z = 0; z < settings.size * Chunk.chunkSize; z++)
            {
                float y = 0;
                foreach (var p in perlins)
                    y += p.Get(x, z);

                int yInt = Mathf.FloorToInt(y);
                world.SetBlock(x, yInt, z, b);

                minHeight = Mathf.Min(yInt - 1, minHeight);
            }
        }

        for (int x = 0; x < settings.size * Chunk.chunkSize; x++)
        {
            for (int z = 0; z < settings.size * Chunk.chunkSize; z++)
            {
                int height = world.GetTopBlockHeight(x, z);

                if (height <= minHeight)
                    continue;
                for(int y = height - 1; y >= minHeight; y--)
                    world.SetBlock(x, y, z, b);
            }
        }

        world.SetBlock(0, 10, 0, b);

        return world;
    }
}
