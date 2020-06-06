using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IsChunkRenderedEvent
{
    public int x;
    public int z;
    public bool rendered;

    public IsChunkRenderedEvent(int _x, int _z)
    {
        x = _x;
        z = _z;
        rendered = false;
    }
}
