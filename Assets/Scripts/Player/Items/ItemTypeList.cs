using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemTypeList
{
    static ItemTypeList m_instance = null;
    public static ItemTypeList instance
    {
        get
        {
            InitInstance();
            return m_instance;
        }
    }

    public static void InitInstance()
    {
        if (m_instance == null)
            m_instance = new ItemTypeList();
    }

    Dictionary<int, ItemType> m_items = new Dictionary<int, ItemType>();

    ItemTypeList()
    {
        var items = Resources.LoadAll<ScriptableObject>("Items");

        foreach(var i in items)
        {
            ItemType item = i as ItemType;
            if (item == null)
                continue;

            if(m_items.ContainsKey(item.UID))
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
}
