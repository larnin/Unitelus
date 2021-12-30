using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// faster logs than debug.log
public static class Logs
{
    static List<string> m_logs = new List<string>();
    static List<string> m_important = new List<string>();

    public static void Add(string text)
    {
        m_logs.Add(text);
    }

    public static void ImportantAdd(string text)
    {
        m_important.Add(text);
    }

    public static void Dump()
    {
        Dump("logs.txt", m_logs);
        Dump("important.txt", m_important);
    }

    static void Dump(string file, List<string> logs)
    {
        string bigStr = "";

        foreach (var log in logs)
        {
            Debug.Log(log);
            bigStr += log + '\n';
        }
        m_logs.Clear();

        File.WriteAllText("C:/Users/ni-la/Prog/Unity/Unitelus/Logs/" + file, bigStr);
    }
}
