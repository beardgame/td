using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
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
    private readonly GameUIController gameUIController;
    private readonly ImmutableArray<INotificationListener> notificationListeners;
    private readonly List<Notification> notifications = new();

    private GameInstance game = null!;
    private ITimeSource timeSource = null!;
    public ReadOnlyCollection<Notification> Notifications { get; }

    public event VoidEventHandler? NotificationsChanged;

    public GameNotificationsUI(GameUIController gameUIController)
    {
        this.gameUIController = gameUIController;
        notificationListeners = createNotificationListeners();
        Notifications = notifications.AsReadOnly();
    }

    private ImmutableArray<INotificationListener> createNotificationListeners() =>
        ImmutableArray.Create<INotificationListener>(
            textAndGameObjectEventListener<BuildingConstructionFinished>(
                @event => $"Constructed {@event.GameObject.NameOrDefault()}",
                @event => ScrollTo(game, @event.GameObject)),
            textAndGameObjectEventListener<BuildingRepairFinished>(
                @event => $"Repaired {@event.GameObject.NameOrDefault()}",
                @event => ScrollTo(game, @event.GameObject)),
            textAndGameObjectEventListener<BuildingGainedLevel>(
                @event => $"{@event.GameObject.NameOrDefault()} gained level",
                @event => ScrollTo(game, @event.GameObject)),
            textAndGameObjectEventListener<BuildingUpgradeFinished>(
                @event => $"Upgraded {@event.BuildingName} with {@event.Upgrade.Name}",
                @event => @event.GameObject is IPositionable positionable ? ScrollTo(game, positionable) : null),
            textOnlyEventListener<TechnologyUnlocked>(
                @event => $"Unlocked {@event.Technology.Name}"),
            new ExplorationTokenListener(this),
            new TechnologyTokenListener(this));

    public void Initialize(GameInstance game, ITimeSource timeSource)
    {
        this.game = game;
        this.timeSource = timeSource;

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
        if (notifications.RemoveAll(n => n.ExpirationTime.HasValue && n.ExpirationTime.Value <= timeSource.Time) > 0)
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

    private Instant expirationTimeForNotification() => timeSource.Time + Constants.Game.GameUI.NotificationDuration;

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

    private abstract class TokenListener<TAwardedEvent, TConsumedEvent> : INotificationListener
        where TAwardedEvent : struct, IGlobalEvent
        where TConsumedEvent : struct, IGlobalEvent
    {
        protected GameNotificationsUI Parent { get; }

        private readonly IListener<TAwardedEvent> awardedListener;
        private readonly IListener<TConsumedEvent> consumedListener;
        private readonly IListener<WaveDeferred> deferredListener;

        protected abstract string NotificationText { get; }
        protected virtual NotificationClickAction? NotificationClickAction => null;

        private Notification? notification;

        protected TokenListener(GameNotificationsUI parent)
        {
            Parent = parent;

            awardedListener = Listener.ForEvent<TAwardedEvent>(_ => onAwarded());
            consumedListener = Listener.ForEvent<TConsumedEvent>(_ => onConsumed());
            deferredListener = Listener.ForEvent<WaveDeferred>(_ => onDeferred());
        }

        public void Subscribe(GlobalGameEvents events)
        {
            events.Subscribe(awardedListener);
            events.Subscribe(consumedListener);
            events.Subscribe(deferredListener);
        }

        public void Unsubscribe(GlobalGameEvents events)
        {
            events.Unsubscribe(awardedListener);
            events.Unsubscribe(consumedListener);
            events.Unsubscribe(deferredListener);
        }

        private void onAwarded()
        {
            DebugAssert.State.Satisfies(!notification.HasValue);
            notification = new Notification(
                NotificationText,
                NotificationClickAction,
                null,
                NotificationStyle.Action);
            Parent.addNotification(notification.Value);
        }

        private void onConsumed()
        {
            DebugAssert.State.Satisfies(notification.HasValue);
            if (!notification.HasValue)
            {
                return;
            }

            Parent.removeNotification(notification.Value);
            notification = null;
        }

        private void onDeferred()
        {
            if (!notification.HasValue)
            {
                return;
            }

            var oldNotification = notification.Value;
            var newNotification = oldNotification with
            {
                Style = NotificationStyle.ImmediateAction(Parent.timeSource)
            };
            Parent.replaceNotification(oldNotification, newNotification);
            notification = newNotification;
        }
    }

    private sealed class ExplorationTokenListener : TokenListener<ExplorationTokenAwarded, ExplorationTokenConsumed>
    {
        protected override string NotificationText => "Exploration token available";

        protected override NotificationClickAction? NotificationClickAction =>
            ScrollToContain(Parent.game, Parent.game.State.Enumerate<IZoneRevealer>());

        public ExplorationTokenListener(GameNotificationsUI parent) : base(parent) {}
    }

    private sealed class TechnologyTokenListener : TokenListener<TechnologyTokenAwarded, TechnologyTokenConsumed>
    {
        protected override string NotificationText => "Technology token available";

        protected override NotificationClickAction? NotificationClickAction =>
            Parent.gameUIController.ShowTechnologyModal;

        public TechnologyTokenListener(GameNotificationsUI parent) : base(parent) {}
    }
}
