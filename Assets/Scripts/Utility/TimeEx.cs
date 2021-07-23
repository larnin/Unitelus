using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

//small class to manipulate time with multithread instance

static class TimeEx
{
    class TimeInstance
    {
        public bool m_isFixed;
        public float m_fixedTime;

        public float GetTime()
        {
            if (m_isFixed)
                return m_fixedTime;
            return Time.time;
        }
    }

    static ThreadLocal<TimeInstance> m_time = new ThreadLocal<TimeInstance>();

    public static float GetTime()
    {
        if (m_time.Value == null)
            m_time.Value = new TimeInstance();

        return m_time.Value.GetTime();
    }

    public static void SetFixedTime(float time)
    {
        if (m_time.Value == null)
            m_time.Value = new TimeInstance();

        m_time.Value.m_fixedTime = time;
        m_time.Value.m_isFixed = true;
    }

    public static void SetTimeDynamic()
    {
        if (m_time.Value == null)
            m_time.Value = new TimeInstance();

        m_time.Value.m_fixedTime = 0;
        m_time.Value.m_isFixed = false;
    }

}