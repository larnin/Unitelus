using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SkillTypeList
{
    Dictionary<int, SkillType> m_skills = new Dictionary<int, SkillType>();

    public SkillTypeList()
    {
        Reload();
    }

    public void Reload()
    {
        m_skills.Clear();

        var skills = Resources.LoadAll<ScriptableObject>("Skills");

        foreach (var s in skills)
        {
            SkillType skill = s as SkillType;
            if (skill == null)
                continue;

            if (m_skills.ContainsKey(skill.UID))
            {
                Debug.LogError("Error when loading the skill " + skill.nameID + " - A skill with the UID " + skill.UID + " already exist");
                continue;
            }

            m_skills.Add(skill.UID, skill);
        }

        Debug.Log("Loaded " + m_skills.Count + " skills");
    }

    public SkillType GetSkill(int ID)
    {
        SkillType skill = null;
        m_skills.TryGetValue(ID, out skill);
        return skill;
    }

    public SkillType GetSkillFromName(string name)
    {
        foreach (var skill in m_skills)
        {
            if (skill.Value.nameID == name)
                return skill.Value;
        }

        return null;
    }

    public int GetSkillCount()
    {
        return m_skills.Count;
    }

    public SkillType GetSkillFromIndex(int index)
    {
        return m_skills.ElementAt(index).Value;
    }

    public int GetFreeUID()
    {
        int nextUID = 1;
        foreach (var skill in m_skills)
        {
            if (skill.Key >= nextUID)
                nextUID = skill.Key + 1;
        }

        return nextUID;
    }
}