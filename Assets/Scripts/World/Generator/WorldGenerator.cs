﻿using Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class WorldGenerator
{
    public enum State
    {
        idle,
        generating,
        finished,
        error,
    }

    readonly object m_stateLock = new object();
    State m_state = State.idle;

    readonly object m_statusLock = new object();
    string m_status = "Idle";

    readonly object m_worldLock = new object();
    World m_world = null;

    Thread m_thread;
    float m_time;

    WorldGeneratorSettings m_settings = null;

    BiomeGenerator m_biomes = null;

    public State state
    {
        get
        {
            lock (m_stateLock)
                return m_state;
        }
        private set
        {
            lock (m_stateLock)
                m_state = value;
        }
    }

    public string statusText
    {
        get
        {
            lock (m_statusLock)
                return m_status;
        }
        private set
        {
            lock (m_statusLock)
                m_status = value;
        }
    }

    public World world
    {
        get
        {
            lock (m_worldLock)
                return m_world;
        }
        private set
        {
            lock (m_worldLock)
                m_world = value;
        }
    }

    public void Generate(WorldGeneratorSettings settings)
    {
        lock(m_stateLock)
        {
            if(m_state == State.generating)
            {
                Assert.IsTrue(false);
                return;
            }

            m_state = State.generating;
        }

        statusText = "Generating ...";

        Assert.IsTrue(m_thread == null);

        m_settings = settings;

        m_time = Time.time;
        m_thread = new Thread(new ThreadStart(Process));
        m_thread.Start();
    }

    public void Stop()
    {
        if(m_thread != null)
        {
            m_thread.Abort();
            m_thread = null;
            state = State.error;
            statusText = "Aborted";
        }
    }

    void Process()
    {
        TimeEx.SetFixedTime(m_time);

        DebugTimer timer = new DebugTimer();
        timer.Start();

        statusText = "Generating biomes ...";

        m_biomes = new BiomeGenerator();
        m_biomes.Generate(m_settings.biomes, m_settings.main.size + Chunk.chunkScale, m_settings.main.seed + 1);

        world = new World(m_settings.main.GetChunkNb(), true);
        SetWorldBiome(world, m_biomes);

        timer.LogAndRestart(statusText);
        statusText = "Generating surface ...";
        
        int minHeight = 2;

        int worldSize = m_settings.main.GetWorldSize();

        Cliff cliff = new Cliff(worldSize, 25, 10, 1, m_settings.main.seed + 2);

        BlockData b;
        b.id = BlockID.INVALID;
        b.data = 0;

        for (int x = 0; x < worldSize; x++)
        {
            for(int z = 0; z < worldSize; z++)
            {
                int height = Mathf.FloorToInt(cliff.GetHeight(new Vector2(x, z)) * 50);

                if (height < minHeight)
                    minHeight = height;

                b.id = BlockID.GRASS;
                world.SetBlock(x, height, z, b, false);

                b.id = BlockID.DIRT;
                for (int i = 1; i <= 2; i++)
                    world.SetBlock(x, height - i, z, b, false);
            }
        }

        minHeight-= 4;
        b.id = BlockID.STONE;

        //BlockData b;
        //b.id = BlockID.INVALID;
        //b.data = 0;

        //int worldSize = m_settings.main.GetWorldSize();

        //for (int x = 0; x < worldSize; x++)
        //{
        //    for (int z = 0; z < worldSize; z++)
        //    {
        //        var biome = m_biomes.GetBiome(x, z);
        //        if (biome == BiomeType.Invalid)
        //            b.id = BlockID.INVALID;
        //        else if (biome == BiomeType.Desert)
        //            b.id = BlockID.SAND;
        //        else if (biome == BiomeType.Mountain)
        //            b.id = BlockID.STONE;
        //        else if (biome == BiomeType.Ocean)
        //            b.id = BlockID.WATER;
        //        else if (biome == BiomeType.Plain)
        //            b.id = BlockID.GRASS;
        //        else if (biome == BiomeType.Snow)
        //            b.id = BlockID.SNOW;
        //        else b.id = BlockID.INVALID;

        //        world.SetBlock(x, minHeight, z, b, false);
        //    }
        //}
        //minHeight--;


        /*List<Perlin> perlins = new List<Perlin>();
        foreach (var p in m_settings.perlins)
            perlins.Add(new Perlin(world.size, p.amplitude, p.frequency, m_settings.seed + perlins.Count));

        BlockData b;
        b.id = BlockID.INVALID;
        b.data = 0;

        int minHeight = 2;


        for (int x = 0; x < m_settings.size * Chunk.chunkSize; x++)
        {
            for (int z = 0; z < m_settings.size * Chunk.chunkSize; z++)
            {
                var biome = m_biomes.GetNearestBiome(new Vector2(x, z));
                b.id = (BlockID)biome;

                world.SetBlock(x, minHeight, z, b, false);
            }
        }
        minHeight--;*/

        /*
        int minHeight = int.MaxValue;

        for (int x = 0; x < m_settings.size * Chunk.chunkSize; x++)
        {
            for (int z = 0; z < m_settings.size * Chunk.chunkSize; z++)
            {
                float y = 0;
                foreach (var p in perlins)
                    y += p.Get(x, z);

                int yInt = Mathf.FloorToInt(y);
                world.SetBlock(x, yInt, z, b, false);

                minHeight = Mathf.Min(yInt - 1, minHeight);
            }
        }*/

        timer.LogAndRestart(statusText);
        statusText = "Generating ground ...";

        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                int height = world.GetBottomBlockHeight(x, z) - 1;

                if (height <= minHeight)
                    continue;

                for (int y = height; y >= minHeight; y--)
                    world.SetBlock(x, y, z, b, false);
            }
        }

        timer.LogAndRestart(statusText);
        statusText = "Updating blocks state ...";

        UpdateWorldData(world);

        timer.LogAndRestart(statusText);
        statusText = "Done";
        m_thread = null;

        state = State.finished;

        TimeEx.SetTimeDynamic();
    }

    static void UpdateWorldData(World world)
    {
        Matrix<BlockData> mat = new Matrix<BlockData>(Chunk.chunkSize + 2, Chunk.chunkSize + 2, Chunk.chunkSize + 2);
        MatrixView<BlockData> view = new MatrixView<BlockData>(mat, 0, 0, 0);

        for(int i = 0; i < world.chunkNb; i++)
        {
            for(int j = 0; j < world.chunkNb; j++)
            {
                var chunk = world.GetChunk(i, j);
                var layers = chunk.GetLayers();

                foreach (var l in layers)
                {
                    world.GetLocalMatrix(i * Chunk.chunkSize - 1, l * Chunk.chunkSize - 1, j * Chunk.chunkSize - 1, mat);

                    for (int x = 0; x < Chunk.chunkSize; x++)
                    {
                        for (int y = 0; y < Chunk.chunkSize; y++)
                        {
                            for (int z = 0; z < Chunk.chunkSize; z++)
                            {
                                view.SetPos(x + 1, y + 1, z + 1);
                                view.Set(0, 0, 0, G.sys.blocks.Get(view.GetCenter().id).UpdateBlock(view));
                            }
                        }
                    }

                    for (int y = 0; y < Chunk.chunkSize; y++)
                    {
                        int height = chunk.LayerToHeight(l, y);
                        for (int x = 0; x < Chunk.chunkSize; x++)
                        {
                            for (int z = 0; z < Chunk.chunkSize; z++)
                            {
                                chunk.SetBlock(x, height, z, mat.Get(x + 1, y + 1, z + 1));
                            }
                        }
                    }
                }
            }
        }
    }

    static void SetWorldBiome(World world, BiomeGenerator biomes)
    {
        Assert.IsTrue(world.size == biomes.GetSize());
        int size = world.size;
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                world.SetBiome(i, j, biomes.GetBiome(i, j));
            }
        }
    }
}
