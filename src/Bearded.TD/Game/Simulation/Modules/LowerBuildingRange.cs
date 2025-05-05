using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("lowerBumpedBuildingRange")]
sealed class LowerWeaponRange(LowerWeaponRange.IParameters parameters)
    : Component<LowerWeaponRange.IParameters>(parameters), IListener<BumpedBuilding>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan Duration { get; }

        [Modifiable(0.5f)]
        double Factor { get; }
    }

    public override void Activate()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(BumpedBuilding e)
    {
        Owner.Sync(LowerBuildingRange.Command, e.Building, Parameters.Duration, Parameters.Factor);
    }
}
