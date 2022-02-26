using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class PathfinderPoolBehaviour : MonoBehaviour
{
    private void Start()
    {
        PathfinderPool.instance.Start();
    }

    private void OnDestroy()
    {
        PathfinderPool.instance.Stop();
    }
}

public class PathfinderPool
{
    static PathfinderPool m_instance;
    public static PathfinderPool instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new PathfinderPool();
            return m_instance;
        }
    }

    class HeavyDatas
    {
        public World world;
        public List<Node> visitedNodes = new List<Node>();
        public List<Node> nextNodes = new List<Node>(); //sorted
        public HashSet<ulong> computedPos = new HashSet<ulong>();
        public ChunkView view = null;

        public void Clear()
        {
            visitedNodes.Clear();
            nextNodes.Clear();
            computedPos.Clear();
            view = null;
        }
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

    class Job
    {
        public PathData path;
        public Vector3Int start;
        public Vector3Int end;
        public PathSettings settings;
    }

    bool m_stopped = false;
    Thread m_thread;

    readonly object m_waitingJobsLock = new object();
    List<Job> m_waitingJobs = new List<Job>();
    PathData m_currentPath = null;

    HeavyDatas m_datas = new HeavyDatas();

    public PathfinderPool()
    {

    }

    public void AddJob(PathData path, Vector3Int start, Vector3Int end, PathSettings settings)
    {
        lock(m_waitingJobsLock)
        {
            foreach (var j in m_waitingJobs)
            {
                if(j.path == path)
                {
                    j.start = start;
                    j.end = end;
                    j.settings = settings;
                    return;
                }
            }

            var job = new Job();
            job.path = path;
            job.settings = settings;
            job.start = start;
            job.end = end;

            m_waitingJobs.Add(job);
        }
    }

    public void Start()
    {
        m_stopped = false;
        m_thread = new Thread(new ThreadStart(Process));
        m_thread.Start();
    }

    public void Stop()
    {
        m_stopped = true;

        if (m_thread != null)
            m_thread.Join();

        m_instance = null;
    }

    public bool HaveJob(PathData path)
    {
        lock(m_waitingJobsLock)
        {
            foreach (var j in m_waitingJobs)
            {
                if (j.path == path)
                    return true;
            }

            if (m_currentPath == path)
                return true;
        }

        return false;
    }

    public void StopJob(PathData path)
    {
        lock(m_waitingJobs)
        {
            for(int i = 0; i < m_waitingJobs.Count; i++)
            {
                if(m_waitingJobs[i].path == path)
                {
                    m_waitingJobs.RemoveAt(i);
                    return;
                }    
            }
        }
    }

    void Process()
    {
        while (!m_stopped)
        {
            bool haveJob = false;

            lock(m_waitingJobsLock)
            {
                if (m_waitingJobs.Count > 0)
                    haveJob = true;
            }

            if (haveJob)
                StartNextTask();
            else Thread.Sleep(20);
        }
    }

    void StartNextTask()
    {
        Job job = null;

        lock(m_waitingJobsLock)
        {
            if (m_waitingJobs.Count > 0)
            {
                job = m_waitingJobs[0];
                m_waitingJobs.RemoveAt(0);
                m_currentPath = job.path;
            }
        }

        if (job == null)
            return;

        DoJob(job);

        lock (m_waitingJobsLock)
        {
            m_currentPath = null;
        }
    }

    void DoJob(Job job)
    {
        m_datas.Clear();

        if (m_datas.world == null)
        {
            GetWorldEvent world = new GetWorldEvent();
            Event<GetWorldEvent>.Broadcast(world);
            m_datas.world = world.world;
            if (m_datas.world == null)
            {
                SetEmptyPath(job);
                return;
            }
        }

        m_datas.view = GetView(job.start, job.end, job.settings.maxDistanceFromPath);

        Vector3Int start, end;
        bool bCanPlace = PlaceOnValidPosition(job.start, 5, out start);
        bCanPlace &= PlaceOnValidPosition(job.end, 5, out end);

        if (!bCanPlace)
        {
            SetEmptyPath(job);
            return;
        }

        if (start == end)
        {
            lock (job.path.dataLock)
            {
                job.path.points.Clear();
                job.path.world = m_datas.world;
                job.path.points.Add(start);
                job.path.updated = true;
            }
            return;
        }

        m_datas.nextNodes.Add(new Node(start, null, 0, end));
        job.start = start;
        job.end = end;

        bool foundPath = false;
        while (m_datas.nextNodes.Count > 0)
        {
            if (Step(job))
            {
                foundPath = true;
                break;
            }
        }

        if (!foundPath || m_datas.visitedNodes.Count == 0)
        {
            m_datas.Clear();
            SetEmptyPath(job);
            return;
        }

        var node = m_datas.visitedNodes[m_datas.visitedNodes.Count - 1];
        lock (job.path.dataLock)
        {
            job.path.world = m_datas.world;
            job.path.points.Clear();
            while (true)
            {
                job.path.points.Insert(0, node.current);
                if (node.previous == null)
                    break;
                node = node.previous;
            }
            job.path.updated = true;
        }
    }

    bool Step(Job job)
    {
        var node = m_datas.nextNodes[0];
        m_datas.nextNodes.RemoveAt(0);
        m_datas.visitedNodes.Add(node);

        var nextsPos = GetNextsPos(node.current, job.settings);
        for (int i = 0; i < nextsPos.Length; i++)
        {
            Vector3Int pos = nextsPos[i];

            if (IsVisited(pos))
                continue;

            var id = PosToID(pos);
            m_datas.computedPos.Add(id);

            float weight = CanWalkOn(pos, node.current, job.settings);
            if (weight < 0)
                continue;

            if (pos == job.end)
            {
                m_datas.visitedNodes.Add(new Node(pos, node, node.weight, job.end));
                return true;
            }

            //insert node in the nextPos, and keep it sorted with totalWeight
            weight *= (pos - node.current).magnitude;
            weight += node.weight;
            var newNode = new Node(pos, node, weight, job.end);
            int addIndex = m_datas.nextNodes.Count;
            for (int j = 0; j < m_datas.nextNodes.Count; j++)
            {
                if (newNode.totalWeight < m_datas.nextNodes[j].totalWeight)
                {
                    addIndex = j;
                    break;
                }
            }
            m_datas.nextNodes.Insert(addIndex, newNode);
        }

        return false;
    }

    void SetEmptyPath(Job job)
    {
        lock(job.path.dataLock)
        {
            job.path.world = m_datas.world;
            job.path.points.Clear();
            job.path.updated = true;
        }
    }

    ChunkView GetView(Vector3Int pos1, Vector3Int pos2, int maxDistanceFromPath)
    {
        Vector3Int min = new Vector3Int(Mathf.Min(pos1.x, pos2.x), Mathf.Min(pos1.y, pos2.y), Mathf.Min(pos1.z, pos2.z));
        Vector3Int max = new Vector3Int(Mathf.Max(pos1.x, pos2.x), Mathf.Max(pos1.y, pos2.y), Mathf.Max(pos1.z, pos2.z));

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

        min.x -= maxDistanceFromPath;
        min.y -= maxDistanceFromPath;
        min.z -= maxDistanceFromPath;

        max.x += maxDistanceFromPath;
        max.y += maxDistanceFromPath;
        max.z += maxDistanceFromPath;

        return m_datas.world.GetChunkView(min, max - min);
    }

    bool PlaceOnValidPosition(Vector3Int pos, int iterations, out Vector3Int result)
    {
        bool bIsOk = false;
        for (int i = 0; i < iterations; i++)
        {
            var b = m_datas.view.GetBlock(pos);
            var t = BlockTypeList.instance.Get(b.id);
            if (t.canWalkThrough)
            {
                bIsOk = true;
                break;
            }
            pos[1]++;
        }
        if (bIsOk)
        {
            bIsOk = false;
            for (int i = 0; i < iterations; i++)
            {
                var b = m_datas.view.GetBlock(pos);
                var t = BlockTypeList.instance.Get(b.id);
                if (t.canFloatTurough)
                {
                    bIsOk = true;
                    break;
                }

                var b2 = m_datas.view.GetBlock(pos + new Vector3Int(0, -1, 0));
                var t2 = BlockTypeList.instance.Get(b2.id);
                if (t2.canWalkOn)
                {
                    bIsOk = true;
                    break;
                }
                pos[1]--;
            }
        }

        result = pos;

        return bIsOk;
    }

    Vector3Int[] GetNextsPos(Vector3Int current, PathSettings settings)
    {
        if (settings.moveDirection == PathSettings.Direction.crossAndDiagonal)
        {
            var pos = new Vector3Int[(settings.agentStepUp + settings.agentStepDown + 1) * 3 * 3 - 1];

            int index = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -settings.agentStepDown; j <= settings.agentStepUp; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (i == 0 && j == 0 && k == 0)
                            continue;

                        pos[index++] = current + new Vector3Int(i, j, k);
                    }
                }
            }

            return pos;
        }
        else
        {
            var pos = new Vector3Int[(settings.agentStepUp + settings.agentStepDown + 1) * 4];
            var offsets = new Vector3Int[] { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1) };

            int index = 0;

            for(int i = -settings.agentStepDown; i <= settings.agentStepUp; i++)
            {
                for(int j = 0; j < offsets.Length; j++)
                {
                    pos[index++] = current + offsets[j] + new Vector3Int(0, i, 0);
                }
            }

            return pos;
        }
    }

    // return the walk weight of the block or -1 if not walkable
    float CanWalkOn(Vector3Int pos, Vector3Int previousPos, PathSettings settings)
    {
        //first, test ground
        //the bloc must be canFloatTurough or canWalkThrough + canWalkOn for the block below
        var bCenter = m_datas.view.GetBlock(pos);

        var typeCenter = BlockTypeList.instance.Get(bCenter.id);
        if (!typeCenter.canWalkThrough)
            return -1;
        if (!typeCenter.canFloatTurough)
        {
            var bDown = m_datas.view.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
            var typeDown = BlockTypeList.instance.Get(bDown.id);
            if (!typeDown.canWalkOn && !typeDown.canFloatTurough)
                return -1;
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

                    var b = m_datas.view.GetBlock(pos + new Vector3Int(i, j, k));
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
                    for (int j = 0; j < -vertical; j++)
                    {
                        var b = m_datas.view.GetBlock(pos + new Vector3Int(i, j + settings.agentHeight, k));
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
                        var b = m_datas.view.GetBlock(previousPos + new Vector3Int(i, j + settings.agentHeight, k));
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
            multiplier *= settings.stepWeightMultiplier;

        return typeCenter.pathWeight * multiplier;
    }

    bool IsVisited(Vector3Int pos)
    {
        var id = PosToID(pos);
        return m_datas.computedPos.Contains(id);
    }

    public ulong PosToID(Vector3Int pos)
    {
        int x, z;
        m_datas.world.ClampWorldPos(pos.x, pos.z, out x, out z);

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