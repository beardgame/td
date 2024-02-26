using Bearded.Graphics;
using Bearded.TD.Content;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class MainMenuBackgroundRenderer(ContentManager content, IDrawableRenderers renderers)
    : IRenderer<MainMenuBackground>
{
    public void Render(MainMenuBackground control)
    {
        var id = Constants.Content.CoreUI.MainMenu.Background;
        var material = content.ResolveMaterial(id);

        var frame = control.Frame;

        var drawer = Custom.MainMenuBackground.GetOrCreate(material, renderers);

        drawer.DrawRectangle(
            ((Vector2)frame.TopLeft).WithZ(),
            (Vector2)frame.Size,
            Color.White
        );
    }
}
