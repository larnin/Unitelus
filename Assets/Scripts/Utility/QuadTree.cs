using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuadTree<T>
{
    class Element
    {
        public T value;
        public int x;
        public int y;

        public Element(T _value, int _x, int _y)
        {
            value = _value;
            x = _x;
            y = _y;
        }
    }

    List<Element> m_elements;
    QuadTree<T>[] m_regions;
    int m_maxElemInCell;
    int m_sizeX;
    int m_sizeY;
    int m_x;
    int m_y;

    public QuadTree(int size, int maxElemInCell) : this(0, 0, size, size, maxElemInCell) { }

    public QuadTree(int sizeX, int sizeY, int maxElemInCell) : this(0, 0, sizeX, sizeY, maxElemInCell) { }

    public QuadTree(int x, int y, int sizeX, int sizeY, int maxElemInCell)
    {
        m_sizeX = sizeX;
        m_sizeY = sizeY;
        m_maxElemInCell = maxElemInCell;
        if (m_maxElemInCell < 1)
            m_maxElemInCell = 1;
        m_x = x;
        m_y = y;

        m_elements = new List<Element>();
    }

    public int GetSizeX()
    {
        return m_sizeX;
    }

    public int GetSizeY()
    {
        return m_sizeY;
    }

    public int GetX()
    {
        return m_x;
    }

    public int GetY()
    {
        return m_y;
    }

    public bool AddElement(int x, int y, T element)
    {
        if (!IsPositionOn(x, y))
            return false;

        if(m_elements != null)
        {
            m_elements.Add(new Element(element, x, y));
            if (m_elements.Count > m_maxElemInCell && m_sizeX > 1 && m_sizeY > 1)
                Split();
            return true;
        }

        var r = GetRegionAt(x, y);
        if (r != null)
            return r.AddElement(x, y, element);
        return false;
    }

    public bool RemoveElementAt(int x, int y, int index = 0)
    {
        if(m_elements != null)
        {
            for(int i = 0; i < m_elements.Count; i++)
            {
                if(m_elements[i].x == x && m_elements[i].y == y)
                {
                    if(index == 0)
                    {
                        m_elements.RemoveAt(i);
                        return true;
                    }
                    index--;
                }
            }
            return false;
        }
        var r = GetRegionAt(x, y);
        if (r != null)
        {
            bool removed = RemoveElementAt(x, y, index);
            if(removed)
            {
                int nbElement = GetNbElement();
                if (nbElement <= m_maxElemInCell)
                    Combine();
            }
            return removed;
        }
        return false;
    }

    public int GetNbElement()
    {
        if (m_elements != null)
            return m_elements.Count;
        int nb = 0;
        foreach (var r in m_regions)
            nb += r.GetNbElement();
        return nb;
    }

    public int GetNbElementAt(int x, int y)
    {
        if(m_elements != null)
        {
            int nb = 0;
            foreach(var e in m_elements)
                if (e.x == x && e.y == y)
                    nb++;
            return nb;
        }
        var r = GetRegionAt(x, y);
        if (r == null)
            return 0;
        return r.GetNbElementAt(x, y);
    }

    public T GetElementAt(int x, int y, int index = 0)
    {
        if(m_elements != null)
        {
            int i = 0;
            foreach(var e in m_elements)
            {
                if(e.x == x && e.y == y)
                {
                    if (i == index)
                        return e.value;
                    i++;
                }
            }
            return default(T);
        }
        var r = GetRegionAt(x, y);
        if (r == null)
            return default(T);
        return r.GetElementAt(x, y, index);
    }

    public bool IsPositionOn(int x, int y)
    {
        if (x < m_x || y < m_y || x >= m_x + m_sizeX || y >= m_y + m_sizeY)
            return false;
        return true;
    }

    public QuadTree<T> GetRegionAt(int x, int y)
    {
        if (m_elements != null)
            return this;

        foreach (var r in m_regions)
            if (r.IsPositionOn(x, y))
                return r;
        
        return null;
    }

    void Split()
    {
        if (m_elements == null || m_sizeX == 1 || m_sizeY == 1)
            return;

        int firstPartSizeX = m_sizeX / 2;
        int secondPartSizeX = m_sizeX - firstPartSizeX;
        int firstPartSizeY = m_sizeY / 2;
        int secondPartSizeY = m_sizeY - firstPartSizeY;

        m_regions = new QuadTree<T>[] {new QuadTree<T>(m_x, m_y, firstPartSizeX, firstPartSizeY, m_maxElemInCell),
                                       new QuadTree<T>(m_x + firstPartSizeX, m_y, secondPartSizeX, firstPartSizeY, m_maxElemInCell),
                                       new QuadTree<T>(m_x, m_y + firstPartSizeY, firstPartSizeX, secondPartSizeY, m_maxElemInCell),
                                       new QuadTree<T>(m_x + firstPartSizeX, m_y + firstPartSizeY, secondPartSizeX, secondPartSizeY, m_maxElemInCell)};

        foreach(var e in m_elements)
        {
            foreach(var r in m_regions)
            {
                if(r.IsPositionOn(e.x, e.y))
                {
                    r.AddElement(e.x, e.y, e.value);
                    break;
                }
            }
        }

        m_elements = null;
    }

    void Combine()
    {
        if (m_elements != null)
            return;

        m_elements = new List<Element>();
        foreach(var r in m_regions)
        {
            foreach (var e in r.m_elements)
                m_elements.Add(e);
        }
        m_regions = null;
    }
}