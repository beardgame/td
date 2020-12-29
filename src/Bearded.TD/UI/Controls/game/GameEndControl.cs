using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using static Bearded.TD.UI.Factories.LegacyDefault;

namespace Bearded.TD.UI.Controls
{
    sealed class GameEndControl : CompositeControl
    {
        public event VoidEventHandler? ReturnToMainMenuButtonClicked;

        public GameEndControl(string text)
        {
            Add(new BackgroundBox { Color = .5f * Color.Black });
            Add(new Label { Color = Color.PaleVioletRed, FontSize = 24, Text = text }.Anchor(a => a
                .Bottom(margin: 32)));
            Add(Button("back to main menu", 16)
                .Anchor(a => a
                    .Bottom(margin: 4, height: 24)
                    .Left(margin: 4)
                    .Right(margin: 4))
                .Subscribe(btn => btn.Clicked += _ => ReturnToMainMenuButtonClicked?.Invoke()));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
