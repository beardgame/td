using System;
using Bearded.Graphics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.UI.Controls;

record struct NotificationStyle(NotificationStyle.NotificationBackgroundColor Background)
{
    public delegate Color NotificationBackgroundColor();

    public static readonly NotificationStyle Default = new()
    {
        Background = () => BackgroundBox.DefaultColor
    };

    public static readonly NotificationStyle Action = Default with
    {
        Background = () => Constants.Game.GameUI.ActionBackgroundColor
    };

    // TODO: this cannot depend on game time
    public static NotificationStyle ImmediateAction(ITimeSource timeSource) => Default with
    {
        Background = flashingBackground(
            Constants.Game.GameUI.ActionBackgroundColor,
            Constants.Game.GameUI.UrgentBackgroundColor,
            1.S(),
            timeSource)
    };

    private static NotificationBackgroundColor flashingBackground(
        Color firstColor, Color secondColor, TimeSpan flashDuration, ITimeSource timeSource) =>
        () => Color.Lerp(firstColor, secondColor, lerpFactor(flashDuration, timeSource));

    private static float lerpFactor(TimeSpan flashDuration, ITimeSource timeSource)
    {
        var t = (float)(timeSource.Time.NumericValue / flashDuration.NumericValue);
        return .5f - .5f * MathF.Cos(t * MathConstants.TwoPi);
    }
}
