using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls
{
    sealed class GameNotificationsUI
    {
        private readonly ImmutableArray<INotificationListener> notificationListeners;
        private readonly List<Notification> notifications = new List<Notification>();

        public GameInstance Game { get; private set; }
        public ReadOnlyCollection<Notification> Notifications { get; }

        public event VoidEventHandler? NotificationsChanged;

        public GameNotificationsUI()
        {
            notificationListeners = createNotificationListeners();
            Notifications = notifications.AsReadOnly();
        }

        private ImmutableArray<INotificationListener> createNotificationListeners() =>
            ImmutableArray.Create<INotificationListener>(
                textAndGameObjectEventListener<BuildingConstructionFinished>(
                    @event => $"Constructed {@event.Building.Blueprint.Name}",
                    @event => @event.Building),
                textAndGameObjectEventListener<BuildingUpgradeFinished>(
                    @event => $"Upgraded {@event.Building.Blueprint.Name} with {@event.Upgrade.Name}",
                    @event => @event.Building),
                textOnlyEventListener<TechnologyUnlocked>(
                    @event => $"Unlocked {@event.Technology.Name}"));

        public void Initialize(GameInstance game)
        {
            Game = game;

            var events = game.Meta.Events;
            foreach (var notificationListener in notificationListeners)
            {
                notificationListener.Subscribe(events);
            }
        }

        public void Terminate()
        {
            var events = Game.Meta.Events;
            foreach (var notificationListener in notificationListeners)
            {
                notificationListener.Unsubscribe(events);
            }
        }

        public void Update()
        {
            var notificationsChanged = false;

            while (notifications.Count > 0 && notifications[0].ExpirationTime < Game.State.Time)
            {
                notifications.RemoveAt(0);
                notificationsChanged = true;
            }

            if (notificationsChanged)
            {
                NotificationsChanged?.Invoke();
            }
        }

        private NotificationListener<T> textOnlyEventListener<T>(Func<T, string> textExtractor, bool isSevere = false)
            where T : struct, IGlobalEvent =>
            new NotificationListener<T>(
                this,
                @event => new Notification(
                    textExtractor(@event),
                    Maybe.Nothing,
                    expirationTimeForNotification(),
                    isSevere ? Maybe.Nothing : Maybe.Just(Color.DarkRed)));

        private NotificationListener<T> textAndGameObjectEventListener<T>(
            Func<T, string> textExtractor, Func<T, GameObject> gameObjectExtractor)
            where T : struct, IGlobalEvent =>
                new NotificationListener<T>(
                    this,
                    @event => new Notification(
                        textExtractor(@event),
                        Maybe.Just(gameObjectExtractor(@event)),
                        expirationTimeForNotification(),
                        Maybe.Nothing));

        private void addNotification(Notification notification)
        {
            if (notifications.Count == Constants.Game.GameUI.MaxNotifications)
            {
                notifications.RemoveAt(0);
            }

            notifications.Add(notification);
            NotificationsChanged?.Invoke();
        }

        private Instant expirationTimeForNotification(bool isSevere = false) =>
            Game.State.Time + (isSevere
                ? Constants.Game.GameUI.SevereNotificationDuration
                : Constants.Game.GameUI.NotificationDuration);

        public class Notification
        {
            public string Text { get; }
            public Maybe<GameObject> Subject { get; }
            public Instant ExpirationTime { get; }
            public Maybe<Color> Background { get; }

            public Notification(string text, Maybe<GameObject> subject, Instant expirationTime, Maybe<Color> background)
            {
                Text = text;
                Subject = subject;
                ExpirationTime = expirationTime;
                Background = background;
            }
        }

        private interface INotificationListener
        {
            void Subscribe(GlobalGameEvents events);
            void Unsubscribe(GlobalGameEvents events);
        }

        private class NotificationListener<T> : IListener<T>, INotificationListener where T : struct, IGlobalEvent
        {
            private readonly GameNotificationsUI parent;
            private readonly Func<T, Notification> eventTransformer;

            public NotificationListener(
                GameNotificationsUI parent,
                Func<T, Notification> eventTransformer)
            {
                this.parent = parent;
                this.eventTransformer = eventTransformer;
            }

            public void Subscribe(GlobalGameEvents events)
            {
                events.Subscribe(this);
            }

            public void Unsubscribe(GlobalGameEvents events)
            {
                events.Unsubscribe(this);
            }

            public void HandleEvent(T @event)
            {
                parent.addNotification(eventTransformer(@event));
            }
        }
    }
}
