using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingConstructor<T>
        : Component<T>, IBuildingConstructionSyncer, IBuildingConstructor, IListener<ObjectDeleting>
        where T : GameObject, IComponentOwner, IDeletable, INamed
    {
        private enum BuildStage : byte
        {
            NotStarted = 0,
            InProgress = 1,
            Completed = 2,
            Cancelled = 3
        }

        private BuildStage buildStage = BuildStage.NotStarted;
        private BuildStage syncedBuildStage = BuildStage.NotStarted;

        public event VoidEventHandler? Completing;

        public bool IsCompleted => buildStage == BuildStage.Completed;
        public bool IsCancelled => buildStage == BuildStage.Cancelled;

        public string StructureName => Owner.Name;

        protected override void Initialize() {}

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}

        public void StartBuild()
        {
            State.Satisfies(syncedBuildStage == BuildStage.NotStarted);

            // Avoid duplicate sync requests.
            if (buildStage != BuildStage.NotStarted)
            {
                return;
            }

            buildStage = BuildStage.InProgress;
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingConstructionStart.Command);
        }

        public void SyncStartBuild()
        {
            State.Satisfies(syncedBuildStage == BuildStage.NotStarted);

            // Catch up the in-memory stage.
            if (buildStage < BuildStage.InProgress)
            {
                buildStage = BuildStage.InProgress;
            }

            syncedBuildStage = BuildStage.InProgress;
            Events.Send(new ConstructionStarted());
            Owner.Game.Meta.Events.Send(new BuildingConstructionStarted(Owner));
        }

        public void ProgressBuild(HitPoints hitPointsAdded)
        {
            State.Satisfies(syncedBuildStage == BuildStage.InProgress);
            Events.Send(new HealDamage(hitPointsAdded));
        }

        public void CompleteBuild()
        {
            State.Satisfies(syncedBuildStage == BuildStage.InProgress);

            // Avoid duplicate sync requests.
            if (buildStage > BuildStage.InProgress)
            {
                return;
            }

            buildStage = BuildStage.Completed;
            // TODO: currently cast needed to get the building ID
            (Owner as Building)?.Sync(SyncBuildingConstructionCompletion.Command);
        }

        public void SyncCompleteBuild()
        {
            State.Satisfies(syncedBuildStage == BuildStage.InProgress);

            // Catch up the in-memory stage.
            if (buildStage < BuildStage.Completed)
            {
                buildStage = BuildStage.Completed;
            }

            // Give clients the chance to catch up on the rest of the progress before marking the building as complete.
            // TODO: the constructor should handle progress itself so it does not need this event
            Completing?.Invoke();

            syncedBuildStage = BuildStage.Completed;
            Events.Send(new ConstructionFinished());
            Owner.Game.Meta.Events.Send(new BuildingConstructionFinished(Owner.Name, Owner));
        }

        public void AbortBuild()
        {
            State.Satisfies(buildStage == BuildStage.NotStarted);
            Owner.Delete();
        }

        public void HandleEvent(ObjectDeleting @event)
        {
            buildStage = BuildStage.Cancelled;
        }
    }

    interface IBuildingConstructor
    {
        event VoidEventHandler? Completing;

        string StructureName { get; }
        bool IsCompleted { get; }
        bool IsCancelled { get; }

        void StartBuild();
        void ProgressBuild(HitPoints hitPointsAdded);
        void CompleteBuild();
        void AbortBuild();
    }

    interface IBuildingConstructionSyncer
    {
        void SyncStartBuild();
        void SyncCompleteBuild();
    }
}
