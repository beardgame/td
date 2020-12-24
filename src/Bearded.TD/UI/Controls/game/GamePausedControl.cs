using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.UI.Factories.LegacyDefault;

namespace Bearded.TD.UI.Controls
{
    sealed class GamePausedControl : CompositeControl
    {
        public event VoidEventHandler? ResumeGameButtonClicked;
        public event VoidEventHandler? ReturnToMainMenuButtonClicked;

        public GamePausedControl()
        {
            Add(new BackgroundBox { Color = .5f * Color.Black });
            Add(new Label { Color = Color.SpringGreen, FontSize = 24, Text = "Game menu" }.Anchor(a => a
                .Bottom(margin: 64)));
            
            Add(Button("resume", 16)
                .Anchor(a => a
                    .Bottom(margin: 32, height: 24)
                    .Left(margin: 4)
                    .Right(margin: 4))
                .Subscribe(btn => btn.Clicked += _ => ResumeGameButtonClicked?.Invoke()));
            Add(Button("exit", 16)
                .Anchor(a => a
                    .Bottom(margin: 4, height: 24)
                    .Left(margin: 4)
                    .Right(margin: 4))
                .Subscribe(btn => btn.Clicked += _ => ReturnToMainMenuButtonClicked?.Invoke()));
        }
    }
}
