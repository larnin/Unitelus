using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BlockRendererBase
{
    [SerializeField] int m_id;

    public int id { get { return m_id; } }

    public BlockRendererBase(int id)
    {
        m_id = id;
    }

    public abstract void Render(Vector3 pos, MatrixView<BlockData> neighbors, MeshParams<WorldVertexDefinition> meshParams);
}
