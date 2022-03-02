﻿using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ItemEditor : OdinMenuEditorWindow
{
    [MenuItem("My Game/Item Editor")]
    public static void OpenWindow()
    {
        GetWindow<ItemEditor>().Show();
    }

    List<ItemType> m_items = new List<ItemType>();

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        var assets = AssetDatabase.FindAssets("*", new string[] { "Assets/Resources/Items/" });

        m_items.Clear();

        foreach (var guid in assets)
        {
            var item = LoadItem(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null)
                continue;
            m_items.Add(item);
            tree.Add(item.nameID, item);
        }

        tree.Add("New item", new NewItemEditor(this));

        return tree;
    }

    ItemType LoadItem(string path)
    {
        var file = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
        if (file == null)
            return null;

        var item = file as ItemType;
        return item;
    }

    public void ReloadItems()
    {
        ForceMenuTreeRebuild();
    }

    public int GetFreeUID()
    {
        int nextUID = 1;
        for(int i = 0; i < m_items.Count; i++)
        {
            if(m_items[i].UID == nextUID)
            {
                nextUID++;
                i = -1;
            }
        }

        return nextUID;
    }
}

public class NewItemEditor
{
    [SerializeField] string m_name = "";

    ItemEditor m_tree = null;

    public NewItemEditor(ItemEditor tree)
    {
        m_tree = tree;
    }

    [Button]
    void CreateItem()
    {
        if(m_name.Length == 0)
        {
            Debug.LogError("Can't create an item with an empty name");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Items"))
            AssetDatabase.CreateFolder("Assets/Resources", "Items");

        ItemType asset = ScriptableObject.CreateInstance(typeof(ItemType)) as ItemType;
        if(asset == null)
        {
            Debug.LogError("Can't create an ItemType");
            return;
        }

        if(!asset.Init(m_name, m_tree.GetFreeUID()))
        {
            Debug.LogError("Unable to init the new Item");
            return;
        }

        AssetDatabase.CreateAsset(asset, "Assets/Resources/Items/" + m_name + ".asset");
        AssetDatabase.SaveAssets();

        m_tree.ReloadItems();
    }
}