using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.UI.Controls.NotificationClickActionFactory;

namespace Bearded.TD.UI.Controls;

sealed class GameNotificationsUI
{
    private readonly ImmutableArray<INotificationListener> notificationListeners;
    private readonly List<Notification> notifications = new();

    private GameInstance game = null!;
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
                @event => $"Constructed {@event.Name}",
                @event => @event.GameObject is IPositionable positionable ? ScrollTo(game, positionable) : null),
            textAndGameObjectEventListener<BuildingRepairFinished>(
                @event => $"Repaired {@event.Name}",
                @event => @event.GameObject is IPositionable positionable ? ScrollTo(game, positionable) : null),
            textAndGameObjectEventListener<BuildingUpgradeFinished>(
                @event => $"Upgraded {@event.BuildingName} with {@event.Upgrade.Name}",
                @event => @event.GameObject is IPositionable positionable ? ScrollTo(game, positionable) : null),
            textOnlyEventListener<TechnologyUnlocked>(
                @event => $"Unlocked {@event.Technology.Name}"));

    public void Initialize(GameInstance game)
    {
        this.game = game;

        var events = game.Meta.Events;
        foreach (var notificationListener in notificationListeners)
        {
            notificationListener.Subscribe(events);
        }
    }

    public void Terminate()
    {
        var events = game.Meta.Events;
        foreach (var notificationListener in notificationListeners)
        {
            notificationListener.Unsubscribe(events);
        }
    }

    public void Update()
    {
        var notificationsChanged = false;

        while (notifications.Count > 0 && notifications[0].ExpirationTime < game.State.Time)
        {
            notifications.RemoveAt(0);
            notificationsChanged = true;
        }

        if (notificationsChanged)
        {
            NotificationsChanged?.Invoke();
        }
    }

    private NotificationListener<T> textOnlyEventListener<T>(Func<T, string> textExtractor)
        where T : struct, IGlobalEvent =>
        new(
            this,
            @event => new Notification(
                textExtractor(@event),
                null,
                expirationTimeForNotification(),
                NotificationStyle.Default));

    private NotificationListener<T> textAndGameObjectEventListener<T>(
        Func<T, string> textExtractor, Func<T, NotificationClickAction?> clickActionExtractor)
        where T : struct, IGlobalEvent =>
        new(
            this,
            @event => new Notification(
                textExtractor(@event),
                clickActionExtractor(@event),
                expirationTimeForNotification(),
                NotificationStyle.Default));

    private void addNotification(Notification notification)
    {
        if (notifications.Count == Constants.Game.GameUI.MaxNotifications)
        {
            notifications.RemoveAt(0);
        }

        notifications.Add(notification);
        NotificationsChanged?.Invoke();
    }

    private Instant expirationTimeForNotification() => game.State.Time + Constants.Game.GameUI.NotificationDuration;

    private interface INotificationListener
    {
        void Subscribe(GlobalGameEvents events);
        void Unsubscribe(GlobalGameEvents events);
    }

    private sealed class NotificationListener<T> : IListener<T>, INotificationListener where T : struct, IGlobalEvent
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
