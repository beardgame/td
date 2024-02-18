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
        Background = () => Constants.UI.Colors.Get(BackgroundColor.Default),
    };

    public static readonly NotificationStyle Action =
        new(Background: () => Constants.Game.GameUI.ActionBackgroundColor);

    public static NotificationStyle ImmediateAction(ITimeSource timeSource) => new(
        Background: flashingBackground(
            Constants.Game.GameUI.ActionBackgroundColor,
            Constants.Game.GameUI.UrgentBackgroundColor,
            1.S(),
            timeSource));

    private static NotificationBackgroundColor flashingBackground(
        Color firstColor, Color secondColor, TimeSpan flashDuration, ITimeSource timeSource) =>
        () => Color.Lerp(firstColor, secondColor, lerpFactor(flashDuration, timeSource));

    private static float lerpFactor(TimeSpan flashDuration, ITimeSource timeSource)
    {
        var t = (float)(timeSource.Time.NumericValue / flashDuration.NumericValue);
        return .5f - .5f * MathF.Cos(t * MathConstants.TwoPi);
    }
}
