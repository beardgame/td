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
sealed partial class BumpBuildingAfterSprint(IParameters parameters) : Component<IParameters>(parameters)
{
    private Instant bumpUntil;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan TimeAfterSprint { get; }
    }

    [Handler]
    private void onStoppedSprinting(StoppedSprinting _)
    {
        bumpUntil = Owner.Game.Time + Parameters.TimeAfterSprint;
    }

    [Handler]
    private void onCollidedWithLevel(CollidedWithLevel e)
    {
        if (bumpUntil < Owner.Game.Time)
            return;

        if (Owner.Game.BuildingLayer.TryGetMaterializedBuilding(e.Tile, out var building))
        {
            Events.Send(new BumpedBuilding(building));
        }
    }
}
