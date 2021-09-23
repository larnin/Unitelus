﻿using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlockEditor : OdinMenuEditorWindow
{
    [MenuItem("My Game/Block Editor")]
    public static void OpenWindow()
    {
        GetWindow<BlockEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        var allBlocks = Enum.GetValues(typeof(BlockID));

        foreach(BlockID b in allBlocks)
        {
            var block = new OneBlockEditor(b);
            tree.Add(b.ToString(), block);
        }

        return tree;
    }
}

[Serializable]
public class OneBlockEditor
{
    BlockID m_id = BlockID.INVALID;
    public BlockTypeBase block = null;

    public int m_editUVIndex = 0;

    public BlockID ID() { return m_id; }

    public OneBlockEditor(BlockID id)
    {
        m_id = id;

        var name = "Blocks/" + id.ToString();

        var file = Resources.Load<ScriptableObject>(name);
        if (file == null)
            return;

        var blockType = file as BlockTypeBase;
        if (blockType == null)
            return;
        if (blockType.id != id)
            Debug.LogWarning("The block " + name + " have an inconsistent id");

        block = blockType;
    }
}

public class OneBlockEditorDrawer : OdinValueDrawer<OneBlockEditor>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var value = ValueEntry.SmartValue;
        if (value.block == null)
        {
            DrawNoAsset();
            return;
        }

        DrawDestroy();

        switch(value.block.type)
        {
            case BlockType.Cube:
                DrawBlockTypeCube();
                break;
            case BlockType.Empty:
                DrawBlockTypeEmpty();
                break;
            case BlockType.Smoothed:
                DrawBlockTypeSmoothed();
                break;
            default:
                DrawUncompatypeType();
                break;
        }

        DrawSave();
    }

    int m_newAssetIndex = 0;

    void DrawNoAsset()
    {
        var baseType = typeof(BlockTypeBase);
        var assembly = baseType.Assembly;

        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));

        EditorGUILayout.LabelField("No block type instanciated for this block");

        EditorGUILayout.BeginHorizontal();

        GUIContent[] contents = new GUIContent[types.Count()];
        int index = 0;
        foreach(var t in types)
        {
            contents[index] = new GUIContent(t.Name);
            index++;
        }
        
        m_newAssetIndex = EditorGUILayout.Popup(m_newAssetIndex, contents);
        if(GUILayout.Button("Create"))
        {
            var t = types.ElementAt(m_newAssetIndex);

            BlockTypeBase asset = ScriptableObject.CreateInstance(t) as BlockTypeBase;
            if(asset == null)
                Debug.LogError("Unable to create an asset of type " + t.Name);
            else
            {
                var value = ValueEntry.SmartValue;
                asset.id = value.ID();
                if(!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                if(!AssetDatabase.IsValidFolder("Assets/Resources/Blocks"))
                    AssetDatabase.CreateFolder("Assets/Resources", "Blocks");
                AssetDatabase.CreateAsset(asset, "Assets/Resources/Blocks/" + value.ID().ToString() + ".asset");
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndHorizontal();
    }
    

    void DrawDestroy()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Want an another block type ?");

        if (GUILayout.Button("Destroy"))
        {
            var value = ValueEntry.SmartValue;
            if (EditorUtility.DisplayDialog("Destroy the block " + value.ID().ToString(), "Are you sure to destroy the block " + value.ID().ToString() + "?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset("Assets/Resources/Blocks/" + value.ID().ToString() + ".asset");
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawSave()
    {
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Save"))
        {
            AssetDatabase.SaveAssets();
        }
    }

    void DrawBlockTypeEmpty()
    {
        EditorGUILayout.HelpBox("Nothing to edit here, this block type is invisible and have no effect", MessageType.Info, true);
    }

    void DrawBlockTypeCube()
    {
        var value = ValueEntry.SmartValue;
        var block = value.block as BlockTypeCube;
        DrawBlockUV(block.m_UV, ref value.m_editUVIndex);

    }

    void DrawBlockTypeSmoothed()
    {
        var value = ValueEntry.SmartValue;
        var block = value.block as BlockTypeSmoothed;
        DrawBlockUV(block.m_UV, ref value.m_editUVIndex);

    }

    void DrawUncompatypeType()
    {
        EditorGUILayout.HelpBox("No editor defined for this block type!", MessageType.Error, true);
    }

    void DrawBlockUV(BlockUV data, ref int editIndex)
    {
        var values = Enum.GetValues(typeof(BlockUVType)).Cast<BlockUVType>();
        GUIContent[] contents = new GUIContent[values.Count()];
        int index = 0;
        foreach (var v in values)
        {
            contents[index] = new GUIContent(v.ToString());
            index++;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Block UV Type");
        data.uvType = (BlockUVType)EditorGUILayout.Popup((int)data.uvType, contents);
        EditorGUILayout.EndHorizontal();

        //update uvs quads size
        int newLength = data.uvs == null ? 0 : data.uvs.Length;
        if (data.uvType == BlockUVType.SimpleFace)
            newLength = 1;
        else if (data.uvType == BlockUVType.UpDownSide)
            newLength = 3;
        else if (data.uvType == BlockUVType.FullUnfold)
            newLength = 6;
        else
        {
            data.uvType = BlockUVType.SimpleFace;
            return;
        }
        if (data.uvs == null)
        {
            data.uvs = new Rect[newLength];
            for (int i = 0; i < newLength; i++)
                data.uvs[i] = new Rect(0, 0, 1, 1);
        }
        else if (newLength != data.uvs.Length)
        {
            var newArray = new Rect[newLength];
            for (int i = 0; i < newLength; i++)
            {
                if (newLength < data.uvs.Length)
                    newArray[i] = data.uvs[i];
                else newArray[i] = new Rect(0, 0, 1, 1);
            }
            data.uvs = newArray;
        }

        //display uvs
        switch (data.uvType)
        {
            case BlockUVType.SimpleFace:
                data.uvs[0] = EditorGUILayout.RectField("Face", data.uvs[0]);
                break;
            case BlockUVType.UpDownSide:
                data.uvs[0] = EditorGUILayout.RectField("Up", data.uvs[(int)BlockUV.Face.Up]);
                data.uvs[0] = EditorGUILayout.RectField("Down", data.uvs[(int)BlockUV.Face.Down]);
                data.uvs[0] = EditorGUILayout.RectField("Side", data.uvs[(int)BlockUV.Face.Front]);
                break;
            case BlockUVType.FullUnfold:
                for (int i = 0; i < data.uvs.Length; i++)
                    data.uvs[i] = EditorGUILayout.RectField(((BlockUV.Face)i).ToString(), data.uvs[i]);
                break;
        }
    }
}

public class BlockTypeDrawer<T> : OdinValueDrawer<T> where T : BlockTypeBase
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        EditorGUILayout.HelpBox("You must not edit the blocks assets directly.\nOpen the block windows instead", MessageType.Info, true);
        if(GUILayout.Button("Open blocks window"))
        {
            BlockEditor.OpenWindow();
        }
    }
}