using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;

public class WorldEditor : OdinMenuEditorWindow
{
    WorldGeneratorSettings m_settings = null;
    double m_lastSaveTime;

    [MenuItem("My Game/World Editor")]
    public static void OpenWindow()
    {
        GetWindow<WorldEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        if(m_settings == null)
        {
            tree.Add("Settings", new EmptyWorldEditor(this));
        }
        else
        {
            tree.Add("General Settings", m_settings.main);
            tree.Add("Biomes", m_settings.biomes);
            tree.Add("Plain", m_settings.plain);
            tree.Add("Ocean", m_settings.ocean);
            tree.Add("Desert", m_settings.desert);
            tree.Add("Snow", m_settings.snow);
            tree.Add("Mountain", m_settings.mountain);
        }
        
        return tree;
    }

    protected override void OnEnable() 
    {
        base.OnEnable();
        LoadSettings();
        m_lastSaveTime = EditorApplication.timeSinceStartup;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EditorUtility.SetDirty(m_settings);
        AssetDatabase.SaveAssets();
    }

    void LoadSettings()
    {
        var file = AssetDatabase.LoadAssetAtPath("Assets/Resources/World/Settings.asset", typeof(ScriptableObject)) as WorldGeneratorSettings;
        if(file != null)
            m_settings = file;
    }

    public void CreateSettings()
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

    private void Update()
    {
        if(EditorApplication.timeSinceStartup - m_lastSaveTime > 30)
        {
            EditorUtility.SetDirty(m_settings);
            AssetDatabase.SaveAssets();
            m_lastSaveTime = EditorApplication.timeSinceStartup;
        }
    }
}

[Serializable]
public class EmptyWorldEditor
{
    WorldEditor m_editor = null;

    public EmptyWorldEditor(WorldEditor editor)
    {
        m_editor = editor;
    }

    [Button("Create settings")]
    void CreateSettings()
    {
        m_editor.CreateSettings();
        m_editor.ForceMenuTreeRebuild();
    }
}