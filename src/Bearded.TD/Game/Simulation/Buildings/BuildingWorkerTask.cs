using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingWorkerTask : WorkerTaskBase
{
    private readonly IIncompleteBuilding incompleteBuilding;

    public override string Name => $"Build {incompleteBuilding.StructureName}";
    protected override bool IsCompleted => incompleteBuilding.IsCompleted;
    protected override bool IsCancelled => incompleteBuilding.IsCancelled;

    public BuildingWorkerTask(
        Id<IWorkerTask> taskId,
        GameState gameState,
        IIncompleteBuilding incompleteBuilding,
        IEnumerable<Tile> tiles,
        FactionResources.IResourceReservation resourceReservation) : base(taskId, tiles, gameState, resourceReservation)
    {
        this.incompleteBuilding = incompleteBuilding;
    }

    protected override void Start() => incompleteBuilding.StartBuild();
    protected override void Complete() => incompleteBuilding.CompleteBuild();
    protected override void Cancel() => incompleteBuilding.CancelBuild();
    protected override void UpdateToMatch() => incompleteBuilding.SetBuildProgress(PercentCompleted);
}