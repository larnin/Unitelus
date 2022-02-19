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
    public int agentWidth = 1;
    public int agentStepUp = 1;
    public int agentStepDown = 2;
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
    Vector3Int m_target;
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

    public Status status()
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
    ChunkView m_view = null;

    public bool Generate(Vector3Int start, Vector3Int end)
    {
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
            m_status = Status.Valid;
            return true;
        }

        m_view = GetView();

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

        Clean();
        m_status = Status.Valid;
        return true;
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

            float weight = CanWalkOn(pos);
            if(weight < 0)
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
    }

    bool IsVisited(Vector3Int pos)
    {
        //slow, need to fasten this shit
        foreach (var n in m_visitedNodes)
            if (n.current == pos)
                return true;
        foreach (var n in m_nextNodes)
            if (n.current == pos)
                return true;

        return false;
    }

    // return the walk weight of the block or -1 if not walkable
    float CanWalkOn(Vector3Int pos)
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

        return typeCenter.pathWeight;
    }

    public Vector3Int[] GetNextsPos(Vector3Int current)
    {
        var pos = new Vector3Int[m_settings.agentStepUp + m_settings.agentStepDown + 4];

        pos[0] = current + new Vector3Int(1, 0, 0);
        pos[1] = current + new Vector3Int(-1, 0, 0);
        pos[2] = current + new Vector3Int(0, 0, 1);
        pos[3] = current + new Vector3Int(0, 0, -1);

        for (int i = 0; i < m_settings.agentStepDown; i++)
            pos[3 + i] = current + new Vector3Int(0, -i - 1, 0);

        for (int i = 0; i < m_settings.agentStepUp; i++)
            pos[3 + m_settings.agentStepDown + i] = current + new Vector3Int(0, i + 1, 0);

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
                m_target = m_end;
                return;
            }
            m_currentPoint++;
            m_target = m_points[m_currentPoint];
        }
    }

    public Vector3 GetPos()
    {
        return m_target;
    }
}
