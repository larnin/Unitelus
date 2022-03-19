using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public abstract class FxBehaviourBase
{
    public enum FxOrientation
    {
        Local,
        World,
    }

    [NonSerialized] protected GameObject m_caster;
    [NonSerialized] protected Vector3 m_casterPos;
    [NonSerialized] protected Quaternion m_casterRot;

    [NonSerialized] protected GameObject m_target;
    [NonSerialized] protected Vector3 m_targetPos;
    [NonSerialized] protected Quaternion m_targetRot;

    [NonSerialized] protected VisualEffect m_Fx;

    [SerializeField] bool m_stickToEntity;
    [ShowIf("m_stickToEntity")]
    [SerializeField] FxOrientation m_orientation;
    [ShowIf("m_stickToEntity")]
    [SerializeField] bool m_followBone;
    [ShowIf("@this.m_stickToEntity && this.m_followBone")]
    [SerializeField] string m_boneName;

    public void InitCaster(GameObject caster)
    {
        m_caster = caster;
        m_casterPos = m_caster.transform.position;
        m_casterRot = m_caster.transform.rotation;
    }

    public void InitCaster(GameObject caster, Vector3 pos, Quaternion rot)
    {
        m_caster = caster;
        m_casterPos = pos;
        m_casterRot = rot;
    }

    public void InitCaster(Vector3 pos, Quaternion rot)
    {
        m_caster = null;
        m_casterPos = pos;
        m_casterRot = rot;
    }

    public void InitTarget(GameObject target)
    {
        m_target = target;
        m_targetPos = m_target.transform.position;
        m_targetRot = m_target.transform.rotation;
    }

    public void InitTarget(Vector3 pos, Quaternion Rot)
    {
        m_target = null;
        m_targetPos = pos;
        m_targetRot = Rot;
    }

    public void InitFx(VisualEffect fx)
    {
        m_Fx = fx;
    }

    public void Update()
    {
        UpdateCaster();
        UpdateTarget();
        UpdateFx();

        OnUpdate();
    }
    protected virtual void OnUpdate() { }

    void UpdateCaster()
    {
        if (m_caster == null || !m_stickToEntity)
            return;

        if(m_followBone)
        {
            //todo
            throw new NotImplementedException("Following a bone is not implemented yet");
        }

        m_casterPos = m_caster.transform.position;
        m_casterRot = m_caster.transform.rotation;
    }

    void UpdateTarget()
    {
        if (m_target == null)
            return;

        m_targetPos = m_target.transform.position;
        m_targetRot = m_target.transform.rotation;
    }

    public virtual void UpdateFx()
    {
        m_Fx.transform.position = m_casterPos;
        m_Fx.transform.rotation = m_casterRot;
    }

    //return the AABB around the real collision
    public abstract Bounds GetBounds();

    //entity are cylinders (or multy cylinders)
    public abstract bool CanHit(Vector3 pos, Vector3 radius, Vector3 height);

    public abstract void DebugDrawCollision();
}
