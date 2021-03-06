﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

    WorldGeneratorSettings m_settings = null;

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
                Debug.Assert(false);
                return;
            }

            m_state = State.generating;
        }

        statusText = "Generating ...";

        Debug.Assert(m_thread == null);

        m_settings = settings;

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
        statusText = "Generating surface ...";

        m_world = new World(m_settings.size, true);

        List<Perlin> perlins = new List<Perlin>();
        foreach (var p in m_settings.perlins)
            perlins.Add(new Perlin(world.size, p.amplitude, p.frequency, m_settings.seed + perlins.Count));

        BlockData b;
        b.id = 1;
        b.data = 0;

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
        }

        statusText = "Generating ground ...";

        for (int x = 0; x < m_settings.size * Chunk.chunkSize; x++)
        {
            for (int z = 0; z < m_settings.size * Chunk.chunkSize; z++)
            {
                int height = world.GetTopBlockHeight(x, z);

                if (height <= minHeight)
                    continue;
                for (int y = height - 1; y >= minHeight; y--)
                    world.SetBlock(x, y, z, b, false);
            }
        }

        //world.SetBlock(0, 10, -1, b, false);
        //world.SetBlock(1, 10, -1, b, false);
        //world.SetBlock(0, 10, 0, b, false);
        //world.SetBlock(1, 10, 0, b, false);
        //world.SetBlock(0, 10, 1, b, false);
        //world.SetBlock(1, 10, 1, b, false);


        //world.SetBlock(4, 10, 0, b, false);
        //world.SetBlock(4, 10, 1, b, false);
        //world.SetBlock(5, 10, 0, b, false);
        //world.SetBlock(5, 10, 1, b, false);
        //world.SetBlock(6, 10, 0, b, false);
        //world.SetBlock(6, 10, 1, b, false);

        //world.SetBlock(16, 7, 0, b, false);
        //world.SetBlock(15, 7, 0, b, false);

        statusText = "Updating blocks state ...";

        UpdateWorldData(world);

        statusText = "Done";
        m_thread = null;

        state = State.finished;
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
                                view.Set(0, 0, 0, BlockTypeList.instance.Get(view.GetCenter().id).UpdateBlock(view));
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
}
