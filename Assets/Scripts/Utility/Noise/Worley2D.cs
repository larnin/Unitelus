using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Noise
{
    public class Worley2D
    {
        int m_size;
        int m_frequency;
        float m_amplitude;

        Vector2[] m_cellPoints;

        public Worley2D(int size, float amplitude, int frequency, Int32 seed)
        {
            m_size = size;
            m_amplitude = amplitude;
            m_frequency = frequency;

            GenerateCells(seed);
        }

        void GenerateCells(Int32 seed)
        {
            MT19937 rand = new MT19937((uint)seed);
            UniformVector2SquareDistribution d = new UniformVector2SquareDistribution();

            m_cellPoints = new Vector2[m_frequency * m_frequency];

            float cellSize = (float)(m_size) / m_frequency;

            for (int i = 0; i < m_frequency; i++)
                for (int j = 0; j < m_frequency; j++)
                {
                    d.SetParams(i * cellSize, (i + 1) * cellSize, j * cellSize, (j + 1) * cellSize);
                    m_cellPoints[PosToIndex(i, j)] = d.Next(rand);
                }
        }

        public float Get(float x, float y, float blendDist = 1.0f, Lerp.Operator lerp = Lerp.Operator.Linear)
        {
            var cell = CellAt(x, y);
            var item = m_cellPoints[PosToIndex(cell.x, cell.y)];

            float distance = float.MaxValue;
            Vector2 pos = new Vector2(x, y);

            for (int i = cell.x - 1; i <= cell.x + 1; i++)
            {
                for (int j = cell.y - 1; j <= cell.y + 1; j++)
                {
                    int posX = i;
                    int posY = j;

                    if (i < 0)
                        posX = m_frequency - 1;
                    else if (i >= m_frequency)
                        posX = 0;
                    if (j < 0)
                        posY = m_frequency - 1;
                    else if (j >= m_frequency)
                        posY = 0;

                    var cellPos = m_cellPoints[PosToIndex(posX, posY)];
                    if (i < 0)
                        cellPos.x -= m_size;
                    else if (i >= m_frequency)
                        cellPos.x += m_size;
                    if (j < 0)
                        cellPos.y -= m_size;
                    else if (j >= m_frequency)
                        cellPos.y += m_size;

                    float d = (pos - cellPos).sqrMagnitude;
                    if (d < distance)
                        distance = d;
                }
            }

            distance = Mathf.Sqrt(distance);

            //half diagonal of a cell
            float maxRadius = 1.41421356237f * (float)(m_size) / m_frequency / 2.0f;
            maxRadius *= blendDist;
            if (distance > maxRadius)
                return 0;
            distance /= maxRadius;

            return Lerp.LerpValue(m_amplitude, 0, distance, lerp);
        }

        Vector2Int CellAt(float x, float y)
        {
            if (x < 0)
                x = (x % m_size + m_size) % m_size;
            else x = x % m_size;
            if (y < 0)
                y = (y % m_size + m_size) % m_size;
            else y = y % m_size;
            //here x & y on bounds

            int iX = Mathf.FloorToInt(x / m_size * m_frequency);
            int iY = Mathf.FloorToInt(y / m_size * m_frequency);

            Debug.Assert(iX >= 0 && iX < m_frequency && iY >= 0 && iY < m_frequency);

            return new Vector2Int(iX, iY);
        }

        int PosToIndex(int x, int y)
        {
            return x * m_frequency + y;
        }
    }
}