using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
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
                @event => $"Unlocked {@event.Technology.Name}"),
            new ExplorationTokenListener(this),
            new TechnologyTokenListener(this));

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
        if (notifications.RemoveAll(n => n.ExpirationTime.HasValue && n.ExpirationTime.Value <= game.State.Time) > 0)
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
        notifications.Add(notification);
        NotificationsChanged?.Invoke();
    }

    private void replaceNotification(Notification oldNotification, Notification newNotification)
    {
        var index = notifications.IndexOf(oldNotification);
        notifications[index] = newNotification;
        NotificationsChanged?.Invoke();
    }

    private void removeNotification(Notification notification)
    {
        notifications.Remove(notification);
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

    private abstract class TokenListener<TAwardEvent, TConsumeEvent> : INotificationListener
        where TAwardEvent : struct, IGlobalEvent
        where TConsumeEvent : struct, IGlobalEvent
    {
        private readonly GameNotificationsUI parent;

        private readonly IListener<TAwardEvent> awardListener;
        private readonly IListener<TConsumeEvent> consumeListener;

        protected abstract string NotificationText { get; }

        private Notification? notification;

        protected TokenListener(GameNotificationsUI parent)
        {
            this.parent = parent;

            awardListener = Listener.ForEvent<TAwardEvent>(_ => onAward());
            consumeListener = Listener.ForEvent<TConsumeEvent>(_ => onConsume());
        }

        public void Subscribe(GlobalGameEvents events)
        {
            events.Subscribe(awardListener);
            events.Subscribe(consumeListener);
        }

        public void Unsubscribe(GlobalGameEvents events)
        {
            events.Unsubscribe(awardListener);
            events.Unsubscribe(consumeListener);
        }

        private void onAward()
        {
            DebugAssert.State.Satisfies(!notification.HasValue);
            notification = new Notification(
                NotificationText,
                null,
                null,
                NotificationStyle.Action);
            parent.addNotification(notification.Value);
        }

        private void onConsume()
        {
            DebugAssert.State.Satisfies(notification.HasValue);
            if (!notification.HasValue)
            {
                return;
            }

            parent.removeNotification(notification.Value);
            notification = null;
        }
    }

    private sealed class ExplorationTokenListener : TokenListener<ExplorationTokenAwarded, ExplorationTokenConsumed>
    {
        protected override string NotificationText => "Exploration token available";

        public ExplorationTokenListener(GameNotificationsUI parent) : base(parent) {}
    }

    private sealed class TechnologyTokenListener : TokenListener<TechnologyTokenAwarded, TechnologyTokenConsumed>
    {
        protected override string NotificationText => "Technology token available";

        public TechnologyTokenListener(GameNotificationsUI parent) : base(parent) {}
    }
}
