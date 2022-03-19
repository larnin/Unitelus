using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FxBehaviourNoCollision : FxBehaviourBase
{
    public override bool CanHit(Vector3 pos, Vector3 radius, Vector3 height)
    {
        return false;
    }

    public override Bounds GetBounds()
    {
        return new Bounds(new Vector3(-100000, -100000, -100000), new Vector3(0, 0, 0));
    }

    public override void DebugDrawCollision()
    {

    }

}
