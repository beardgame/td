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

    public static NotificationStyle ImmediateAction(ITimeSource timeSource) => Default with
    {
        Background = flashingBackground(
            BackgroundBox.DefaultColor,
            new Color(246, 190, 0) * .75f, // dark yellow
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
