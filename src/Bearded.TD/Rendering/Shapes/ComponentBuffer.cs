using System;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeComponentBuffer
{
    ShapeComponentIds AddComponent(ShapeComponentForDrawing component);
    ShapeComponentIds AddComponents(ReadOnlySpan<ShapeComponentForDrawing> components);
}

sealed class ComponentBuffer() : TextureBuffer<ShapeComponentForDrawing>(SizedInternalFormat.Rgba32ui), IShapeComponentBuffer
{
    public ShapeComponentIds AddComponents(ReadOnlySpan<ShapeComponentForDrawing> components)
        => (new ShapeComponentId((uint)Add(components)), components.Length);

    public ShapeComponentIds AddComponent(ShapeComponentForDrawing component)
        => (new ShapeComponentId((uint)Add(component)), 1);
}
