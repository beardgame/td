using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;

namespace Bearded.TD.Content.Models;

readonly struct SpriteParameters
{
    public UVRectangle UV { get; }
    public Vector2 BaseSize { get; }

    public SpriteParameters(UVRectangle uv, Vector2 baseSize)
    {
        UV = uv;
        BaseSize = baseSize;
    }
}