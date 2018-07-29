using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameOverControl : CompositeControl
    {
        public event VoidEventHandler ReturnToMainMenuButtonClicked;

        public GameOverControl()
        {
            Add(new BackgroundBox { Color = .5f * Color.Black });
            Add(new Label { Color = Color.PaleVioletRed, FontSize = 24, Text = "oh dear, u ded" }.Anchor(a => a
                .Bottom(margin: 32)));
            Add(new Button { new Label { FontSize = 16, Text = "back to main menu" }}
                .Anchor(a => a
                    .Bottom(margin: 4, height: 24)
                    .Left(margin: 4)
                    .Right(margin: 4))
                .Subscribe(btn => btn.Clicked += () => ReturnToMainMenuButtonClicked?.Invoke()));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
