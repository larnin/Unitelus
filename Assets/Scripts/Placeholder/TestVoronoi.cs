﻿using NDelaunay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;
using System.Diagnostics;

class TestVoronoi : MonoBehaviour
{
    public int gridSize;
    public int nbCell;
    public int seed;
    public int breakCount;

    PeriodicDelaunay m_delaunay;

    private void Start()
    {
        m_delaunay = new PeriodicDelaunay(gridSize, nbCell * nbCell);

        float cellSize = gridSize / (float)(nbCell);

        MT19937 rand = new MT19937((uint)seed);
        UniformVector2SquareDistribution d = new UniformVector2SquareDistribution();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        int count = 0;
        for(int i = 0; i < nbCell; i++)
        {
            for(int j = 0; j < nbCell; j++)
            {
                count++;
                if (count > breakCount)
                    break;

                d.SetParams(i * cellSize, (i + 1) * cellSize, j * cellSize, (j + 1) * cellSize);
                var pos = d.Next(rand);

                m_delaunay.Add(pos);

                Logs.ImportantAdd("Point " + count + " T " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");
            }
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        Logs.ImportantAdd("Time " + ts.TotalSeconds + " s");

        Logs.Dump();
    }

    private void Update()
    {
        m_delaunay.Draw();
    }

}
