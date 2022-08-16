using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

[Component("autoRepair")]
sealed class AutoRepair
    : Component<AutoRepair.IParameters>, IListener<ObjectRuined>, IListener<RepairFinished>, IListener<TakeDamage>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(4)]
        TimeSpan TimeUntilRepairStart { get; }

        [Modifiable(4)]
        TimeSpan RepairDuration { get; }

        [Modifiable(true, DefaultValueType = typeof(bool))]
        bool ResetTimerOnDamage { get; }
    }

    private IFactionProvider? factionProvider;

    private IRuined? ruined;
    private ScheduledRepair? repair;

    public AutoRepair(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);

        Events.Subscribe<ObjectRuined>(this);
        Events.Subscribe<RepairFinished>(this);
        Events.Subscribe<TakeDamage>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<ObjectRuined>(this);
        Events.Unsubscribe<RepairFinished>(this);
        Events.Unsubscribe<TakeDamage>(this);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        repair?.Update();
    }

    public void HandleEvent(ObjectRuined @event)
    {
        if (factionProvider == null)
        {
            Owner.Game.Meta.Logger.Debug?.Log("Cannot initiate auto-repair for a game object without faction.");
            return;
        }

        ruined = Owner.GetComponents<IRuined>().Single();
        resetTimer();
    }

    public void HandleEvent(RepairFinished @event)
    {
        ruined = null;
        repair = null;
    }

    public void HandleEvent(TakeDamage @event)
    {
        if (ruined != null && Parameters.ResetTimerOnDamage)
        {
            resetTimer();
        }
    }

    private void resetTimer()
    {
        if (ruined == null)
        {
            throw new InvalidOperationException("Cannot reset timer when the building is not ruined.");
        }
        if (factionProvider == null)
        {
            throw new InvalidOperationException("Cannot initiate auto-repair for a game object without faction.");
        }

        repair?.CancelIfStarted();
        repair = new ScheduledRepair(
            Owner.Game,
            ruined,
            factionProvider,
            Owner.Game.Time + Parameters.TimeUntilRepairStart,
            Parameters.RepairDuration);
    }

    private sealed class ScheduledRepair
    {
        private readonly GameState game;
        private readonly IRuined ruined;
        private readonly IFactionProvider factionProvider;
        private readonly Instant startTime;
        private readonly TimeSpan repairDuration;
        private IIncompleteRepair? incompleteRepair;

        public ScheduledRepair(
            GameState game, IRuined ruined, IFactionProvider factionProvider, Instant startTime, TimeSpan repairDuration)
        {
            this.game = game;
            this.ruined = ruined;
            this.factionProvider = factionProvider;
            this.startTime = startTime;
            this.repairDuration = repairDuration;
        }

        public void Update()
        {
            if (incompleteRepair == null && startTime <= game.Time)
            {
                incompleteRepair = ruined.StartRepair(factionProvider.Faction);
                // TODO: cannot start the repair in the same frame, because we need to wait for the incomplete repair to
                // be added as component first, which doesn't happen this frame due to enumeration problems.
                return;
            }

            if (incompleteRepair == null)
            {
                return;
            }

            if (!incompleteRepair.IsStarted)
            {
                incompleteRepair.StartRepair();
            }

            var expectedProgress = ((game.Time - startTime) / repairDuration).Clamped(0, 1);
            incompleteRepair.SetRepairProgress(expectedProgress);

            if (expectedProgress >= 1)
            {
                incompleteRepair.CompleteRepair();
            }
        }

        public void CancelIfStarted()
        {
            incompleteRepair?.CancelRepair();
            incompleteRepair = null;
        }
    }
}
