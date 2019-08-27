using System.Collections.ObjectModel;
using Bearded.TD.Game;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Input;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;

namespace Bearded.TD.UI.Controls
{
    sealed class GameNotificationsUIControl : CompositeControl, IListItemSource
    {
        private const int notificationHeight = 24;

        private ReadOnlyCollection<GameNotificationsUI.Notification> notifications;
        private readonly GameNotificationsUI model;
        private readonly ListControl list;

        public int ItemCount => notifications.Count;

        public GameNotificationsUIControl(GameNotificationsUI model)
        {
            this.model = model;

            list = new ListControl {ItemSource = this};
            Add(list);

            model.NotificationsChanged += updateList;
            updateNotifications();
        }

        private void updateList()
        {
            updateNotifications();
            list.Reload();
        }

        private void updateNotifications()
        {
            notifications = model.Notifications;
            new AnchorTemplate(this)
                .Top(margin: 0, height: notifications.Count * notificationHeight)
                .ApplyTo(this);
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        public double HeightOfItemAt(int index) => notificationHeight;

        public Control CreateItemControlFor(int index) => new NotificationControl(model.Game, notifications[index]);

        public void DestroyItemControlAt(int index, Control control) {}

        private class NotificationControl : CompositeControl
        {
            private const double margin = 2;

            private readonly GameInstance game;
            private readonly GameNotificationsUI.Notification notification;

            public NotificationControl(GameInstance game, GameNotificationsUI.Notification notification)
            {
                this.game = game;
                this.notification = notification;

                Add(new BackgroundBox().Anchor(a => a.MarginAllSides(margin)));
                Add(new Label { Text = notification.Text, TextAnchor = new Vector2d(0, .5), FontSize = 14 }
                    .Anchor(a => a.MarginAllSides(margin * 2)));
            }

            public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
            {
                if (eventArgs.MouseButton == MouseButton.Left)
                {
                    notification.Subject
                        .SelectMany(gameObject => Maybe.FromNullable(gameObject as IPositionable))
                        .Match(positionable => game.Camera.Position = positionable.Position.NumericValue);
                    eventArgs.Handled = true;
                    return;
                }
                base.MouseButtonHit(eventArgs);
            }
        }
    }
}
