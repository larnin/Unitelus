using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class LayerRendererPassBase
{
    public BlockData block;

    public abstract void Render(Chunk c, int x, int y, int z, float scaleX, float scaleY, float scaleZ, MeshParams<WorldVertexDefinition> meshParams);
}