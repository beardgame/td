using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class IncompleteBuilding<T>
        : Component<T>,
            IBuildingConstructionSyncer,
            IIncompleteBuilding,
            IListener<ObjectDeleting>,
            ProgressTracker.IProgressSubject
        where T : GameObject, IComponentOwner, IDeletable
    {
        private readonly ProgressTracker progressTracker;

        private INameProvider? nameProvider;
        private IHealthEventReceiver? receiver;
        private HitPoints healthGiven;
        private HitPoints maxHealth;

        public string StructureName => nameProvider.NameOrDefault();

        public double PercentageComplete { get; private set; }

        public IncompleteBuilding()
        {
            progressTracker = new ProgressTracker(this);
        }

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<INameProvider>(Owner, Events, provider => nameProvider = provider);
            ComponentDependencies.Depend<IHealthEventReceiver>(Owner, Events, r => receiver = r);
            healthGiven = 1.HitPoints();
            maxHealth = Owner.GetComponents<IHealth>().SingleOrDefault()?.MaxHealth ?? new HitPoints(1);
        }

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}

        public void SendSyncStart()
        {
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingConstructionStart.Command);
        }

        public void SendSyncComplete()
        {
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingConstructionCompletion.Command);
        }

        public void OnStart()
        {
            State.Satisfies(receiver != null);
            Events.Send(new ConstructionStarted());
            Owner.Game.Meta.Events.Send(new BuildingConstructionStarted(Owner));
        }

        public void OnProgressSet(double percentage)
        {
            PercentageComplete = percentage;
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

        public void OnComplete()
        {
            var healthRemaining = maxHealth - healthGiven;
            addHealth(healthRemaining);

            Events.Send(new ConstructionFinished());
            Owner.Game.Meta.Events.Send(new BuildingConstructionFinished(StructureName, Owner));
        }

        private void addHealth(HitPoints hitPoints)
        {
            receiver!.Heal(new HealInfo(hitPoints));
        }

        public void OnCancel()
        {
            Owner.Delete();
        }

        public void HandleEvent(ObjectDeleting @event)
        {
            progressTracker.Cancel();
        }

        public bool IsCompleted => progressTracker.IsCompleted;
        public bool IsCancelled => progressTracker.IsCancelled;
        public void StartBuild() => progressTracker.Start();
        public void SetBuildProgress(double percentage) => progressTracker.SetProgress(percentage);
        public void CompleteBuild() => progressTracker.Complete();
        public void CancelBuild() => progressTracker.Cancel();

        public void SyncStartBuild() => progressTracker.SyncStart();
        public void SyncCompleteBuild() => progressTracker.SyncComplete();
    }

    interface IIncompleteBuilding
    {
        bool IsCompleted { get; }
        bool IsCancelled { get; }
        string StructureName { get; }
        double PercentageComplete { get; }

        public void StartBuild();
        public void SetBuildProgress(double percentage);
        public void CompleteBuild();
        public void CancelBuild();
    }

    interface IBuildingConstructionSyncer
    {
        void SyncStartBuild();
        void SyncCompleteBuild();
    }
}
