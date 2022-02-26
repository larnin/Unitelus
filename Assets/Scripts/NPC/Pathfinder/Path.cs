using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathSettings
{
    public enum Direction { cross, crossAndDiagonal, }

    public int maxDistanceFromPath = 5;

    public int agentHeight = 2;
    public int agentWidth = 1;
    public int agentStepUp = 1;
    public int agentStepDown = 2;
    public float stepWeightMultiplier = 1.5f;
    public Direction moveDirection = Direction.crossAndDiagonal;
    public int smoothDistance = 3;
}

public class PathData
{
    public readonly object dataLock = new object();
    public List<Vector3Int> points = new List<Vector3Int>();
    public World world;
    public bool updated = false;
}

public class Path
{
    enum Status
    {
        Invalid = 0,
        Generating = 1 << 0,
        Valid = 1 << 1,
        Ended = 1 << 2,
    }

    PathData m_data = new PathData();

    PathSettings m_settings;

    Vector3Int m_start;
    Vector3Int m_end;

    Vector3 m_target;

    int m_currentPoint;
    Status m_status = Status.Invalid;

    public Path(PathSettings settings)
    {
        m_settings = settings;
        m_currentPoint = 0;
    }

    public bool pathValid { get { return (m_status & Status.Valid) > 0; } }
    public bool pathGenerated { get { return (m_status & Status.Generating) == 0; } }
    public bool pathEnded { get { return (m_status & Status.Ended) == 0; } }

    public Vector3 target { get { return m_target; } }

    public void Generate(Vector3 start, Vector3 end)
    {
        Generate(Vector3Int.FloorToInt(start), Vector3Int.FloorToInt(end));
    }

    public void Generate(Vector3Int start, Vector3Int end)
    {
        PathfinderPool.instance.AddJob(m_data, start, end, m_settings);
        m_start = start;
        m_end = end;
        m_status |= Status.Generating;
    }

    public void Process(Vector3 currentPos)
    {
        lock(m_data.dataLock)
        {
            if(m_data.updated)
                OnDataUpdate(currentPos);
            else if(pathValid)
                ProcessPath(currentPos);
        }
    }

    public void Draw()
    {
        if (!pathValid)
            return;

        lock (m_data.dataLock)
        {
            int nb = m_data.points.Count - 1;

            for (int i = 0; i < nb; i++)
            {
                Vector3 pos1 = m_data.points[i] + new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 pos2 = m_data.points[i + 1] + new Vector3(0.5f, 0.5f, 0.5f);

                DebugDraw.Line(pos1, pos2, Color.red);
            }

            DebugDraw.Rectangle(m_start, new Vector3(1, 1, 1), Color.red);
            DebugDraw.Rectangle(m_end, new Vector3(1, 1, 1), Color.red);

            if (m_currentPoint >= 0 && m_currentPoint < m_data.points.Count)
                DebugDraw.Rectangle(m_data.points[m_currentPoint], new Vector3(1, 1, 1), Color.blue);

            DebugDraw.Sphere(m_target, 0.5f, Color.green);
        }
    }

    void OnDataUpdate(Vector3 currentPos)
    {
        m_data.updated = false;
        m_currentPoint = 0;

        m_status &= ~Status.Generating;
        if(m_data.points.Count == 0)
        {
            m_status &= ~Status.Valid;
            return;
        }

        m_status |= Status.Valid;

        //update current point
        float distance = float.MaxValue;
        for(int i = 0; i < m_data.points.Count; i++)
        {
            Vector3 pos = m_data.points[i] + new Vector3(0.5f, 0.5f, 0.5f);
            float dist = (pos - currentPos).sqrMagnitude;
            if (dist < distance)
            {
                distance = dist;
                m_currentPoint = i;
            }
        }

        ProcessPath(currentPos, true);
    }

    void ProcessPath(Vector3 currentPos, bool force = false)
    {
        //update current position
        Vector3Int pos = Vector3Int.FloorToInt(currentPos);

        bool moved = false;
        for(int i = 1; i < m_settings.smoothDistance; i++)
        {
            int index = m_currentPoint + i;
            if (index >= m_data.points.Count)
                break;

            if(m_data.points[index] == pos)
            {
                moved = true;
                m_currentPoint = index;
                break;
            }
        }

        if (m_data.points.Count == 0 || m_currentPoint >= m_data.points.Count)
            m_status |= Status.Ended;
        else m_status &= ~Status.Ended;

        if (!moved && !force)
            return;

        //update target and try to smooth path
        bool haveSetTarget = false;
        float rayStep = 0.25f;
        Vector3Int oldPos = Vector3Int.FloorToInt(currentPos);
        float maxDist = m_settings.smoothDistance * 2 + 1;
        for(int i = m_settings.smoothDistance; i > 1; i--)
        {
            int index = m_currentPoint + i;
            if (index >= m_data.points.Count)
                continue;

            Vector3 target = m_data.points[index] + new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 dir = target - currentPos;
            float dist = dir.magnitude;
            dir /= dist;

            if (dist >= maxDist)
                continue;

            bool testOk = true;
            int nbStep = (int)(dist / rayStep);
            for(int j = 0; j < nbStep; j++)
            {
                Vector3 testPos = currentPos + dir * j * rayStep;
                Vector3Int testPosI = Vector3Int.FloorToInt(testPos);
                if (testPosI == oldPos)
                    continue;

                if(!CanWalkOn(testPosI, oldPos, m_settings))
                {
                    testOk = false;
                    break;
                }
            }

            if(testOk)
            {
                haveSetTarget = true;
                m_target = target;

                break;
            }
        }

        if(!haveSetTarget)
        {
            if (m_currentPoint >= m_data.points.Count)
                m_target = m_end + new Vector3(0.5f, 0.5f, 0.5f);
            else if (m_currentPoint == m_data.points.Count - 1)
                m_target = m_data.points[m_currentPoint] + new Vector3(0.5f, 0.5f, 0.5f);
            else m_target = m_data.points[m_currentPoint + 1] + new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    // similar to PathfinderPool.CanWalkOn
    // must update this one too
    bool CanWalkOn(Vector3Int pos, Vector3Int previousPos, PathSettings settings)
    {
        if (m_data.world == null)
            return false;

        //first, test ground
        //the bloc must be canFloatTurough or canWalkThrough + canWalkOn for the block below
        var bCenter = m_data.world.GetBlock(pos);

        var typeCenter = BlockTypeList.instance.Get(bCenter.id);
        if (!typeCenter.canWalkThrough)
            return false;
        if (!typeCenter.canFloatTurough)
        {
            var bDown = m_data.world.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
            var typeDown = BlockTypeList.instance.Get(bDown.id);
            if (!typeDown.canWalkOn && !typeDown.canFloatTurough)
                return false;
        }

        //the blocks must be canWalkThrough on the agent area
        int radius = settings.agentWidth / 2;
        for (int i = -radius; i <= radius; i++)
        {
            for (int k = -radius; k <= radius; k++)
            {
                for (int j = 0; j < settings.agentHeight; j++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue; //already tested with the ground test

                    var b = m_data.world.GetBlock(pos + new Vector3Int(i, j, k));
                    var type = BlockTypeList.instance.Get(b.id);
                    if (!type.canWalkThrough)
                        return false;
                }
            }
        }

        //if the agent need to go down or up, we check blocks with vertical offset
        int vertical = pos.y - previousPos.y;
        if (vertical < 0) //go down
        {
            for (int i = -radius; i <= radius; i++)
            {
                for (int k = -radius; k <= radius; k++)
                {
                    for (int j = 0; j < -vertical; j++)
                    {
                        var b = m_data.world.GetBlock(pos + new Vector3Int(i, j + settings.agentHeight, k));
                        var type = BlockTypeList.instance.Get(b.id);
                        if (!type.canWalkThrough)
                            return false;
                    }
                }
            }
        }
        else if (vertical > 0) // go up
        {
            for (int i = -radius; i <= radius; i++)
            {
                for (int k = -radius; k <= radius; k++)
                {
                    for (int j = 0; j < vertical; j++)
                    {
                        var b = m_data.world.GetBlock(previousPos + new Vector3Int(i, j + settings.agentHeight, k));
                        var type = BlockTypeList.instance.Get(b.id);
                        if (!type.canWalkThrough)
                            return false;
                    }
                }
            }
        }

        return true;
    }
}
