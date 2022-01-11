using System.Collections.ObjectModel;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;

namespace Bearded.TD.UI.Controls;

sealed class GameNotificationsUIControl : CompositeControl, IListItemSource
{
    private const int notificationHeight = 24;
    private const int maxNotifications = 6;

    private ReadOnlyCollection<GameNotificationsUI.Notification> notifications;
    private readonly GameNotificationsUI model;
    private readonly ListControl list;

    public int ItemCount => notifications.Count;

    public GameNotificationsUIControl(GameNotificationsUI model)
    {
        this.model = model;

        IsClickThrough = true;
        this.Anchor(a => a.Top(margin: 0, height: maxNotifications * notificationHeight));
        list = new ListControl(listContainer: CreateClickThrough()) {ItemSource = this};
        Add(list);

        model.NotificationsChanged += updateList;
        notifications = model.Notifications;
    }

    private void updateList()
    {
        notifications = model.Notifications;
        list.Reload();
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    public double HeightOfItemAt(int index) => notificationHeight;

    public Control CreateItemControlFor(int index) => new NotificationControl(notifications[index]);

    public void DestroyItemControlAt(int index, Control control) {}

    private sealed class NotificationControl : CompositeControl
    {
        private const double margin = 2;

        private readonly GameNotificationsUI.Notification notification;

        public NotificationControl(GameNotificationsUI.Notification notification)
        {
            this.notification = notification;

            Add(new BackgroundBox(notification.Background ?? BackgroundBox.DefaultColor)
                .Anchor(a => a.MarginAllSides(margin)));
            Add(new Label { Text = notification.Text, TextAnchor = new Vector2d(0, .5), FontSize = 14 }
                .Anchor(a => a.MarginAllSides(margin * 2)));
        }

        public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.MouseButton == MouseButton.Left)
            {
                notification.OnClick();
                eventArgs.Handled = true;
                return;
            }
            base.MouseButtonHit(eventArgs);
        }
    }
}
