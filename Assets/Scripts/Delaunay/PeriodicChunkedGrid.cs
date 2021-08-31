using System.Collections.Generic;
using UnityEngine;

namespace NDelaunay
{
    public class PeriodicChunkedGrid
    {
        class LocalTriangle
        {
            public int triangleIndex;
            public int vertex1;
            public int vertex2;
            public int vertex3;
            public Vector2 pos1;
            public Vector2 pos2;
            public Vector2 pos3;

            public LocalTriangle(int _vertex1, int _vertex2, int _vertex3, Vector2 _pos1, Vector2 _pos2, Vector2 _pos3, int _triangleIndex)
            {
                vertex1 = _vertex1;
                vertex2 = _vertex2;
                vertex3 = _vertex3;
                pos1 = _pos1;
                pos2 = _pos2;
                pos3 = _pos3;
                triangleIndex = _triangleIndex;
            }
        }

        class ChunkInfo
        {
            //triangles are local on chunk and all touch the chunk at iteration 0
            public List<LocalTriangle> triangles = new List<LocalTriangle>();

            public int GetTriangle(Vector2 pos)
            {
                int nbTriangle = triangles.Count;
                for(int i = 0; i < nbTriangle; i++)
                {
                    if (Utility.IsOnTriangle(pos, triangles[i].pos1, triangles[i].pos2, triangles[i].pos3))
                        return i;
                }

                return -1;
            }
        }

        int m_totalSize;
        int m_chunkNb;
        int m_chunkSize;

        int m_triangleNB = 0;
        
        ChunkInfo[] m_chunks;

        public PeriodicChunkedGrid(int totalSize, int chunkSize)
        {
            Debug.Assert(totalSize % chunkSize == 0);

            m_chunkNb = totalSize / chunkSize;
            m_chunkSize = chunkSize;
            m_totalSize = m_chunkNb * m_chunkSize;

            m_chunks = new ChunkInfo[m_chunkNb * m_chunkNb];
            for (int i = 0; i < m_chunkNb * m_chunkNb; i++)
                m_chunks[i] = new ChunkInfo();
        }

        public int AddTriangle(Vector2 pos1, Vector2 pos2, Vector2 pos3, int vertex1, int vertex2, int vertex3)
        {
            float minX = pos1.x;
            float maxX = pos1.x;
            float minY = pos1.y;
            float maxY = pos1.y;

            if (pos2.x < minX) minX = pos2.x;
            if (pos2.x > maxX) maxX = pos2.x;
            if (pos2.y < minY) minY = pos2.y;
            if (pos2.y > maxY) maxY = pos2.y;

            if (pos3.x < minX) minX = pos3.x;
            if (pos3.x > maxX) maxX = pos3.x;
            if (pos3.y < minY) minY = pos3.y;
            if (pos3.y > maxY) maxY = pos3.y;

            int minChunkX, maxChunkX, minChunkY, maxChunkY;
            PosToUnclampedChunkPos(minX, minY, out minChunkX, out minChunkY);
            PosToUnclampedChunkPos(maxX, maxY, out maxChunkX, out maxChunkY);

            for(int i = minChunkX; i <= maxChunkX; i++)
            {
                for(int j = minChunkY; j <= maxChunkY; j++)
                {
                    int localChunkX = i;
                    int localChunkY = j;
                    int offsetX = 0;
                    int offsetY = 0;

                    while(localChunkX < 0)
                    {
                        localChunkX += m_chunkNb;
                        offsetX--;
                    }
                    while (localChunkX >= m_chunkNb)
                    {
                        localChunkX -= m_chunkNb;
                        offsetX++;
                    }
                    while (localChunkY < 0)
                    {
                        localChunkY += m_chunkNb;
                        offsetY--;
                    }
                    while (localChunkY >= m_chunkNb)
                    {
                        localChunkY -= m_chunkNb;
                        offsetY++;
                    }

                    Vector2 localPos1 = pos1 - new Vector2(offsetX * m_totalSize, offsetY * m_totalSize);
                    Vector2 localPos2 = pos2 - new Vector2(offsetX * m_totalSize, offsetY * m_totalSize);
                    Vector2 localPos3 = pos3 - new Vector2(offsetX * m_totalSize, offsetY * m_totalSize);

                    Vector2 chunkPos = new Vector2(localChunkX * m_chunkSize, localChunkY * m_chunkSize);
                    Vector2 chunkSize = new Vector2(m_chunkSize, m_chunkSize);
                    
                    if(Utility.TriangleRectangeCollision(chunkPos, chunkSize, localPos1, localPos2, localPos3))
                    {
                        var chunk = m_chunks[ChunkPosToIndex(localChunkX, localChunkY)];
                        chunk.triangles.Add(new LocalTriangle(vertex1, vertex2, vertex3, localPos1, localPos2, localPos3, m_triangleNB));
                    }
                }
            }

            m_triangleNB++;

            return m_triangleNB - 1;
        }

        public void GetTriangleVerticesAt(Vector2 pos, out int vertex1, out int vertex2, out int vertex3)
        {
            pos = ClampPos(pos);
            int chunkX, chunkY;
            PosToUnclampedChunkPos(pos.x, pos.y, out chunkX, out chunkY);

            var chunk = m_chunks[ChunkPosToIndex(chunkX, chunkY)];
            int triangle = chunk.GetTriangle(pos);
            if (triangle == -1)
            {
                Debug.Assert(false);
                vertex1 = -1;
                vertex2 = -1;
                vertex3 = -1;
                return;
            }

            var t = chunk.triangles[triangle];
            vertex1 = t.vertex1;
            vertex2 = t.vertex2;
            vertex3 = t.vertex3;
        }

        public void GetTriangleVerticesPosAt(Vector2 pos, out Vector2 pos1, out Vector2 pos2, out Vector2 pos3)
        {
            pos = ClampPos(pos);
            int chunkX, chunkY;
            PosToUnclampedChunkPos(pos.x, pos.y, out chunkX, out chunkY);

            var chunk = m_chunks[ChunkPosToIndex(chunkX, chunkY)];
            int triangle = chunk.GetTriangle(pos);
            if(triangle == -1)
            {
                Debug.Assert(false);
                pos1 = Vector2.zero;
                pos2 = Vector2.zero;
                pos3 = Vector2.zero;
                return;
            }

            var t = chunk.triangles[triangle];
            pos1 = t.pos1;
            pos2 = t.pos2;
            pos3 = t.pos3;
        }

        public void GetTriangleVerticesInfosAt(Vector2 pos, out Vector2 pos1, out int vertex1, out Vector2 pos2, out int vertex2, out Vector2 pos3, out int vertex3)
        {
            pos = ClampPos(pos);
            int chunkX, chunkY;
            PosToUnclampedChunkPos(pos.x, pos.y, out chunkX, out chunkY);

            var chunk = m_chunks[ChunkPosToIndex(chunkX, chunkY)];
            int triangle = chunk.GetTriangle(pos);
            if (triangle == -1)
            {
                Debug.Assert(false);
                vertex1 = -1;
                vertex2 = -1;
                vertex3 = -1;
                pos1 = Vector2.zero;
                pos2 = Vector2.zero;
                pos3 = Vector2.zero;
                return;
            }

            var t = chunk.triangles[triangle];
            vertex1 = t.vertex1;
            vertex2 = t.vertex2;
            vertex3 = t.vertex3;
            pos1 = t.pos1;
            pos2 = t.pos2;
            pos3 = t.pos3;
        }

        int ChunkPosToIndex(int x, int y)
        {
            return x * m_chunkNb + y;
        }

        void PosToUnclampedChunkPos(float x, float y, out int outX, out int outY)
        {
            PosToUnclampedChunkPos(Mathf.FloorToInt(x), Mathf.FloorToInt(y), out outX, out outY);
        }

        void PosToUnclampedChunkPos(int x, int y, out int outX, out int outY)
        {
            if (x < 0)
                outX = (x + 1) / m_chunkSize - 1;
            else outX = x / m_chunkSize;
            if (y < 0)
                outY = (y + 1) / m_chunkSize - 1;
            else outY = y / m_chunkSize;
        }

        void ClampChunkPos(int x, int y, out int outX, out int outY)
        {
            if (x < 0)
                outX = (x % m_chunkNb + m_chunkNb) % m_chunkNb;
            else outX = x % m_chunkNb;
            if (y < 0)
                outY = (y % m_chunkNb + m_chunkNb) % m_chunkNb;
            else outY = y % m_chunkNb;
        }

        public Vector2 ClampPos(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_totalSize + m_totalSize) % m_totalSize;
            else pos.x = pos.x % m_totalSize;
            if (pos.y < 0)
                pos.y = (pos.y % m_totalSize + m_totalSize) % m_totalSize;
            else pos.y = pos.y % m_totalSize;

            return pos;
        }

        public void Draw()
        {
            int index = Mathf.FloorToInt(Time.time / 0.5f) - 20;
            if (index < 0)
                index = 0;
            //index = 4;

            index %= m_chunkNb * m_chunkNb;

            int x = index % m_chunkNb;
            int y = index / m_chunkNb;
            index = ChunkPosToIndex(x, y);

            var c = m_chunks[index];

            float height = 5;

            //draw full size
            DebugDraw.Rectangle(new Vector3(0, height, 0), new Vector2(m_totalSize, m_totalSize), Color.magenta);

            //draw chunk size
            DebugDraw.Rectangle(new Vector3(x * m_chunkSize, height, y * m_chunkSize), new Vector2(m_chunkSize, m_chunkSize), Color.red);

            //draw triangles
            foreach(var t in c.triangles)
                DebugDraw.Triangle(new Vector3(t.pos1.x, height, t.pos1.y), new Vector3(t.pos2.x, height, t.pos2.y), new Vector3(t.pos3.x, height, t.pos3.y), Color.green);
        }
    }
}
