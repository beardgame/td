using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

sealed class RepairWorkerTask : WorkerTaskBase
{
    private readonly IIncompleteRepair incompleteRepair;

    public override string Name => $"Build {incompleteRepair.StructureName}";
    protected override bool IsCompleted => incompleteRepair.IsCompleted;
    protected override bool IsCancelled => incompleteRepair.IsCancelled;

    public RepairWorkerTask(
        Id<IWorkerTask> taskId,
        IIncompleteRepair incompleteRepair,
        IEnumerable<Tile> tiles,
        GameState gameState,
        FactionResources.IResourceReservation resourceReservation) : base(taskId, tiles, gameState, resourceReservation)
    {
        this.incompleteRepair = incompleteRepair;
    }

    protected override void Start() => incompleteRepair.StartRepair();
    protected override void Complete() => incompleteRepair.CompleteRepair();
    protected override void UpdateToMatch() => incompleteRepair.SetRepairProgress(PercentCompleted);
}
