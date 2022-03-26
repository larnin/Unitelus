using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class FxBehaviourNoCollision : FxBehaviourBase
{
    protected override void OnStart()
    {
        pos = casterPos;
        rot = casterRot;
    }

    protected override void OnUpdate()
    {
        pos = casterPos;
        rot = casterRot;
    }
}
