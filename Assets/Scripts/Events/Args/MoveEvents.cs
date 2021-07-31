using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CenterUpdatedEventInstant
{
    public Vector3 pos;

    public CenterUpdatedEventInstant(Vector3 _pos)
    {
        pos = _pos;
    }
}

public class CenterUpdatedEvent
{
    public Vector3 pos;

    public CenterUpdatedEvent(Vector3 _pos)
    {
        pos = _pos;
    }
}
