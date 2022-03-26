using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class SkillTypeStep
{
    [SerializeField] VisualEffectAsset m_fx;
    public VisualEffectAsset fx { get { return m_fx; } }

    [SerializeField] FxBehaviourBase m_fxBehaviour;
    public FxBehaviourBase fxBehaviour { get { return m_fxBehaviour; } }

    public enum FxOrientation
    {
        Local,
        World,
    }

    [BoxGroup("Caster")]
    [SerializeField] bool m_stickToEntity;
    [ShowIf("m_stickToEntity")] [BoxGroup("Caster")]
    [SerializeField] FxOrientation m_orientation;
    [ShowIf("m_stickToEntity")] [BoxGroup("Caster")]
    [SerializeField] bool m_followBone;
    [ShowIf("@this.m_stickToEntity && this.m_followBone")] [BoxGroup("Caster")]
    [SerializeField] string m_boneName;

    public bool stickToEntity { get { return m_stickToEntity; } }
    public FxOrientation orientation { get { return m_orientation; } }
    public bool followBone { get { return m_followBone; } }
    public string boneName { get { return m_boneName; } }
}
