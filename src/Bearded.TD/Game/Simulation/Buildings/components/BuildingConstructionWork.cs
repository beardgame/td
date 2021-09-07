using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingConstructionWork<T> : Component<T>
        where T : IComponentOwner, IDeletable, IFactioned, IGameObject, INamed
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

            var cost = Owner.GetComponents<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
            workerTask = new BuildingWorkerTask(
                taskId,
                Owner.Game,
                Owner.GetComponents<IIncompleteBuilding>().Single(),
                OccupiedTileAccumulator.AccumulateOccupiedTiles(Owner),
                resources.ReserveResources(new FactionResources.ResourceRequest(cost)));
            workers.RegisterTask(workerTask);
        }

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}
    }
}
