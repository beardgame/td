using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Graphical
{
    [Component("sprite")]
    class Sprite<T> : Component<T, ISpriteParameters>
        where T : IPositionable
    {
        public Sprite(ISpriteParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
            Parameters.Sprite.Draw(Owner.Position.NumericValue.WithZ(0.2f), Parameters.Color, 0.1f);
        }
    }
}
