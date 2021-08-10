using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DebugTimer
{
    Stopwatch m_StopWatch = new Stopwatch();

    public void Start()
    {
        m_StopWatch.Start();
    }

    public void Stop()
    {
        m_StopWatch.Stop();
    }

    public void Restart()
    {
        m_StopWatch.Restart();
    }

    public bool IsRunning()
    {
        return m_StopWatch.IsRunning;
    }

    public long ElapsedTimeMS()
    {
        return m_StopWatch.ElapsedMilliseconds;
    }

    public double ElapsedTime()
    {
        return m_StopWatch.Elapsed.TotalSeconds;
    }

    public void Log(string title)
    {
        long time = ElapsedTimeMS();
        if(time < 1000)
            DebugConsole.Log(title + " " + ElapsedTimeMS() + "ms");
        else if(time < 60000)
        {
            long sec = time / 1000;
            time %= 1000;
            DebugConsole.Log(title + " " + sec + "s " + time + "ms");
        }
        else
        {
            long sec = time / 1000;
            time %= 1000;
            long min = sec / 60;
            sec %= 60;
            DebugConsole.Log(title + " " + min + "m " + sec + "s " + time + "ms");
        }
    }

    public void LogAndRestart(string title)
    {
        Log(title);
        Restart();
    }
}