using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls;

readonly record struct Notification(
    string Text,
    NotificationClickAction? ClickAction,
    Instant ExpirationTime,
    NotificationStyle Style)
{
    public void OnClick() => ClickAction?.Invoke();
}
