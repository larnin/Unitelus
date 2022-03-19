using Sirenix.OdinInspector;
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
    [MenuItem("Game/Item Editor")]
    public static void OpenWindow()
    {
        GetWindow<ItemEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        int nb = G.sys.items.GetItemCount();
        for(int i = 0; i < nb; i++)
        {
            var item = G.sys.items.GetItemFromIndex(i);
            tree.Add(item.nameID, item);
        }

        tree.Add("New item", new NewItemEditor(this));

        return tree;
    }

    public void ReloadItems()
    {
        G.sys.items.Reload();

        ForceMenuTreeRebuild();
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

        if(!asset.Init(m_name, G.sys.items.GetFreeUID()))
        {
            Debug.LogError("Unable to init the new Item");
            return;
        }

        AssetDatabase.CreateAsset(asset, "Assets/Resources/Items/" + m_name + ".asset");
        AssetDatabase.SaveAssets();

        m_tree.ReloadItems();
    }
}