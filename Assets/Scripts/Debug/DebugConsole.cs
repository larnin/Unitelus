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
    static List<Line> m_lines = new List<Line>();

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
        }

        public string text;
        public LogType logType;
    }

#if !UNITY_EDITOR
    private void OnGUI()
    {
        for (int i = 0; i < m_lines.Count; i++)
        {
            GUI.contentColor = m_logColors[(int)(m_lines[i].logType)];
            GUI.Label(new Rect(10, 10 + m_spacing * i, 500, 50), m_lines[i].text);
        }
    }
#endif

    public static void Log(string line)
    {
        m_lines.Add(new Line(line, LogType.Log));
        while (m_lines.Count > m_maxLines)
            m_lines.RemoveAt(0);
        Debug.Log(line);
    }

    public static void Warning(string line)
    {
        m_lines.Add(new Line(line, LogType.Warning));
        while (m_lines.Count > m_maxLines)
            m_lines.RemoveAt(0);
        Debug.LogWarning(line);
    }

    public static void Error(string line)
    {
        m_lines.Add(new Line(line, LogType.Error));
        while (m_lines.Count > m_maxLines)
            m_lines.RemoveAt(0);
        Debug.LogError(line);
    }
}
