using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class GameMenuControl : OnTopCompositeControl
{
    public event VoidEventHandler? ResumeGameButtonClicked;
    public event VoidEventHandler? ReturnToMainMenuButtonClicked;

    public GameMenuControl(UIFactories factories) : base("In-Game Menu")
    {
        IsClickThrough = true;
        this.Add(new ComplexBox().WithBlurredBackground()); // block click-through
        this.BuildLayout()
            .AddMenu(factories, b => b
                .WithCloseAction("To main menu", () => ReturnToMainMenuButtonClicked?.Invoke())
                .AddMenuAction("Resume game", () => ResumeGameButtonClicked?.Invoke())
                .AddMenuAction("Options", () => { }, new Binding<bool>(false))
            );
    }
}
