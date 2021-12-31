using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuadTreeInt<T>
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
    QuadTreeInt<T>[] m_regions;
    int m_maxElemInCell;
    int m_sizeX;
    int m_sizeY;
    int m_x;
    int m_y;

    public QuadTreeInt(int size, int maxElemInCell) : this(0, 0, size, size, maxElemInCell) { }

    public QuadTreeInt(int sizeX, int sizeY, int maxElemInCell) : this(0, 0, sizeX, sizeY, maxElemInCell) { }

    public QuadTreeInt(int x, int y, int sizeX, int sizeY, int maxElemInCell)
    {
        m_sizeX = sizeX;
        m_sizeY = sizeY;
        m_maxElemInCell = maxElemInCell;
        if (m_maxElemInCell < 1)
            m_maxElemInCell = 1;
        m_x = x;
        m_y = y;

        m_elements = new List<Element>(maxElemInCell);
    }

    public int GetSizeX()
    {
        return m_sizeX;
    }

    public int GetSizeY()
    {
        return m_sizeY;
    }

    public Vector2Int GetSize()
    {
        return new Vector2Int(m_sizeX, m_sizeY);
    }

    public int GetX()
    {
        return m_x;
    }

    public int GetY()
    {
        return m_y;
    }

    public Vector2Int GetPos()
    {
        return new Vector2Int(m_x, m_y);
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

    public int GetNbLocalElement()
    {
        if (m_elements != null)
            return m_elements.Count();
        return 0;
    }

    public T GetLocalElement(int index)
    {
        if (m_elements != null && index >= 0 && index < m_elements.Count())
            return m_elements[index].value;
        return default(T);
    }

    public Vector2Int GetLocalElementPosition(int index)
    {
        if (m_elements != null && index >= 0 && index < m_elements.Count())
            return new Vector2Int(m_elements[index].x, m_elements[index].y);
        return Vector2Int.zero;
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

    public QuadTreeInt<T> GetRegionAt(int x, int y)
    {
        if (m_elements != null)
            return this;

        foreach (var r in m_regions)
            if (r.IsPositionOn(x, y))
                return r;
        
        return null;
    }

    public List<QuadTreeInt<T>> GetRegionsInCircle(float x, float y, float radius)
    {
        List<QuadTreeInt<T>> result = new List<QuadTreeInt<T>>();
        GetRegionsInCircleNoAlloc(x, y, radius, result);
        return result;
    }

    public void GetRegionsInCircleNoAlloc(float x, float y, float radius, List<QuadTreeInt<T>> result)
    {
        result.Clear();
        GetRegionsInCircleNoAllocImpl(x, y, radius, result);
    }

    void GetRegionsInCircleNoAllocImpl(float x, float y, float radius, List<QuadTreeInt<T>> result)
    {
        float distX = x - m_x;
        float distY = y - m_y;
        if (x > m_x && x <= m_x + m_sizeX)
            distX = 0;
        else if (x > m_x + m_sizeX)
            distX -= m_sizeX;
        if (y > m_y && y <= m_y + m_sizeY)
            distY = 0;
        else if (y > m_y + m_sizeY)
            distY -= m_sizeY;

        float sqrDist = distX * distX + distY * distY;
        if(sqrDist <= radius)
        {
            if (m_elements != null)
                result.Add(this);
            else
            {
                foreach (var r in m_regions)
                    r.GetRegionsInCircleNoAllocImpl(x, y, radius, result);
            }
        }
    }

    public List<QuadTreeInt<T>> GetRegionsInRect(float x, float y, float sizeX, float sizeY)
    {
        List<QuadTreeInt<T>> result = new List<QuadTreeInt<T>>();
        GetRegionsInRectNoAlloc(x, y, sizeX, sizeY, result);
        return result;
    }

    public void GetRegionsInRectNoAlloc(float x, float y, float sizeX, float sizeY, List<QuadTreeInt<T>> result)
    {
        result.Clear();
        GetRegionsInRectNoAllocImpl(x, y, sizeX, sizeY, result);
    }

    void GetRegionsInRectNoAllocImpl(float x, float y, float sizeX, float sizeY, List<QuadTreeInt<T>> result)
    {
        if (x + sizeX < m_x || m_x + m_sizeX < x || y + sizeY < m_y || m_y + m_sizeY < y)
            return;

        if (m_elements != null)
            result.Add(this);
        else
        {
            foreach (var r in m_regions)
                r.GetRegionsInRectNoAllocImpl(x, y, sizeX, sizeY, result);
        }
    }

    public void Draw()
    {
        int y = 5;
        if(m_elements != null)
        {
            Color borderColor = new Color(0, 1, 0);
            Debug.DrawLine(new Vector3(m_x, y, m_y), new Vector3(m_x, y, m_y + m_sizeY), borderColor);
            Debug.DrawLine(new Vector3(m_x, y, m_y + m_sizeY), new Vector3(m_x + m_sizeX, y, m_y + m_sizeY), borderColor);
            Debug.DrawLine(new Vector3(m_x + m_sizeX, y, m_y + m_sizeY), new Vector3(m_x + m_sizeX, y, m_y), borderColor);
            Debug.DrawLine(new Vector3(m_x + m_sizeX, y, m_y), new Vector3(m_x, y, m_y), borderColor);

            Color elementColor = new Color(1, 0, 0);
            foreach(var e in m_elements)
            {
                Debug.DrawLine(new Vector3(e.x, y, e.y), new Vector3(e.x + 1, y, e.y + 1), elementColor);
                Debug.DrawLine(new Vector3(e.x, y, e.y + 1), new Vector3(e.x + 1, y, e.y), elementColor);
            }
        }
        else
        {
            foreach (var r in m_regions)
                r.Draw();
        }
    }

    void Split()
    {
        if (m_elements == null || m_sizeX == 1 || m_sizeY == 1)
            return;

        int firstPartSizeX = m_sizeX / 2;
        int secondPartSizeX = m_sizeX - firstPartSizeX;
        int firstPartSizeY = m_sizeY / 2;
        int secondPartSizeY = m_sizeY - firstPartSizeY;

        m_regions = new QuadTreeInt<T>[] {new QuadTreeInt<T>(m_x, m_y, firstPartSizeX, firstPartSizeY, m_maxElemInCell),
                                       new QuadTreeInt<T>(m_x + firstPartSizeX, m_y, secondPartSizeX, firstPartSizeY, m_maxElemInCell),
                                       new QuadTreeInt<T>(m_x, m_y + firstPartSizeY, firstPartSizeX, secondPartSizeY, m_maxElemInCell),
                                       new QuadTreeInt<T>(m_x + firstPartSizeX, m_y + firstPartSizeY, secondPartSizeX, secondPartSizeY, m_maxElemInCell)};

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