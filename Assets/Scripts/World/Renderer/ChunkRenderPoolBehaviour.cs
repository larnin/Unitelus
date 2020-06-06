using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderPoolBehaviour : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<CenterUpdatedEvent>.Subscriber(OnCenterMoved));

        m_subscriberList.Subscribe();
    }

    private void Start()
    {
        ChunkRendererPool.instance.Start();
    }

    private void OnDestroy()
    {
        ChunkRendererPool.instance.Stop();
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        ChunkRendererPool.instance.UpdateEndedJobs();
    }

    void OnCenterMoved(CenterUpdatedEvent e)
    {
        ChunkRendererPool.instance.SetCenter(e.pos);
    }
}

public class ChunkRendererPool
{
    const int m_dataPoolSize = 8;
    const int m_endedMaxFrames = 5;

    static ChunkRendererPool m_instance;
    public static ChunkRendererPool instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new ChunkRendererPool();
            return m_instance;
        }
    }

    class Job
    {
        public int x = 0;
        public int z = 0;
        public int layer = 0;
        public World world = null;
        public MeshParams<WorldVertexDefinition> data = null;
        public bool aborted = false;
        public int taskID = 0;
        public int endedFrames = 0;
    }

    class DoingJobStatus
    {
        public bool working = false;
        public int x = 0;
        public int z = 0;
        public int layer = 0;
        public World world = null;
        public bool aborted = false;
        public int taskID = 0;

        public void CloneJob(Job job)
        {
            working = true;
            world = job.world;
            x = job.x;
            z = job.z;
            layer = job.layer;
            taskID = job.taskID;
            aborted = job.aborted;
        }

    }

    bool m_stopped = false;

    readonly object m_freeDatasLock = new object();
    List<MeshParams<WorldVertexDefinition>> m_freeDatas = new List<MeshParams<WorldVertexDefinition>>();

    readonly object m_waitingJobsLock = new object();
    List<Job> m_waitingJobs = new List<Job>();

    readonly object m_doingJobLock = new object();
    Job m_doingJob = null;
    readonly object m_doingJobStatusLock = new object();
    DoingJobStatus m_doingJobStatus = new DoingJobStatus();

    readonly object m_endedJobsLock = new object();
    List<Job> m_endedJobs = new List<Job>();

    readonly object m_centerLock = new object();
    Vector3 m_center = Vector3.zero;

    Thread m_thread;

    int m_nextTaskID = 0;

    public ChunkRendererPool()
    {
        for (int i = 0; i < m_dataPoolSize; i++)
            m_freeDatas.Add(new MeshParams<WorldVertexDefinition>());
    }

    public void SetCenter(Vector3 center)
    {
        lock(m_centerLock)
        {
            m_center = center;
        }
    }

    public int AddJob(int x, int z, int layer, World world)
    {
        lock (m_waitingJobsLock)
        {
            var currentItem = m_waitingJobs.Find(v => { return x == v.x && z == v.z && layer == v.layer && world == v.world; });
            if (currentItem != null)
                return currentItem.taskID;
            var job = new Job();
            job.x = x;
            job.z = z;
            job.layer = layer;
            job.world = world;
            job.taskID = m_nextTaskID++;
            m_waitingJobs.Add(job);
            return job.taskID;
        }
    }

    public MeshParams<WorldVertexDefinition> GetJobData(int taskID)
    {
        lock (m_endedJobsLock)
        {
            var item = m_endedJobs.Find(v => { return taskID == v.taskID; });
            if (item == null)
                return null;
            return item.data;
        }
    }

    public bool HaveJob(int taskID)
    {
        lock(m_waitingJobsLock)
        {
            lock(m_doingJobStatusLock)
            {
                lock(m_endedJobsLock)
                {
                    foreach (var j in m_waitingJobs)
                        if (j.taskID == taskID)
                            return true;
                    if (m_doingJobStatus.working && m_doingJobStatus.taskID == taskID && !m_doingJobStatus.aborted)
                        return true;
                    foreach (var j in m_endedJobs)
                        if (j.taskID == taskID)
                            return true;
                }
            }
        }

        return false;
    }

    //free the oldest job that match
    public bool FreeJob(int taskID)
    {
        lock (m_waitingJobsLock)
        {
            lock (m_doingJobStatusLock)
            {
                lock (m_endedJobsLock)
                {
                    for (int i = 0; i < m_endedJobs.Count; i++)
                    {
                        var j = m_endedJobs[i];

                        if (j.taskID == taskID)
                        {
                            m_endedJobs.RemoveAt(i);
                            CleanData(j.data);
                            return true;
                        }
                    }

                    if(m_doingJobStatus.working && m_doingJobStatus.taskID == taskID && !m_doingJobStatus.aborted)
                    {
                        m_doingJobStatus.aborted = true;
                        return true;
                    }

                    for (int i = 0; i < m_waitingJobs.Count; i++)
                    {
                        var j = m_waitingJobs[i];
                        if (j.taskID == taskID)
                        {
                            m_waitingJobs.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void UpdateEndedJobs()
    {
        lock(m_endedJobsLock)
        {
            for(int i = 0; i < m_endedJobs.Count; i++)
            {
                m_endedJobs[i].endedFrames++;

                if(m_endedJobs[i].endedFrames > m_endedMaxFrames)
                {
                    CleanData(m_endedJobs[i].data);
                    m_endedJobs.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    void CleanData(MeshParams<WorldVertexDefinition> data)
    {
        data.ResetSize();
        lock (m_freeDatasLock)
        {
            m_freeDatas.Add(data);
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

    void Process()
    {
        while (!m_stopped)
        {
            bool haveJob = false;
            //assume that m_waitingJobs && m_freeDatas sizes will not go down until use 
            lock (m_waitingJobsLock)
            {
                haveJob = m_waitingJobs.Count > 0;
            }
            lock (m_freeDatasLock)
            {
                if (m_freeDatas.Count == 0)
                    haveJob = false;
            }

            if (haveJob)
                StartNextTask();
            else Thread.Sleep(50);
        }
    }

    void StartNextTask()
    {
        //create next task
        MeshParams<WorldVertexDefinition> data = null;
        lock (m_freeDatasLock)
        {
            if (m_freeDatas.Count > 0)
            {
                data = m_freeDatas[0];
                m_freeDatas.RemoveAt(0);
            }
        }
        if (data == null)
            return;

        int nextJobID = GetNearestJobIndex();

        lock (m_waitingJobsLock)
        {
            Job job = null;

            if(nextJobID >= 0)
            {
                job = m_waitingJobs[nextJobID];
                m_waitingJobs.RemoveAt(nextJobID);
            }

            if (job == null)
            {
                lock (m_freeDatasLock)
                {
                    m_freeDatas.Add(data);
                }
                return;
            }

            data.ResetSize();
            job.data = data;

            lock (m_doingJobStatusLock)
            {
                lock (m_doingJobLock)
                {
                    Debug.Assert(m_doingJob == null);
                    m_doingJob = job;
                    m_doingJobStatus.CloneJob(m_doingJob);
                }
            }
        }

        DoJob();

        //move task to ended task
        lock (m_doingJobLock)
        {
            lock(m_doingJobStatusLock)
            {
                m_doingJobStatus.working = false;
                m_doingJob.aborted = m_doingJobStatus.aborted;
            }

            lock (m_endedJobsLock)
            {

                if(m_doingJob.aborted)
                    CleanData(m_doingJob.data);
                else m_endedJobs.Add(m_doingJob);

                m_doingJob = null;
            }
        }
    }

    void DoJob()
    {
        lock (m_doingJobLock)
        {
            if (PlaceholderPass.instance == null)
                return;
            var pass = PlaceholderPass.instance.m_pass;

            foreach (var p in pass)
            {
                var chunk = m_doingJob.world.GetChunk(m_doingJob.x, m_doingJob.z);
                if (chunk != null)
                {
                    p.Render(chunk, m_doingJob.x, m_doingJob.layer, m_doingJob.z, m_doingJob.data);
                }
            }

            Debug.Log("Job done on " + m_doingJob.x + " " + m_doingJob.layer + " " + m_doingJob.z);
        }
    }

    int GetNearestJobIndex()
    {
        float minDistance = float.MaxValue;
        int bestIndex = -1;

        Vector3 center;
        lock(m_centerLock)
        {
            center = m_center;
        }

        lock(m_waitingJobsLock)
        {
            if (m_waitingJobs.Count == 0)
                return -1;

            for (int i = 0; i < m_waitingJobs.Count; i++)
            {
                var job = m_waitingJobs[i];
                var pos = new Vector3(job.x + 0.5f, job.layer + 0.5f, job.z + 0.5f);
                pos *= Chunk.chunkSize;

                var dist = (center - pos).sqrMagnitude;
                if(dist < minDistance)
                {
                    bestIndex = i;
                    minDistance = dist;
                }
            }
        }

        Debug.Assert(bestIndex >= 0);
        return bestIndex;
    }
}
