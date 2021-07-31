﻿using UnityEngine;
using System.Collections;


class AddScreenShakeEvent
{
    public ScreenShakeBase screenShake;
    public int resultID;

    public AddScreenShakeEvent(ScreenShakeBase _screenShake)
    {
        screenShake = _screenShake;
    }
}

class StopScreenShakeEvent
{
    public int ID;

    public StopScreenShakeEvent(int id)
    {
        ID = id;
    }
}

class StopAllScreenShakeEvent { }
