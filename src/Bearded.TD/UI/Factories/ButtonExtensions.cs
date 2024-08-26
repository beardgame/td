using System;
using Bearded.Graphics;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class ButtonExtensions
{
    public static Button BindIsEnabled(this Button button, Binding<bool>? isEnabled)
    {
        if (isEnabled == null)
        {
            return button;
        }

        button.IsEnabled = isEnabled.Value;
        isEnabled.SourceUpdated += enabled => button.IsEnabled = enabled;

        return button;
    }

    public static void AnimateBackground(
        this Button button,
        ButtonBackgroundColor colors,
        Action<Color> setFillColor,
        Func<ShapeComponent> getFillComponent,
        Animations animations,
        IReadonlyBinding<bool>? isEnabled = null,
        IReadonlyBinding<bool>? isActive = null,
        bool alwaysRenderAsEnabled = false)
    {
        var mouseState = new MouseStateObserver(button);
        var stateChanged = Binding.AggregateChanges([isEnabled, isActive]);

        isEnabled ??= Binding.Constant(button.IsEnabled);
        isActive ??= Binding.Constant(false);
        IAnimationController? backgroundAnimation = null;

        mouseState.StateChanged += () => updateColor();
        stateChanged.SourceUpdated += _ => updateColor();
        stateChanged.ControlUpdated += _ => updateColor();

        updateColor(true);
        return;

        void updateColor(bool skipAnimation = false)
        {
            var color = (isEnabled, mouseState) switch
            {
                ({ Value: false }, _) when !alwaysRenderAsEnabled => colors.Disabled,
                ({ Value: true }, { MouseIsOver: false }) => colors.Neutral,
                ({ Value: true }, { MouseIsDown: true }) => colors.Active,
                ({ Value: true }, { MouseIsOver: true }) => colors.Hover,
                _ when isActive.Value => colors.Active,
                _ => colors.Neutral,
            } ?? Color.Transparent;

            if (skipAnimation)
            {
                backgroundAnimation?.Cancel();
                setFillColor(color);
                return;
            }

            backgroundAnimation?.Cancel();
            backgroundAnimation = animations.Start(
                Constants.UI.Button.BackgroundColorAnimation(getFillComponent(), setFillColor, color)
            );
        }
    }
}
