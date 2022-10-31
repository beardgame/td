using System;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class IncompleteBuildingComponent : Component, IBuildingConstructionSyncer
{
    private IncompleteBuildingWork? work;
    private IHealthEventReceiver? receiver;

    public IIncompleteBuilding Work =>
        work ?? throw new InvalidOperationException("Accessed work before adding component to collection.");

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IHealthEventReceiver>(Owner, Events, r => receiver = r);

        var maxHealth = Owner.GetComponents<IHealth>().SingleOrDefault()?.MaxHealth ?? new HitPoints(1);
        work = new IncompleteBuildingWork(Owner, Events, maxHealth, addHealth);
        Events.Subscribe(work);
    }

    private void addHealth(HitPoints hitPoints)
    {
        receiver!.Heal(new HealInfo(hitPoints));
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void SyncStartBuild()
    {
        if (work == null)
        {
            throw new InvalidOperationException("Attempted to start incomplete building before adding to building.");
        }
        work.SyncStartBuild();
    }

    public void SyncCompleteBuild()
    {
        if (work == null)
        {
            throw new InvalidOperationException("Attempted to start incomplete building before adding to building.");
        }
        work.SyncCompleteBuild();
    }

    private sealed class IncompleteBuildingWork : IncompleteWork,
        IIncompleteBuilding,
        IListener<ObjectDeleting>
    {
        private readonly GameObject owner;
        private readonly ComponentEvents events;
        private readonly HitPoints maxHealth;
        private readonly Action<HitPoints> addHealth;
        private HitPoints healthGiven;

        public IncompleteBuildingWork(
            GameObject owner,
            ComponentEvents events,
            HitPoints maxHealth,
            Action<HitPoints> addHealth)
        {
            this.owner = owner;
            this.events = events;
            this.maxHealth = maxHealth;
            this.addHealth = addHealth;
            healthGiven = 1.HitPoints();
        }

        public override void SendSyncStart()
        {
            owner.Sync(SyncBuildingConstructionStart.Command);
        }

        public override void SendSyncComplete()
        {
            owner.Sync(SyncBuildingConstructionCompletion.Command);
        }

        public override void OnStart()
        {
            events.Send(new ConstructionStarted());
            owner.Game.Meta.Events.Send(new BuildingConstructionStarted(owner));
        }

        public override void OnProgressSet(double percentage)
        {
            var expectedHealthGiven = new HitPoints(MoreMath.CeilToInt(percentage * maxHealth.NumericValue));
            State.Satisfies(expectedHealthGiven >= 0.HitPoints());
            if (expectedHealthGiven == HitPoints.Zero)
            {
                return;
            }

            var newHealthGiven = expectedHealthGiven - healthGiven;
            addHealth(newHealthGiven);
            healthGiven = expectedHealthGiven;
        }

        public override void OnComplete()
        {
            var healthRemaining = maxHealth - healthGiven;
            addHealth(healthRemaining);

            events.Send(new ConstructionFinished());
            owner.Game.Meta.Events.Send(new BuildingConstructionFinished(owner));
        }

        public override void OnCancel()
        {
            State.IsInvalid("Progress is only cancelled by deleting the building.");
        }

        public void HandleEvent(ObjectDeleting @event)
        {
            // If the deletion comes from an external event, make sure the progress tracker is caught up.
            MakeCancelled();
        }

        public void SyncStartBuild() => SyncStart();
        public void SyncCompleteBuild() => SyncComplete();
    }
}

interface IIncompleteBuilding : IIncompleteWork {}

interface IBuildingConstructionSyncer
{
    void SyncStartBuild();
    void SyncCompleteBuild();
}
