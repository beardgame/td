using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingConstructionWork : Component<Building>
    {
        private readonly Id<IWorkerTask> taskId;
        private BuildingWorkerTask workerTask = null!;

        public BuildingConstructionWork(Id<IWorkerTask> taskId)
        {
            this.taskId = taskId;
        }

        protected override void Initialize()
        {
            if (!Owner.Faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                throw new NotSupportedException("Cannot build building without resources.");
            }
            if (!Owner.Faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var workers))
            {
                throw new NotSupportedException("Cannot build building without workers.");
            }

            workerTask = new BuildingWorkerTask(
                taskId,
                Owner,
                resources.ReserveResources(new FactionResources.ResourceRequest(Owner.Blueprint.ResourceCost)));
            workers.RegisterTask(workerTask);
        }

        public void StartWork()
        {
            Owner.Game.Meta.Events.Send(new BuildingConstructionStarted(Owner));
        }

        public IRequest<Player, GameInstance> CancelRequest()
        {
            return AbortTask.Request(Owner.Faction, workerTask);
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
