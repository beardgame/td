using System;
using Bearded.Graphics;
using Bearded.TD.Content;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class CaveBackgroundRenderer(ContentManager content, IDrawableRenderers renderers)
    : IRenderer<MainMenuBackground>
{
    public void Render(MainMenuBackground control)
    {
        var id = Constants.Content.CoreUI.MainMenu.CaveBackground;
        var material = content.ResolveMaterial(id);

        var frame = control.Frame;

        var drawer = Custom.MainMenuBackground.GetOrCreate(material, renderers);

        var center = frame.TopLeft + frame.Size / 2;
        var size = new Vector2d(Math.Max(frame.Size.X, frame.Size.Y * (16f / 9f)));

        drawer.DrawRectangle(
            (Vector3)(center - size / 2).WithZ(),
            (Vector2)size,
            new Color(0, 3, 13)
        );
    }
}
