using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins
{
    sealed class IncompleteRepair<T>
        : Component<T>,
            IIncompleteRepair,
            IRepairSyncer,
            ProgressTracker.IProgressSubject
        where T : IComponentOwner<T>, IGameObject, INamed
    {
        private readonly Faction repairingFaction;
        private readonly ProgressTracker progressTracker;

        public ResourceAmount Cost { get; }
        public double PercentageComplete { get; private set; }

        public IncompleteRepair(ResourceAmount cost, Faction repairingFaction)
        {
            Cost = cost;
            this.repairingFaction = repairingFaction;
            progressTracker = new ProgressTracker(this);
        }

        protected override void OnAdded() {}

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}

        public void SendSyncStart()
        {
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingRepairStart.Command);
        }

        public void SendSyncComplete()
        {
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingRepairCompletion.Command);
        }

        public void OnStart() {}

        public void OnProgressSet(double percentage)
        {
            PercentageComplete = percentage;
        }

        public void OnComplete()
        {
            Events.Send(new RepairFinished(repairingFaction));
            Owner.Game.Meta.Events.Send(new BuildingRepairFinished(Owner.Name, Owner));
        }

        public void OnCancel()
        {
            Events.Send(new RepairCancelled());
        }

        public bool IsCompleted => progressTracker.IsCompleted;
        public bool IsCancelled => progressTracker.IsCancelled;
        public string StructureName => Owner.Name;
        public void StartRepair() => progressTracker.Start();
        public void SetRepairProgress(double percentage) => progressTracker.SetProgress(percentage);
        public void CompleteRepair() => progressTracker.Complete();
        public void CancelRepair() => progressTracker.Cancel();

        public void SyncStartRepair() => progressTracker.SyncStart();
        public void SyncCompleteRepair() => progressTracker.SyncComplete();
    }

    interface IIncompleteRepair
    {
        bool IsCompleted { get; }
        bool IsCancelled { get; }
        string StructureName { get; }
        double PercentageComplete { get; }
        ResourceAmount Cost { get; }

        public void StartRepair();
        public void SetRepairProgress(double percentage);
        public void CompleteRepair();
        public void CancelRepair();
    }

    interface IRepairSyncer
    {
        void SyncStartRepair();
        void SyncCompleteRepair();
    }
}
