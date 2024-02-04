using System;
using Bearded.Graphics;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

[Obsolete("Use a label with bindings instead")]
sealed class DynamicLabel : Label
{
    private readonly Func<string> stringProvider;
    private readonly Func<Color>? colorProvider;

    public DynamicLabel(Func<string> stringProvider, Func<Color>? colorProvider = null)
    {
        this.stringProvider = stringProvider;
        this.colorProvider = colorProvider;
    }

    protected override void RenderStronglyTyped(IRendererRouter r)
    {
        Text = stringProvider();
        Color = colorProvider?.Invoke() ?? Color;
        r.Render(this);
    }
}
