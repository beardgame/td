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
    sealed class BuildingConstructor<T> : Component<T>, IBuildingConstructor, IListener<ObjectDeleting>
        where T : GameObject, IComponentOwner, IDeletable, INamed
    {
        private enum BuildStage
        {
            NotStarted,
            InProgress,
            Completed,
            Cancelled
        }

        private BuildStage buildStage = BuildStage.NotStarted;

        public event VoidEventHandler? Completing;

        public bool IsCompleted => buildStage == BuildStage.Completed;
        public bool IsCancelled => buildStage == BuildStage.Cancelled;

        public string StructureName => Owner.Name;

        protected override void Initialize() {}

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}

        public void StartBuild()
        {
            State.Satisfies(buildStage == BuildStage.NotStarted);
            buildStage = BuildStage.InProgress;
            Events.Send(new ConstructionStarted());
            Owner.Game.Meta.Events.Send(new BuildingConstructionStarted(Owner));
        }

        public void ProgressBuild(HitPoints hitPointsAdded)
        {
            State.Satisfies(buildStage == BuildStage.InProgress);
            Events.Send(new HealDamage(hitPointsAdded));
        }

        public void CompleteBuild()
        {
            State.Satisfies(buildStage == BuildStage.InProgress);
            Completing?.Invoke();
            buildStage = BuildStage.Completed;
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
}
