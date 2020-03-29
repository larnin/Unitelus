﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderPoolBehaviour : MonoBehaviour
{
    private void Start()
    {
        ChunkRendererPool.instance.Start();
    }

    private void OnDestroy()
    {
        ChunkRendererPool.instance.Stop();
    }
}

public class ChunkRendererPool
{
    const int m_dataPoolSize = 8;

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
    }

    bool m_stopped = false;

    readonly object m_freeDatasLock = new object();
    List<MeshParams<WorldVertexDefinition>> m_freeDatas = new List<MeshParams<WorldVertexDefinition>>();

    readonly object m_waitingJobsLock = new object();
    List<Job> m_waitingJobs = new List<Job>();

    readonly object m_doingJobLock = new object();
    Job m_doingJob = null;

    readonly object m_endedJobsLock = new object();
    List<Job> m_endedJobs = new List<Job>();

    Thread m_thread;

    public ChunkRendererPool()
    {
        for (int i = 0; i < m_dataPoolSize; i++)
            m_freeDatas.Add(new MeshParams<WorldVertexDefinition>());
    }

    public bool AddJob(int x, int z, int layer, World world)
    {
        lock (m_waitingJobs)
        {
            var currentItem = m_waitingJobs.Find(v => { return x == v.x && z == v.z && layer == v.layer && world == v.world; });
            if (currentItem != null)
                return false;
            var job = new Job();
            job.x = x;
            job.z = z;
            job.layer = layer;
            job.world = world;
            m_waitingJobs.Add(job);
            return true;
        }
    }

    public MeshParams<WorldVertexDefinition> GetJobData(int x, int z, int layer, World world)
    {
        lock (m_endedJobsLock)
        {
            var item = m_endedJobs.Find(v => { return x == v.x && z == v.z && layer == v.layer && world == v.world; });
            if (item == null)
                return null;
            return item.data;
        }
    }

    //free the oldest job that match
    public bool FreeJob(int x, int z, int layer, World world)
    {
        lock (m_waitingJobsLock)
        {
            lock (m_doingJobLock)
            {
                lock (m_endedJobsLock)
                {
                    for (int i = 0; i < m_endedJobs.Count; i++)
                    {
                        var j = m_endedJobs[i];

                        if (j.x == x && j.z == z && j.layer == layer && j.world == world)
                        {
                            m_endedJobs.RemoveAt(i);
                            CleanData(j.data);
                            return true;
                        }
                    }

                    if (m_doingJob != null && m_doingJob.x == x && m_doingJob.z == z && m_doingJob.layer == layer && m_doingJob.world == world && !m_doingJob.aborted)
                    {
                        m_doingJob.aborted = true;
                        return true;
                    }

                    for (int i = 0; i < m_waitingJobs.Count; i++)
                    {
                        var j = m_waitingJobs[i];
                        if (j.x == x && j.z == z && j.layer == layer && j.world == world)
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
            lock (m_waitingJobs)
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

        lock (m_waitingJobs)
        {
            Job job = null;

            if (m_waitingJobs.Count > 0)
            {
                job = m_waitingJobs[0];
                m_waitingJobs.RemoveAt(0);
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

            lock(m_doingJobLock)
            {
                Debug.Assert(m_doingJob == null);
                m_doingJob = job;
            }
        }

        DoJob();

        //move task to ended task
        lock(m_endedJobsLock)
        {
            lock(m_doingJobLock)
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
}