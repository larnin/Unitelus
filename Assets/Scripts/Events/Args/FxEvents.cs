using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class StartFxEvent
{
    public int skillUID;
    public int skillStep;

    public GameObject caster = null;
    public Vector3 casterPos = Vector3.zero;
    public Quaternion casterRot = Quaternion.identity;

    public GameObject target = null;
    public Vector3 targetPos = Vector3.zero;
    public Quaternion targetRot = Quaternion.identity;

    public int outFxID = -1;

    public StartFxEvent(int _skillUID, int _skillStep)
    {
        skillUID = _skillUID;
        skillStep = _skillStep;
    }

    public void InitCaster(GameObject _caster)
    {
        caster = _caster;
        casterPos = caster.transform.position;
        casterRot = caster.transform.rotation;
    }

    public void InitCaster(GameObject _caster, Vector3 _pos, Quaternion _rot)
    {
        caster = _caster;
        casterPos = _pos;
        casterRot = _rot;
    }

    public void InitCaster(Vector3 _pos, Quaternion _rot)
    {
        caster = null;
        casterPos = _pos;
        casterRot = _rot;
    }

    public void InitTarget(GameObject _target)
    {
        target = _target;
        targetPos = target.transform.position;
        targetRot = target.transform.rotation;
    }

    public void InitTarget(Vector3 _pos, Quaternion _rot)
    {
        target = null;
        targetPos = _pos;
        targetRot = _rot;
    }
}

class StopFxEvent
{
    public int fxID;

    public StopFxEvent(int _id)
    {
        fxID = _id;
    }
}
