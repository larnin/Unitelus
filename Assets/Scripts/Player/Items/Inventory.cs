using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class InventoryItem
{
    int m_itemUID;
    public int itemUID 
    {
        get { return m_itemUID; } 
        set
        {
            var type = G.sys.items.GetItem(value);
            if(type == null)
            {
                m_itemUID = -1;
                m_stack = 0;
                return;
            }
            m_itemUID = value;
        }
    }

    int m_stack;
    public int stack 
    { 
        get { return m_stack; } 
        set 
        { 
            if (value > 0)
            {
                m_stack = value;
                var type = G.sys.items.GetItem(m_itemUID);
                if (type != null)
                {
                    if (m_stack > type.maxStack)
                        m_stack = type.maxStack;
                }
                else m_stack = 0;
            }
            else m_stack = 0; 
        } 
    }

    public InventoryItem() : this(-1, 1) { }

    public InventoryItem(ItemType type, int _stack) : this(type.UID, _stack) { }

    public InventoryItem(int _itemUID, int _stack)
    {
        m_itemUID = _itemUID;
        stack = _stack;

        var type = G.sys.items.GetItem(m_itemUID);
        if(type == null)
        {
            m_itemUID = -1;
            m_stack = 0;
        }
        else if (m_stack > type.maxStack)
            m_stack = type.maxStack;
    }

    public void Set(InventoryItem item)
    {
        m_itemUID = item.m_itemUID;
        m_stack = item.m_stack;
    }

    public bool IsValid()
    {
        if (m_itemUID < 0)
            return false;

        var type = G.sys.items.GetItem(m_itemUID);
        return type != null;

    }

    //return the real added count
    //value can be negative to remove items
    public int AddStack(int value)
    {
        var type = G.sys.items.GetItem(m_itemUID);
        if(type == null)
        {
            m_stack = 0;
            return 0;
        }

        int oldValue = m_stack;

        m_stack += value;
        if (m_stack < 0)
            m_stack = 0;
        if (m_stack > type.maxStack)
            m_stack = type.maxStack;

        return m_stack - oldValue;
    }

    public void Reset()
    {
        m_itemUID = -1;
        m_stack = 0;
    }
}

public class Inventory
{
    List<InventoryItem> m_items = new List<InventoryItem>();

    public int size { get { return m_items.Count; } }

    public Inventory(int size)
    {
        for (int i = 0; i < size; i++)
            m_items.Add(new InventoryItem());
    }

    //return a list of removed items with the resize
    public List<InventoryItem> Resize(int size)
    {
        List<InventoryItem> removedItems = new List<InventoryItem>();

        if (size == m_items.Count)
            return removedItems;

        if (size > m_items.Count)
        {
            for (int i = m_items.Count; i < size; i++)
                m_items.Add(new InventoryItem());
            return removedItems;
        }

        int nbRemoved = m_items.Count - size;

        for(int i = 0; i < m_items.Count; i++)
        {
            var item = m_items[m_items.Count - 1];
            if (item.IsValid())
                removedItems.Add(item);
            m_items.RemoveAt(m_items.Count - 1);
        }

        //try to add items back
        for(int i = 0; i < removedItems.Count; i++)
        {
            int added = AddStack(removedItems[i].itemUID, removedItems[i].stack);
            removedItems[i].AddStack(-added);
            if (removedItems[i].stack == 0)
            {
                removedItems.RemoveAt(i);
                i--;
            }
        }

        return removedItems;
    }

    //return the real added count
    //value can be negative to remove items
    public int AddStack(ItemType item, int stack)
    {
        return AddStack(item.UID, stack);
    }

    public int AddStack(int itemUID, int stack)
    {
        int nbAdded = AddNotFullStack(itemUID, stack);
        stack -= nbAdded;
        if (stack <= 0)
            return nbAdded;

        int nbAdded2 = AddStackOnEmptySlot(itemUID, stack);

        return nbAdded + nbAdded2;
    }

    int AddNotFullStack(int itemUID, int stack)
    {
        int nbAdded = 0;
        for (int i = 0; i < m_items.Count; i++)
        {
            if(m_items[i].itemUID == itemUID)
            {
                nbAdded += m_items[i].AddStack(stack - nbAdded);
                if (m_items[i].stack == 0)
                    m_items[i].Reset();
                if (nbAdded == stack)
                    return nbAdded;
            }
        }

        return nbAdded;
    }

    int AddStackOnEmptySlot(int itemUID, int stack)
    {
        if (stack <= 0)
            return 0;

        var type = G.sys.items.GetItem(itemUID);
        if (type == null)
            return 0;

        int nbAdded = 0;

        for(int i = 0; i < m_items.Count; i++)
        {
            if (m_items[i].IsValid())
                continue;

            m_items[i].itemUID = itemUID;

            nbAdded += m_items[i].AddStack(stack);
            if (nbAdded == stack)
                return nbAdded;
        }

        return nbAdded;
    }

    public InventoryItem GetItemAt(int index)
    {
        if (index < 0 || index > m_items.Count)
        {
            Assert.IsTrue(false, "Invalid inventory index " + index);
            return new InventoryItem();
        }

        return m_items[index];
    }

    public void SetItemAt(int index, InventoryItem item)
    {
        if (index < 0 || index > m_items.Count)
        {
            Assert.IsTrue(false, "Invalid inventory index " + index);
            return;
        }
            
        m_items[index].Set(item);
    }

    public int GetTotalStack(ItemType type)
    {
        return GetTotalStack(type.UID);
    }

    public int GetTotalStack(int itemUID)
    {
        int stack = 0;

        for(int i = 0; i < m_items.Count; i++)
        {
            if (m_items[i].itemUID == itemUID)
                stack += m_items[i].stack;
        }

        return stack;
    }
}
