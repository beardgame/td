using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Water
{
    [Component("waterGenerator")]
    class WaterGenerator<T> : Component<T, IWaterGeneratorParameters>
        where T : GameObject, IPositionable
    {
        public WaterGenerator(IWaterGeneratorParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var tile = Level.GetTile(Owner.Position.XY());

            Owner.Game.FluidLayer.Water.Add(tile, Parameters.VolumePerSecond * elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
