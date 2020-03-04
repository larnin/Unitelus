using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BlockRendererBase
{
    public abstract RendererData Render(Vector3 pos, Vector3 scale, BlockNeighbors neighbors);
}
