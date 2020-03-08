using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BlockRendererBase
{
    int m_id;

    public int id { get { return m_id; } }

    public BlockRendererBase(int id)
    {
        m_id = id;
    }

    public abstract RendererData Render(Vector3 pos, Vector3 scale, BlockNeighbors neighbors);
}
