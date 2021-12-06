using NDelaunay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

class TestVoronoi : MonoBehaviour
{
    public int gridSize;
    public int nbCell;
    public int seed;

    PeriodicDelaunayV2 m_delaunay;

    private void Start()
    {
        m_delaunay = new PeriodicDelaunayV2(gridSize);

        float cellSize = gridSize / (float)(nbCell);

        MT19937 rand = new MT19937((uint)seed);
        UniformVector2SquareDistribution d = new UniformVector2SquareDistribution();

        for(int i = 0; i < nbCell; i++)
        {
            for(int j = 0; j < nbCell; j++)
            {
                d.SetParams(i * cellSize, (i + 1) * cellSize, j * cellSize, (j + 1) * cellSize);
                var pos = d.Next(rand);

                m_delaunay.Add(pos);
            }
        }
    }

    private void Update()
    {
        m_delaunay.Draw();
    }

}
