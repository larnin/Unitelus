using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    Color[] m_logColors = new Color[]
    {
        Color.white,
        Color.yellow,
        Color.red
    };

    const float m_spacing = 15;
    const int m_maxLines = 50;
    static readonly object m_linesLock = new object();
    static List<Line> m_lines = new List<Line>();

    static int m_mainThreadID;

    enum LogType
    {
        Log,
        Warning,
        Error
    }

    class Line
    {
        public Line(string _text, LogType _logType)
        {
            text = _text;
            logType = _logType;
            sent = false;
        }

        public string text;
        public LogType logType;
        public bool sent;
    }

#if !UNITY_EDITOR
    private void OnGUI()
    {
        lock (m_linesLock)
        {
            for (int i = 0; i < m_lines.Count; i++)
            {
                GUI.contentColor = m_logColors[(int)(m_lines[i].logType)];
                GUI.Label(new Rect(10, 10 + m_spacing * i, 500, 50), m_lines[i].text);
            }
        }
    }
#endif

    private void Start()
    {
        m_mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    private void Update()
    {
        lock (m_linesLock)
        {
            foreach(var l in m_lines)
            {
                if(!l.sent)
                {
                    l.sent = true;
                    LogLine(l.text, l.logType);
                }
            }
        }
    }

    public static void Log(string line)
    {
        AddLine(line, LogType.Log);
    }

    public static void Warning(string line)
    {
        AddLine(line, LogType.Warning);
    }

    public static void Error(string line)
    {
        AddLine(line, LogType.Error);
    }

    static void AddLine(string line, LogType type)
    {
        Line l = new Line(line, type);
        if(System.Threading.Thread.CurrentThread.ManagedThreadId == m_mainThreadID)
        {
            LogLine(l.text, l.logType);
            l.sent = true;
        }
        lock (m_linesLock)
        {
            m_lines.Add(l);
            while (m_lines.Count > m_maxLines)
                m_lines.RemoveAt(0);
        }
    }

    //must be on the main thread to do it
    static void LogLine(string line, LogType type)
    {
        if (type == LogType.Log)
            Debug.Log(line);
        else if (type == LogType.Warning)
            Debug.LogWarning(line);
        else if (type == LogType.Error)
            Debug.LogError(line);
    }
}
