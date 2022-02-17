using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathSettings
{
    public int maxDistanceFromPath = 5;

    public int agentHeight = 2;
    public int agetWidth = 1;
}

public class Path
{
    World m_world;
    PathSettings m_settings;

    Vector3Int m_start;
    Vector3Int m_end;

    List<Vector3Int> m_points = new List<Vector3Int>();
    int m_currentPoint;

    public Path(PathSettings settings)
    {
        m_settings = settings;
        m_currentPoint = 0;
    }

    public bool IsPathValid()
    {
        return m_points.Count > 0;
    }

    public Vector3Int GetCurrentPoint()
    {
        if (m_currentPoint < 0 || m_currentPoint >= m_points.Count)
            return Vector3Int.zero;

        return m_points[m_currentPoint];
    }

    public bool Generate(Vector3Int start, Vector3Int end)
    {
        Clear();

        m_start = start;
        m_end = end;

        if(m_world == null)
        {
            GetWorldEvent world = new GetWorldEvent();
            Event<GetWorldEvent>.Broadcast(world);
            m_world = world.world;
            if (m_world == null)
                return false;
        }

        ChunkView view = GetView();

        //todo make path !

        return true;
    }

    ChunkView GetView()
    {
        Vector3Int min = new Vector3Int(Mathf.Min(m_start.x, m_end.x), Mathf.Min(m_start.y, m_end.y), Mathf.Min(m_start.z, m_end.z));
        Vector3Int max = new Vector3Int(Mathf.Max(m_start.x, m_end.x), Mathf.Max(m_start.y, m_end.y), Mathf.Max(m_start.z, m_end.z));

        int offset = (max.x - min.x) - (max.z - min.z);
        int halfOffset = offset / 2;
        if (offset < 0)
        {
            min.x += halfOffset;
            max.x -= (offset - halfOffset);
        }
        else
        {
            min.z -= halfOffset;
            max.z += (offset - halfOffset);
        }

        min.x -= m_settings.maxDistanceFromPath;
        min.y -= m_settings.maxDistanceFromPath;
        min.z -= m_settings.maxDistanceFromPath;

        max.x += m_settings.maxDistanceFromPath;
        max.y += m_settings.maxDistanceFromPath;
        max.z += m_settings.maxDistanceFromPath;

        return m_world.GetChunkView(min, max - min);
    }

    public void Clear()
    {
        m_start = Vector3Int.zero;
        m_end = Vector3Int.zero;

        m_points.Clear();
        m_currentPoint = 0;
    }
}
