using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Vertices;

public delegate TVertex CreateVertex<out TVertex, in TVertexData>(Vector3 position, Vector2 uv, TVertexData data)
    where TVertex : struct, IVertexData;
