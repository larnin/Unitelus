using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class WorldEditor : OdinMenuEditorWindow
{
    WorldGeneratorSettings m_settings = null;

    [MenuItem("My Game/World Editor")]
    public static void OpenWindow()
    {
        GetWindow<BlockEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        if(m_settings == null)
        {
            
        }
        else
        {

        }
        
        return tree;
    }

    protected override void OnEnable() 
    {
        base.OnEnable();
        LoadSettings();
    }

    void LoadSettings()
    {
        var name = "World/Settings";

        var file = Resources.Load<ScriptableObject>(name) as WorldGeneratorSettings;
        if(file != null)
            m_settings = file;
    }

    void CreateSettings()
    {
        WorldGeneratorSettings asset = ScriptableObject.CreateInstance<WorldGeneratorSettings>();
        m_settings = asset;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/World"))
            AssetDatabase.CreateFolder("Assets/Resources", "World");
        AssetDatabase.CreateAsset(asset, "Assets/Resources/World/Settings.asset");
        AssetDatabase.SaveAssets();
    }
}