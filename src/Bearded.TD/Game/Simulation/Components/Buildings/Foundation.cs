using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Components.Buildings
{
    [Component("foundation")]
    sealed class Foundation : Component<Building>
    {
        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            var sprite = Owner.Game.Meta.Blueprints
                .Sprites[ModAwareId.ForDefaultMod("buildings")]
                .Sprites.GetSprite("base-32px");

            var z = Owner.Position.Z;

            foreach (var tile in Owner.OccupiedTiles)
            {
                var position = Level
                    .GetPosition(tile)
                    .WithZ(z + 0.1.U())
                    .NumericValue;

                sprite.Draw(position, Color.White, 2.5f);
            }
        }
    }
}
