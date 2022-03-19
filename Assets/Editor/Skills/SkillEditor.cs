using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class SkillEditor : OdinMenuEditorWindow
{
    [MenuItem("Game/Skill Editor")]
    public static void OpenWindow()
    {
        GetWindow<SkillEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        int nb = G.sys.skills.GetSkillCount();
        for (int i = 0; i < nb; i++)
        {
            var skill = G.sys.skills.GetSkillFromIndex(i);
            tree.Add(skill.nameID, skill);
        }

        tree.Add("New item", new NewSkillEditor(this));

        return tree;
    }

    public void ReloadSkills()
    {
        G.sys.skills.Reload();

        ForceMenuTreeRebuild();
    }
}

public class NewSkillEditor
{
    [SerializeField] string m_name = "";

    SkillEditor m_tree = null;

    public NewSkillEditor(SkillEditor tree)
    {
        m_tree = tree;
    }

    [Button]
    void CreateItem()
    {
        if (m_name.Length == 0)
        {
            Debug.LogError("Can't create an item with an empty name");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Skills"))
            AssetDatabase.CreateFolder("Assets/Resources", "Skills");

        SkillType asset = ScriptableObject.CreateInstance(typeof(SkillType)) as SkillType;
        if (asset == null)
        {
            Debug.LogError("Can't create a SkillType");
            return;
        }

        if (!asset.Init(m_name, G.sys.skills.GetFreeUID()))
        {
            Debug.LogError("Unable to init the new Skill");
            return;
        }

        AssetDatabase.CreateAsset(asset, "Assets/Resources/Skills/" + m_name + ".asset");
        AssetDatabase.SaveAssets();

        m_tree.ReloadSkills();
    }
}