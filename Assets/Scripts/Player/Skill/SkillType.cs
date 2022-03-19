using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SkillType : SerializedScriptableObject
{
    [SerializeField] string m_nameID = "";
    public string nameID { get { return m_nameID; } }

    [SerializeField] [DisplayAsString] int m_UID = 0;
    public int UID { get { return m_UID; } }
    public bool Init(string name, int UID)
    {
        if (m_nameID.Length != 0 || m_UID != 0)
        {
            Debug.LogError("You can't init an object that was already initialized");
            return false;
        }

        m_nameID = name;
        m_UID = UID;

        return true;
    }

    [SerializeField] SkillBehaviourBase m_skillBehaviour = new SkillBehaviourPassive();
    public SkillBehaviourBase skillBehaviour { get { return m_skillBehaviour; } }

    [SerializeField] List<SkillTypeStep> m_steps = new List<SkillTypeStep>();

}