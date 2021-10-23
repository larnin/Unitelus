using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockTypeList
{
    static BlockTypeList m_instance = null;
    public static BlockTypeList instance
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
            m_instance = new BlockTypeList();
    }

    List<BlockTypeBase> m_blocks = new List<BlockTypeBase>();

    BlockTypeList()
    {
        var allBlocks = Enum.GetValues(typeof(BlockID));
        foreach(BlockID b in allBlocks)
        {
            var name = "Blocks/" + b.ToString();

            var file = Resources.Load<ScriptableObject>(name);
            if(file == null)
            {
                DebugConsole.Error("Unable to find Block " + name);
                continue;
            }

            var blockType = file as BlockTypeBase;
            if(blockType == null)
            {
                DebugConsole.Error("Load Block " + name + " have an uncompatible type !");
                continue;
            }
            if(blockType.id != b)
                DebugConsole.Warning("The block " + name + " have an inconsistent id");

            Set(b, blockType);
        }
    }

    public void Set(BlockID id, BlockTypeBase block)
    {
        int index = (int)id;

        while (index >= m_blocks.Count)
            m_blocks.Add(null);

        m_blocks[index] = block;
    }

    public BlockTypeBase Get(BlockID id)
    {
        int index = (int)id;

        Debug.Assert(m_blocks[0] != null);
        if (index >= m_blocks.Count)
        {
            Debug.Assert(false);
            return m_blocks[0];
        }
        if(m_blocks[index] == null)
        {
            Debug.Assert(false);
            return m_blocks[0];
        }
        return m_blocks[index];
    }
}
