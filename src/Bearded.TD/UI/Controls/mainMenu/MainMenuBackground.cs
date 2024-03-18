using Bearded.TD.Rendering;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuBackground : CompositeControl
{
    public MainMenuBackground()
    {
        Add(new CaveBackground().Anchor(
            a => a.Right(relativePercentage: 0.96)));

        const float turretSpriteEdgeMarginPercentage = -0.05f;
        const float turretScale = 0.7f;

        Add(new Sprite
            {
                SpriteId = Constants.Content.CoreUI.MainMenu.Turret,
                Layout = new SpriteLayout(
                    default,
                    SpriteSize.ContainInFrame,
                    frameAlign: (0, 1),
                    spriteAlign: (0, 1),
                    scale: new Vector2(1, -1) * turretScale
                ),
            }.Anchor(a => a
                .Right(relativePercentage: 0.8)
                .Left(relativePercentage: turretSpriteEdgeMarginPercentage)
                .Top(relativePercentage: 0.22)
                .Bottom(relativePercentage: 1 - turretSpriteEdgeMarginPercentage)
            )
        );
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

sealed class CaveBackground : Control
{
    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
