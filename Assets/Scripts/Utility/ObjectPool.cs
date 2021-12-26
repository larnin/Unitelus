using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ObjectPoolItem
{
    public bool valid { get; set; } // must not be set outside of object pool

    public ObjectPoolItem()
    {
        valid = false;
    }

    public abstract void Reset();
}

public class ObjectPool <T> where T : ObjectPoolItem, new()
{
    List<T> m_items = new List<T>();
    public ObjectPool(int initialSize = 0)
    {
        Resize(initialSize);
    }

    public void Resize(int size)
    {
        if (m_items.Count > size)
            m_items.RemoveRange(size, m_items.Count - size);
        while (m_items.Count < size)
            m_items.Add(new T());
    }

    public T Get()
    {
        if (m_items.Count == 0)
        {
            T newItem = new T();
            newItem.valid = true;
            return newItem;
        }

        T item = m_items[m_items.Count - 1];
        item.valid = true;
        m_items.RemoveAt(m_items.Count - 1);

        return item;
    }

    public void Add(T item)
    {
        item.valid = false;
        item.Reset();
        m_items.Add(item);
    }
}
