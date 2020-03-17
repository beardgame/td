using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Graphical
{
    [Component("sprite")]
    class Sprite<T> : Component<T, ISpriteParameters>
        where T : IPositionable
    {
        private IDirected? ownerAsDirected;

        public Sprite(ISpriteParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            ownerAsDirected = Owner as IDirected;
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
            var p = Owner.Position.NumericValue;
            p.Z += Parameters.HeightOffset.NumericValue;

            var angle = ownerAsDirected?.Direction.Radians ?? 0;

            Parameters.Sprite.Draw(p, Parameters.Color, Parameters.Size.NumericValue, angle);
        }
    }
}
