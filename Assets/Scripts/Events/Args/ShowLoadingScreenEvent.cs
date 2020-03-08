using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ShowLoadingScreenEvent
{
    public bool start;

    public ShowLoadingScreenEvent(bool _start)
    {
        start = _start;
    }
}