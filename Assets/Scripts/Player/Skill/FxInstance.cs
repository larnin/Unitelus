using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace FxStructs
{
    public struct Sphere
    {
        public Vector3 pos;
        public float radius;

        public Sphere(Vector3 _pos, float _radius) { pos = _pos; radius = _radius; }
    }

    public struct Box
    {
        public Vector3 center;
        public Vector3 halfExtends;
        public Quaternion orientation;

        public Box(Vector3 _center, Vector3 _halfExtends) { center = _center; halfExtends = _halfExtends; orientation = Quaternion.identity; }
        public Box(Vector3 _center, Vector3 _halfExtends, Quaternion _orientation) { center = _center; halfExtends = _halfExtends; orientation = _orientation; }
    }

    public struct Capsule
    {
        public Vector3 pos1;
        public Vector3 pos2;
        public float radius;

        public Capsule(Vector3 _pos1, Vector3 _pos2, float _radius) { pos1 = _pos1; pos2 = _pos2; radius = _radius; }
    }

    public struct Ray
    {
        public Vector3 pos1;
        public Vector3 pos2;

        public Ray(Vector3 _pos1, Vector3 _pos2) { pos1 = _pos1; pos2 = _pos2; }
    }
}

public class FxInstance
{
    int m_ID;

    int m_skillID;
    int m_skillStep;

    FxBehaviourBase m_behaviour;
    GameObject m_object;
    VisualEffect m_effect;
    SkillTypeStep m_step;

    GameObject m_caster;
    Vector3 m_casterPos;
    Quaternion m_casterRot;
    
    GameObject m_target;
    Vector3 m_targetPos;
    Quaternion m_targetRot;

    List<FxStructs.Sphere> m_spheres;
    List<FxStructs.Box> m_boxes;
    List<FxStructs.Capsule> m_capsules;
    List<FxStructs.Ray> m_rays;

    Bounds m_bounds;
    bool m_boundsSet;

    bool m_needToStop;

    public FxInstance(int id, int skillID, int skillStep, GameObject parent = null)
    {
        m_ID = id;
        m_skillID = skillID;
        m_skillStep = skillStep;

        MakeObject(parent);
    }

    public void InitCaster(GameObject caster, Vector3 pos, Quaternion rot)
    {
        m_caster = caster;
        m_casterPos = pos;
        m_casterRot = rot;
    }

    public void InitTarget(GameObject target, Vector3 pos, Quaternion rot)
    {
        m_target = target;
        m_targetPos = pos;
        m_targetRot = rot;
    }


    void MakeObject(GameObject parent)
    {
        var skill = G.sys.skills.GetSkill(m_skillID);
        if (skill == null)
        {
            Debug.LogError("No skill with UID " + m_skillID);
            return;
        }
        var step = skill.Step(m_skillStep);
        if (step == null)
        {
            Debug.LogError("No step " + m_skillStep + " on skill " + skill.nameID);
            return;
        }
        if (step.fx == null || step.fxBehaviour == null)
        {
            Debug.LogError("You need a fx and a fxBehaviour to start the effect - Skill " + skill.nameID + " - step " + m_skillStep);
            return;
        }
        m_step = step;

        m_object = new GameObject("Skill " + skill.nameID);
        if (parent != null)
            m_object.transform.parent = parent.transform;

        m_effect = m_object.AddComponent<VisualEffect>();
        m_effect.visualEffectAsset = step.fx;

        m_behaviour = Utility.DeepClone(step.fxBehaviour);
    }

    public void OnInit()
    {
        if (NeedToDestroy())
            return;

        m_behaviour.Start(this);
    }

    public bool NeedToDestroy()
    {
        if (m_step == null || m_object == null || m_effect == null || m_behaviour == null)
            return true;

        return m_needToStop;
    }

    public void BeforeDestroy()
    {
        if(m_behaviour != null)
            m_behaviour.End(this);

        GameObject.Destroy(m_object);
    }

    public void Update()
    {
        if (NeedToDestroy())
            return;

        UpdateCasterAndTarget();

        ClearCollisions();

        m_behaviour.Update(this);

        UpdateCollisions();
        DrawDebug();
    }

    void UpdateCasterAndTarget()
    {
        if (m_caster == null || !m_step.stickToEntity)
            return;

        if (m_step.followBone)
        {
            //todo
            throw new NotImplementedException("Following a bone is not implemented yet");
        }

        m_casterPos = m_caster.transform.position;
        m_casterRot = m_caster.transform.rotation;

        if (m_target == null)
            return;

        m_targetPos = m_target.transform.position;
        m_targetRot = m_target.transform.rotation;
    }

    void ClearCollisions()
    {
        m_bounds = new Bounds();
        m_boundsSet = false;

        m_spheres.Clear();
        m_boxes.Clear();
        m_capsules.Clear();
        m_rays.Clear();
    }

    void UpdateCollisions()
    {
        if (!m_boundsSet)
            return;

        //todo !
    }

    //return the AABB around the real collision
    public Bounds GetBounds()
    {
        return m_bounds;
    }

    void AddBounds(Bounds bounds)
    {
        if (!m_boundsSet)
        {
            m_boundsSet = true;
            m_bounds = bounds;
            return;
        }

        m_bounds.Encapsulate(bounds);
    }

    public void CollideSphere(Vector3 pos, float radius)
    {
        AddBounds(new Bounds(pos, new Vector3(radius, radius, radius) * 2));

        m_spheres.Add(new FxStructs.Sphere(pos, radius));
    }

    public void CollideBox(Vector3 center, Vector3 halfExtents)
    {
        AddBounds(new Bounds(center, halfExtents * 2));

        m_boxes.Add(new FxStructs.Box(center, halfExtents));
    }

    public void CollideBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        Vector3[] points = new Vector3[]
        {
            orientation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z),
            orientation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z),
            orientation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
            orientation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z)
        };

        Vector3 max = Vector3.zero;

        for (int i = 0; i < 4; i++)
        {
            max.x = Mathf.Max(max.x, Mathf.Abs(points[i].x));
            max.y = Mathf.Max(max.y, Mathf.Abs(points[i].y));
            max.z = Mathf.Max(max.z, Mathf.Abs(points[i].z));
        }

        AddBounds(new Bounds(center, max * 2));

        m_boxes.Add(new FxStructs.Box(center, halfExtents, orientation));
    }

    public void CollideCapsule(Vector3 pos1, Vector3 pos2, float radius)
    {
        Vector3 size = pos2 - pos1;
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        size += new Vector3(radius, radius, radius) * 2;

        AddBounds(new Bounds((pos1 + pos2) / 2, size));

        m_capsules.Add(new FxStructs.Capsule(pos1, pos2, radius));
    }

    public void CollideRay(Vector3 pos1, Vector3 pos2)
    {
        Vector3 size = pos2 - pos1;
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));

        AddBounds(new Bounds((pos1 + pos2) / 2, size));

        m_rays.Add(new FxStructs.Ray(pos1, pos2));
    }

    public void DrawDebug()
    {
        if (!m_boundsSet)
            return;

        DebugDraw.CentredBox(m_bounds.center, m_bounds.size, Color.green);

        Color drawColor = Color.white;

        foreach (var sphere in m_spheres)
            DebugDraw.Sphere(sphere.pos, sphere.radius, drawColor);

        foreach (var box in m_boxes)
            DebugDraw.CenteredOrientedBox(box.center, box.halfExtends * 2, box.orientation, drawColor);

        foreach (var capsule in m_capsules)
            DebugDraw.OrientedCapsule(capsule.pos1, capsule.pos2, capsule.radius, drawColor);

        foreach (var ray in m_rays)
            DebugDraw.Line(ray.pos1, ray.pos2, drawColor);
    }

    public Vector3 casterPos { get { return m_casterPos; } }
    public Quaternion casterRot { get { return m_casterRot; } }

    public Vector3 targetPos { get { return m_targetPos; } }
    public Quaternion targetRot { get { return m_targetRot; } }

    public Vector3 pos
    {
        get
        {
            if (NeedToDestroy())
                return Vector3.zero;
            return m_object.transform.position;
        }
        set
        {
            if (NeedToDestroy())
                return;
            m_object.transform.position = value;
        }
    }

    public Quaternion rot
    {
        get
        {
            if (NeedToDestroy())
                return Quaternion.identity;
            return m_object.transform.rotation;
        }
        set
        {
            if (NeedToDestroy())
                return;
            m_object.transform.rotation = value;
        }
    }

    public void Stop()
    {
        m_needToStop = true;
    }
}