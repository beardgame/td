using System;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers;

sealed class MiningTaskPlaceholder : GameObject
{
    private readonly Faction faction;
    private readonly Tile tile;
    private readonly Id<IWorkerTask> taskId;
    private MiningTask task = null!;

    public MiningTaskPlaceholder(Faction faction, Tile tile, Id<IWorkerTask> taskId)
    {
        this.faction = faction;
        this.tile = tile;

        this.taskId = taskId;
    }

    protected override void OnAdded()
    {
        base.OnAdded();

        task = new MiningTask(taskId, this, tile, Game.GeometryLayer);
        if (!faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var workers))
        {
            throw new NotSupportedException("Cannot mine tile without workers.");
        }
        workers.RegisterTask(task);
        Game.MiningLayer.MarkTileForMining(tile);
    }

    protected override void OnDelete()
    {
        Game.MiningLayer.CancelTileForMining(tile);

        base.OnDelete();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!task.Finished) return;

        Game.Meta.Events.Send(new TileMined(faction, tile));
        Delete();
    }

    public override void Draw(CoreDrawers drawers)
    {
        var color = .5f * Color.MediumVioletRed;
        drawers.ConsoleBackground.FillCircle(
            Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, color, 6);
    }
}