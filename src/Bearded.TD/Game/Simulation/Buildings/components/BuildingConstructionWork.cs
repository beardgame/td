using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingConstructionWork : Component
{
    private readonly Id<IWorkerTask> taskId;

    private IFactionProvider? factionProvider;
    private Faction? faction;
    private BuildingWorkerTask? workerTask;

    public ResourceAmount? ResourcesInvestedSoFar => workerTask?.ResourcesConsumed;

    public BuildingConstructionWork(Id<IWorkerTask> taskId)
    {
        this.taskId = taskId;
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider =>
        {
            State.Satisfies(
                factionProvider == null && faction == null,
                "Should not initialize faction provider more than once.");

            factionProvider = provider;
            faction = provider.Faction;
        });
    }

    public override void Activate()
    {
        base.Activate();

        if (faction == null)
        {
            throw new InvalidOperationException("Faction must be resolved before activating this component.");
        }

        if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
        {
            throw new NotSupportedException("Cannot build building without resources.");
        }
        if (!faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var workers))
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

    public override void Update(TimeSpan elapsedTime)
    {
        if (factionProvider?.Faction != faction)
        {
            State.IsInvalid(
                "Did not expect the faction provider to ever provide a different faction during the construction " +
                "work.");
        }
    }
}
