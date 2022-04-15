using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("waterGenerator")]
class WaterGenerator : Component<WaterGenerator.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable]
        FlowRate VolumePerSecond { get; }
    }

    public WaterGenerator(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var tile = Level.GetTile(Owner.Position.XY());

        Owner.Game.FluidLayer.Water.Add(tile, Parameters.VolumePerSecond * elapsedTime);
    }
}
