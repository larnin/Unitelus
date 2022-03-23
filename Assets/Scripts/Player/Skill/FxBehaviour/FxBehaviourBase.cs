using Sirenix.OdinInspector;
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

    [NonSerialized] private List<FxStructs.Sphere> m_spheres;
    [NonSerialized] private List<FxStructs.Box> m_boxes;
    [NonSerialized] private List<FxStructs.Capsule> m_capsules;
    [NonSerialized] private List<FxStructs.Ray> m_rays;

    [NonSerialized] private Bounds m_bounds;
    [NonSerialized] private bool m_boundsSet;

    [SerializeField] bool m_bCollideWithWorld;
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

        ClearCollisions();

        OnUpdate();

        UpdateCollisions();
        DrawDebug();
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
        if(!m_boundsSet)
        {
            m_boundsSet = true;
            m_bounds = bounds;
            return;
        }

        m_bounds.Encapsulate(bounds);
    }

    public void CollideFrameSphere(Vector3 pos, float radius)
    {
        AddBounds(new Bounds(pos, new Vector3(radius, radius, radius) * 2));

        m_spheres.Add(new FxStructs.Sphere(pos, radius));
    }

    public void CollideFrameBox(Vector3 center, Vector3 halfExtents)
    {
        AddBounds(new Bounds(center, halfExtents * 2));

        m_boxes.Add(new FxStructs.Box(center, halfExtents));
    }

    public void CollideFrameBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        Vector3[] points = new Vector3[]
        {
            orientation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z),
            orientation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z),
            orientation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
            orientation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z)
        };

        Vector3 max = Vector3.zero;

        for(int i = 0; i < 4; i++)
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
}
