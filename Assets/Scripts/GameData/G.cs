using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class G
{
    static G m_instance = null;

    public static G sys
    {
        get
        {
            if(m_instance == null)
                m_instance = new G();

            return m_instance;
        }
    }

    BlockTypeList m_blockTypeList = new BlockTypeList();
    public BlockTypeList blocks { get { return m_blockTypeList; } }

    ItemTypeList m_itemTypeList = new ItemTypeList();
    public ItemTypeList items { get { return m_itemTypeList; } }

    SkillTypeList m_skillTypeList = new SkillTypeList();
    public SkillTypeList skills { get { return m_skillTypeList; } }

    G()
    {
        
    }

    public static void Init()
    {
        if (m_instance == null)
            m_instance = new G();
    }

    public void Reload()
    {
        m_blockTypeList.Reload();
        m_itemTypeList.Reload();
        m_skillTypeList.Reload();
    }
}
