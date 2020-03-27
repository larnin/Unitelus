using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    const float m_spacing = 15;
    const int m_maxLines = 50;
    static List<String> m_lines = new List<string>();

    private void OnGUI()
    {
        for (int i = 0; i < m_lines.Count; i++)
            GUI.Label(new Rect(10, 10 + m_spacing * i, 500, 50), m_lines[i]);
    }

    public static void Log(string line)
    {
        m_lines.Add(line);
        while (m_lines.Count > m_maxLines)
            m_lines.RemoveAt(0);
        Debug.Log(line);
    }
}
