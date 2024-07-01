using Bearded.Graphics.Rendering;
using Bearded.Graphics.Shading;

namespace Bearded.TD.Rendering;

abstract class RendererDecorator(IRenderer renderer) : IRenderer
{
    public virtual void Dispose()
        => renderer.Dispose();

    public virtual void SetShaderProgram(ShaderProgram program)
        => renderer.SetShaderProgram(program);

    public virtual void Render()
        => renderer.Render();
}
