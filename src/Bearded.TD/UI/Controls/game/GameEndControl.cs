using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class GameEndControl : CompositeControl
{
    public event VoidEventHandler? ReturnToMainMenuButtonClicked;

    public GameEndControl(UIFactories factories, string text)
    {
        Add(new BackgroundBox { Color = .5f * Color.Black });
        Add(new Label { Color = Color.PaleVioletRed, FontSize = 24, Text = text }.Anchor(a => a
            .Bottom(margin: 32)));
        Add(factories.Button("back to main menu")
            .Anchor(a => a
                .Bottom(margin: 4, height: Constants.UI.Button.Height)
                .Left(relativePercentage: 0.5, margin: -0.5f * Constants.UI.Button.Width))
            .Subscribe(btn => btn.Clicked += _ => ReturnToMainMenuButtonClicked?.Invoke()));
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
