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
            if (m_instance == null)
                m_instance = new BlockTypeList();
            return m_instance;
        }
    }

    List<BlockTypeBase> m_blocks = new List<BlockTypeBase>();

    BlockTypeList()
    {
        Set(0, new BlockTypeEmpty());
    }

    public void Set(int id, BlockTypeBase block)
    {
        while (id >= m_blocks.Count)
            m_blocks.Add(null);

        m_blocks[id] = block;
    }

    public BlockTypeBase Get(int id)
    {
        Debug.Assert(m_blocks[0] != null);
        if (id >= m_blocks.Count)
        {
            Debug.Assert(false);
            return m_blocks[0];
        }
        if(m_blocks[id] == null)
        {
            Debug.Assert(false);
            return m_blocks[0];
        }
        return m_blocks[id];
    }
}
