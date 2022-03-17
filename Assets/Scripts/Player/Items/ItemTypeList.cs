using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemTypeList
{
    Dictionary<int, ItemType> m_items = new Dictionary<int, ItemType>();

    public ItemTypeList()
    {
        Reload();
    }

    public void Reload()
    {
        m_items.Clear();

        var items = Resources.LoadAll<ScriptableObject>("Items");

        foreach (var i in items)
        {
            ItemType item = i as ItemType;
            if (item == null)
                continue;

            if (m_items.ContainsKey(item.UID))
            {
                Debug.LogError("Error when loading the item " + item.nameID + " - An item with the UID " + item.UID + " already exist");
                continue;
            }

            m_items.Add(item.UID, item);
        }

        Debug.Log("Loaded " + m_items.Count + " items");
    }

    public ItemType GetItem(int ID)
    {
        ItemType item = null;
        m_items.TryGetValue(ID, out item);
        return item;
    }

    public ItemType GetItemFromName(string name)
    {
        foreach(var item in m_items)
        {
            if (item.Value.nameID == name)
                return item.Value;
        }

        return null;
    }

    public int GetItemCount()
    {
        return m_items.Count;
    }

    public ItemType GetItemFromIndex(int index)
    {
        return m_items.ElementAt(index).Value;
    }

    public int GetFreeUID()
    {
        int nextUID = 1;
        foreach (var item in m_items)
        {
            if (item.Key >= nextUID)
                nextUID = item.Key + 1;
        }

        return nextUID;
    }
}
