using NDelaunay;
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
    public int maxGroupSize;

    PeriodicDelaunay m_delaunay;

    UnstructuredPeriodicGrid m_grid;

    private void Start()
    {
        m_delaunay = new PeriodicDelaunay(gridSize, nbCell * nbCell);

        float cellSize = gridSize / (float)(nbCell);

        MT19937 rand = new MT19937((uint)seed);
        UniformVector2SquareDistribution d = new UniformVector2SquareDistribution();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        int nbPoint = nbCell * nbCell;
        List<Vector2> points = new List<Vector2>(nbPoint);

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

                points.Add(pos);
            }
        }

        Logs.ImportantAdd("Generated points " + stopWatch.Elapsed.TotalSeconds + " s");
        points.Shuffle(rand);
        Logs.ImportantAdd("Shuffled points " + stopWatch.Elapsed.TotalSeconds + " s");

        for (int i = 0; i < points.Count; i++)
            m_delaunay.Add(points[i]);

        Logs.ImportantAdd("Delaunay " + stopWatch.Elapsed.TotalSeconds + " s");

        m_grid = PeriodicGraph.MakeGraph(m_delaunay.GetGrid(), maxGroupSize);

        Logs.ImportantAdd("Graph " + stopWatch.Elapsed.TotalSeconds + " s");

        stopWatch.Stop();

        Logs.Dump();
    }

    private void Update()
    {
        //m_delaunay.Draw();
        //m_graph.Draw();
        m_delaunay.GetGrid().Draw(true, false);
        m_grid.Draw(false, true);
    }

}
