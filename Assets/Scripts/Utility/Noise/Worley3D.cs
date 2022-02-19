using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Noise
{
    public class Worley3D
    {
        int m_size;
        int m_frequency;
        float m_amplitude;

        Vector3[] m_cellPoints;

        public Worley3D(int size, float amplitude, int frequency, Int32 seed)
        {
            m_size = size;
            m_amplitude = amplitude;
            m_frequency = frequency;

            GenerateCells(seed);
        }

        void GenerateCells(Int32 seed)
        {
            MT19937 rand = new MT19937((uint)seed);
            UniformVector3BoxDistribution d = new UniformVector3BoxDistribution();

            m_cellPoints = new Vector3[m_frequency * m_frequency * m_frequency];

            float cellSize = (float)(m_size) / m_frequency;

            for (int i = 0; i < m_frequency; i++)
                for (int j = 0; j < m_frequency; j++)
                    for (int k = 0; k < m_frequency; k++)
                    {
                        d.SetParams(i * cellSize, (i + 1) * cellSize, j * cellSize, (j + 1) * cellSize, k * cellSize, (k + 1) * cellSize);
                        m_cellPoints[PosToIndex(i, j, k)] = d.Next(rand);
                    }
        }

        public float Get(float x, float y, float z, float blendDist = 1.0f, Lerp.Operator lerp = Lerp.Operator.Linear)
        {
            var cell = CellAt(x, y, z);
            var item = m_cellPoints[PosToIndex(cell.x, cell.y, cell.z)];

            float distance = float.MaxValue;
            Vector3 pos = new Vector3(x, y, z);

            for (int i = cell.x - 1; i <= cell.x + 1; i++)
            {
                for (int j = cell.y - 1; j <= cell.y + 1; j++)
                {
                    for (int k = cell.z - 1; k <= cell.z + 1; k++)
                    {
                        int posX = i;
                        int posY = j;
                        int posZ = k;

                        if (i < 0)
                            posX = m_frequency - 1;
                        else if (i >= m_frequency)
                            posX = 0;
                        if (j < 0)
                            posY = m_frequency - 1;
                        else if (j >= m_frequency)
                            posY = 0;
                        if (k < 0)
                            posZ = m_frequency - 1;
                        else if (k >= m_frequency)
                            posZ = 0;

                        var cellPos = m_cellPoints[PosToIndex(posX, posY, posZ)];
                        if (i < 0)
                            cellPos.x -= m_size;
                        else if (i >= m_frequency)
                            cellPos.x += m_size;
                        if (j < 0)
                            cellPos.y -= m_size;
                        else if (j >= m_frequency)
                            cellPos.y += m_size;
                        if (k < 0)
                            cellPos.z -= m_size;
                        else if (k >= m_frequency)
                            cellPos.z += m_size;

                        float d = (pos - cellPos).sqrMagnitude;
                        if (d < distance)
                            distance = d;
                    }
                }
            }

            distance = Mathf.Sqrt(distance);

            //half diagonal of a cell
            float maxRadius = 1.73205080757f * (float)(m_size) / m_frequency / 2.0f;
            maxRadius *= blendDist;
            if (distance > maxRadius)
                return 0;
            distance /= maxRadius;

            return Lerp.LerpValue(m_amplitude, 0, distance, lerp);
        }

        Vector3Int CellAt(float x, float y, float z)
        {
            if (x < 0)
                x = (x % m_size + m_size) % m_size;
            else x = x % m_size;
            if (y < 0)
                y = (y % m_size + m_size) % m_size;
            else y = y % m_size;
            if (z < 0)
                z = (z % m_size + m_size) % m_size;
            else z = z % m_size;
            //here x & y & z on bounds

            int iX = Mathf.FloorToInt(x / m_size * m_frequency);
            int iY = Mathf.FloorToInt(y / m_size * m_frequency);
            int iZ = Mathf.FloorToInt(z / m_size * m_frequency);

            Assert.IsTrue(iX >= 0 && iX < m_frequency && iY >= 0 && iY < m_frequency && iZ >= 0 && iZ < m_frequency);

            return new Vector3Int(iX, iY, iZ);
        }

        int PosToIndex(int x, int y, int z)
        {
            return (x * m_frequency + y) * m_frequency + z;
        }
    }
}