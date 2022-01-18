using Bearded.Graphics;

namespace Bearded.TD.UI.Controls;

public record struct NotificationStyle(Color Background)
{
    public static readonly NotificationStyle Default = new()
    {
        Background = BackgroundBox.DefaultColor
    };

    public static readonly NotificationStyle Severe = Default with
    {
        Background = Color.DarkRed
    };
}
