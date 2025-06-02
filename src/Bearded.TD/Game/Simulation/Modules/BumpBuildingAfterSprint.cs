using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

using static BumpBuildingAfterSprint;

[Trigger("bumpedBuilding")]
readonly record struct BumpedBuilding(GameObject Building) : IComponentEvent;

[Component("bumpBuildingAfterSprint")]
sealed class BumpBuildingAfterSprint(IParameters parameters)
    : Component<IParameters>(parameters), IListener<StoppedSprinting>, IListener<CollidedWithLevel>
{
    private Instant bumpUntil;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan TimeAfterSprint { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe<StoppedSprinting>(this);
        Events.Subscribe<CollidedWithLevel>(this);
    }

    public void HandleEvent(StoppedSprinting _)
    {
        bumpUntil = Owner.Game.Time + Parameters.TimeAfterSprint;
    }

    public void HandleEvent(CollidedWithLevel e)
    {
        if (bumpUntil < Owner.Game.Time)
            return;

        if (Owner.Game.BuildingLayer.TryGetMaterializedBuilding(e.Tile, out var building))
        {
            Events.Send(new BumpedBuilding(building));
        }
    }
}
