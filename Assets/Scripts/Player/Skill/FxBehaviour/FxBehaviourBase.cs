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
    [NonSerialized] FxInstance m_instance;

    public void Start(FxInstance instance)
    {
        m_instance = instance;
        OnStart();
    }
    protected virtual void OnStart() { }

    public void Update(FxInstance instance)
    {
        m_instance = instance;
        OnUpdate();
    }
    protected virtual void OnUpdate() { }

    public void End(FxInstance instance)
    {
        m_instance = instance;
        OnEnd();
    }
    protected virtual void OnEnd() { }

    protected void CollideSphere(Vector3 pos, float radius)
    {
        if (m_instance != null)
            m_instance.CollideSphere(pos, radius);
    }

    protected void CollideBox(Vector3 center, Vector3 halfExtents)
    {
        if (m_instance != null)
            m_instance.CollideBox(center, halfExtents);
    }

    protected void CollideBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        if (m_instance != null)
            CollideBox(center, halfExtents, orientation);
    }

    protected void CollideCapsule(Vector3 pos1, Vector3 pos2, float radius)
    {
        if (m_instance != null)
            m_instance.CollideCapsule(pos1, pos2, radius);
    }

    protected void CollideRay(Vector3 pos1, Vector3 pos2)
    {
        if (m_instance != null)
            m_instance.CollideRay(pos1, pos2);
    }

    protected Vector3 casterPos
    {
        get
        {
            if (m_instance == null)
                return Vector3.zero;
            return m_instance.casterPos;
        }
    }

    protected Quaternion casterRot 
    { 
        get 
        {
            if (m_instance == null)
                return Quaternion.identity;
            return m_instance.casterRot; 
        } 
    }

    protected Vector3 targetPos 
    { 
        get 
        {
            if (m_instance == null)
                return Vector3.zero;
            return m_instance.targetPos; 
        } 
    }

    protected Quaternion targetRot 
    {
        get 
        {
            if (m_instance == null)
                return Quaternion.identity;
            return m_instance.targetRot; 
        } 
    }

    protected Vector3 pos
    {
        get
        {
            if (m_instance == null)
                return Vector3.zero;
            return m_instance.pos;
        }
        set
        {
            if (m_instance == null)
                return;
            m_instance.pos = value;
        }
    }

    protected Quaternion rot
    {
        get
        {
            if (m_instance == null)
                return Quaternion.identity;
            return m_instance.rot;
        }
        set
        {
            if (m_instance == null)
                return;
            m_instance.rot = value;
        }
    }

    protected void Stop()
    {
        if (m_instance != null)
            m_instance.Stop();
    }
}
