using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ChunkRendererPassBase
{
    public BlockData block;

    public abstract RendererData[] Render(Chunk c, int x, int y, float scaleX, float scaleY, float scaleZ);
}