using System;
using System.Collections.Generic;
using UnityEngine;

class Event<T>
{
	public class Subscriber : IEventSubscriber
	{
		private Action<T> m_func;

		public Subscriber(Action<T> func)
		{
            m_func = func;
		}

		void IEventSubscriber.Subscribe()
		{
			Event<T>.Subscribe(m_func);
		}

		void IEventSubscriber.Unsubscribe()
		{
			Event<T>.Unsubscribe(m_func);
		}
	}

    private static event Action<T> m_event;

	public static void Subscribe(Action<T> func)
	{
        m_event += func;
	}

	public static void Unsubscribe(Action<T> func)
	{
        m_event -= func;
	}

	public static void Broadcast(T data)
	{
		if (m_event != null)
		{
            m_event(data);
		}
	}

    //------------------------------------------------------
    public class LocalSubscriber : IEventSubscriber
    {
        private Action<T> m_func;
        private GameObject m_obj;

        public LocalSubscriber(Action<T> func, GameObject obj)
        {
            m_func = func;
            m_obj = obj;
        }

        void IEventSubscriber.Subscribe()
        {
            Event<T>.Subscribe(m_func, m_obj);
        }

        void IEventSubscriber.Unsubscribe()
        {
            Event<T>.Unsubscribe(m_func, m_obj);
        }
    }

    private static Dictionary<GameObject, Action<T>> m_events = new Dictionary<GameObject, Action<T>>();

    public static void Subscribe(Action<T> func, GameObject obj)
    {
        if (m_events.ContainsKey(obj))
            m_events[obj] += func;
        else m_events.Add(obj, func);
    }

    public static void Unsubscribe(Action<T> func, GameObject obj)
    {
        if (m_events.ContainsKey(obj))
            m_events[obj] -= func;
    }

    public static void Broadcast(T data, GameObject obj, bool propagateToChild = false)
    {
        Action<T> func;
        if (m_events.TryGetValue(obj, out func) && func != null)
            func(data);

        if(propagateToChild)
        {
            var t = obj.transform;
            for(int i = 0; i < t.childCount; i++)
                Broadcast(data, t.GetChild(i).gameObject, true);
        }
    }
}