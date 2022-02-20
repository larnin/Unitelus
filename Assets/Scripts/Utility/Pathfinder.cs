using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathSettings
{
    public int maxDistanceFromPath = 5;

    public int agentHeight = 2;
    public int agentWidth = 1;
    public int agentStepUp = 1;
    public int agentStepDown = 2;
    public float stepWeightMultiplier = 1.5f;
}

public class Path
{
    public enum Status
    {
        Invalid, //initial state, impossible to generate
        Generating, // during generation, usefull if threaded
        Valid, // path generated successfully
        ended, // current position on the end
    }

    World m_world;
    PathSettings m_settings;

    Vector3Int m_start;
    Vector3Int m_end;

    List<Vector3Int> m_points = new List<Vector3Int>();
    Vector3 m_target;
    int m_currentPoint;
    Status m_status = Status.Invalid;

    public Path(PathSettings settings)
    {
        m_settings = settings;
        m_currentPoint = 0;
    }

    public bool IsPathValid()
    {
        return m_points.Count > 0;
    }

    public Status GetStatus()
    {
        return m_status;
    }

    public Vector3Int GetCurrentPoint()
    {
        if (m_currentPoint < 0 || m_currentPoint >= m_points.Count)
            return Vector3Int.zero;

        return m_points[m_currentPoint];
    }

    class Node
    {
        public float weight;
        public float totalWeight;
        public Node previous;
        public Vector3Int current;

        public Node(Vector3Int _current, Node _previous, float _weight, Vector3Int _target)
        {
            current = _current;
            previous = _previous;
            weight = _weight;
            float dist = (_target - _current).magnitude;
            totalWeight = dist + weight;
        }
    }

    //buffers for path generation
    List<Node> m_visitedNodes = new List<Node>();
    List<Node> m_nextNodes = new List<Node>(); //sorted
    HashSet<ulong> m_computedPos = new HashSet<ulong>();
    ChunkView m_view = null;

    public bool Generate(Vector3 start, Vector3 end)
    {
        return Generate(Vector3Int.FloorToInt(start), Vector3Int.FloorToInt(end));
    }

    public bool Generate(Vector3Int start, Vector3Int end)
    {
        Stopwatch chrono = new Stopwatch();
        chrono.Start();

        m_status = Status.Generating;

        Clear();

        m_start = start;
        m_end = end;

        if(m_world == null)
        {
            GetWorldEvent world = new GetWorldEvent();
            Event<GetWorldEvent>.Broadcast(world);
            m_world = world.world;
            if (m_world == null)
            {
                m_status = Status.Invalid;
                return false;
            }
        }

        if (m_start == m_end)
        {
            m_points.Add(m_start);
            m_target = m_start + new Vector3(0.5f, 0.5f, 0.5f);
            m_status = Status.Valid;
            return true;
        }

        m_view = GetView();

        PlaceStartAndEndOnValidPosition();

        m_visitedNodes.Clear();
        m_nextNodes.Clear();

        m_nextNodes.Add(new Node(m_start, null, 0, m_end));

        bool foundPath = false;
        while(m_nextNodes.Count > 0)
        {
            if(Step())
            {
                foundPath = true;
                break;
            }
        }

        if(!foundPath || m_visitedNodes.Count == 0)
        {
            Clean();
            m_status = Status.Invalid;

            var time2 = chrono.Elapsed.TotalSeconds * 1000;
            UnityEngine.Debug.Log("Path " + time2 + " ms - Not found");

            return false;
        }

        var node = m_visitedNodes[m_visitedNodes.Count - 1];
        while(true)
        {
            m_points.Insert(0, node.current);
            if (node.previous == null)
                break;
            node = node.previous;
        }

        var time = chrono.Elapsed.TotalSeconds * 1000;
        UnityEngine.Debug.Log("Path " + time + " ms");

        for(int i = 0; i < m_visitedNodes.Count; i++)
        {
            DebugDraw.Cube(m_visitedNodes[i].current, new Vector3(1, 1, 1), Color.blue, 1);
        }

        Clean();
        m_status = Status.Valid;
        m_target = m_start + new Vector3(0.5f, 0.5f, 0.5f);
        return true;
    }

    void PlaceStartAndEndOnValidPosition()
    {
        var bStart = m_view.GetBlock(m_start);
        var typeStart = BlockTypeList.instance.Get(bStart.id);
        if (!typeStart.canWalkThrough)
            m_start[1]++;

        var bEnd = m_view.GetBlock(m_end);
        var typeEnd = BlockTypeList.instance.Get(bEnd.id);
        if (!typeEnd.canWalkThrough)
            m_end[1]++;
    }

    // return true if the current case is the end
    bool Step()
    {
        var node = m_nextNodes[0];
        m_nextNodes.RemoveAt(0);
        m_visitedNodes.Add(node);

        var nextsPos = GetNextsPos(node.current);
        for(int i = 0; i < nextsPos.Length; i++)
        {
            Vector3Int pos = nextsPos[i];

            if (IsVisited(pos))
                continue;

            var id = PosToID(pos);
            m_computedPos.Add(id);

            float weight = CanWalkOn(pos, node.current);
            if (weight < 0)
                continue;

            if (pos == m_end)
            {
                m_visitedNodes.Add(new Node(pos, node, node.weight, m_end));
                return true;
            }

            //insert node in the nextPos, and keep it sorted with totalWeight
            weight *= (pos - node.current).magnitude;
            weight += node.weight;
            var newNode = new Node(pos, node, weight, m_end);
            int addIndex = m_nextNodes.Count;
            for(int j = 0; j < m_nextNodes.Count; j++)
            {
                if (newNode.totalWeight < m_nextNodes[j].totalWeight)
                {
                    addIndex = j;
                    break;
                }
            }
            m_nextNodes.Insert(addIndex, newNode);
        }

        return false;
    }

    //must be called at the end of the generation to clean lists
    void Clean()
    {
        m_view = null;
        m_visitedNodes.Clear();
        m_nextNodes.Clear();
        m_computedPos.Clear();
    }

    bool IsVisited(Vector3Int pos)
    {
        var id = PosToID(pos);
        return m_computedPos.Contains(id);
    }

    // return the walk weight of the block or -1 if not walkable
    float CanWalkOn(Vector3Int pos, Vector3Int previousPos)
    {
        //first, test ground
        //the bloc must be canFloatTurough or canWalkThrough + canWalkOn for the block below
        var bCenter = m_view.GetBlock(pos);

        var typeCenter = BlockTypeList.instance.Get(bCenter.id);
        if (!typeCenter.canWalkThrough)
            return -1;
        if(!typeCenter.canFloatTurough)
        {
            var bDown = m_view.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
            var typeDown = BlockTypeList.instance.Get(bDown.id);
            if (!typeDown.canWalkOn && !typeDown.canFloatTurough)
                return -1;
        }

        //the blocks must be canWalkThrough on the agent area
        int radius = m_settings.agentWidth / 2;
        for(int i = -radius; i <= radius; i++)
        {
            for(int k = -radius; k <= radius; k++)
            {
                for(int j = 0; j < m_settings.agentHeight; j++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue; //already tested with the ground test

                    var b = m_view.GetBlock(pos + new Vector3Int(i, j, k));
                    var type = BlockTypeList.instance.Get(b.id);
                    if (!type.canWalkThrough)
                        return -1;
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
                    for(int j = 0; j < -vertical; j++)
                    {
                        var b = m_view.GetBlock(pos + new Vector3Int(i, j + m_settings.agentHeight, k));
                        var type = BlockTypeList.instance.Get(b.id);
                        if (!type.canWalkThrough)
                            return -1;
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
                        var b = m_view.GetBlock(previousPos + new Vector3Int(i, j + m_settings.agentHeight, k));
                        var type = BlockTypeList.instance.Get(b.id);
                        if (!type.canWalkThrough)
                            return -1;
                    }
                }
            }
        }

        float multiplier = 1;
        int absVertical = Mathf.Abs(vertical);
        for (int i = 0; i < absVertical; i++)
            multiplier *= m_settings.stepWeightMultiplier;

        return typeCenter.pathWeight * multiplier;
    }

    Vector3Int[] GetNextsPos(Vector3Int current)
    {
        var pos = new Vector3Int[(m_settings.agentStepUp + m_settings.agentStepDown + 1) * 3 * 3 - 1];

        int index = 0;

        for(int i = -1; i <= 1; i++)
        {
            for (int j = -m_settings.agentStepDown; j <= m_settings.agentStepUp; j++)
            {
                for(int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue;

                    pos[index++] = current + new Vector3Int(i, j, k);
                }
            }
        }

        return pos;
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
        m_target = Vector3Int.zero;
        m_currentPoint = 0;
    }

    public void Process(Vector3 currentPos)
    {
        if (m_status != Status.Valid)
            return;

        //todo more complexe process to smooth path

        var pos = Vector3Int.FloorToInt(currentPos);

        if(pos == m_points[m_currentPoint])
        {
            if(m_currentPoint == m_points.Count - 1)
            {
                m_status = Status.ended;
                m_target = m_end + new Vector3(0.5f, 0.5f, 0.5f);
                return;
            }
            m_currentPoint++;
            m_target = m_points[m_currentPoint] + new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    public Vector3 GetPos()
    {
        return m_target;
    }

    public void Draw()
    {
        if (m_status != Status.Valid)
            return;

        for(int i = 0; i < m_points.Count - 1; i++)
        {
            Vector3 pos1 = m_points[i] + new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 pos2 = m_points[i + 1] + new Vector3(0.5f, 0.5f, 0.5f);
            DebugDraw.Line(pos1, pos2, Color.red);
        }

        DebugDraw.Cube(m_start, new Vector3(1, 1, 1), Color.red);
        DebugDraw.Cube(m_end, new Vector3(1, 1, 1), Color.red);
    }

    public ulong PosToID(Vector3Int pos)
    {
        int x, z;
        m_world.ClampWorldPos(pos.x, pos.z, out x, out z);

        int y = pos.y + (1 << 15);

        ulong uX = (ulong)(x & 0xFFFFFF); // 24 bits max
        ulong uY = (ulong)(y & 0xFFFF); // 16 bits max
        ulong uZ = (ulong)(z & 0xFFFFFF); // 24 bits max

        ulong value = uX << 16;
        value += uY;
        value <<= 24;
        value += uZ;

        return value;
    }
}
