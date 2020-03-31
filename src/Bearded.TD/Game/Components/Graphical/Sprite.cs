using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Graphical
{
    [Component("sprite")]
    class Sprite<T> : Component<T, ISpriteParameters>
        where T : IPositionable
    {
        private IDirected ownerAsDirected;

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

            var angle = Maybe
                .FromNullable(ownerAsDirected)
                .Select(adjustSpriteAngleToDirection)
                .ValueOrDefault(0);

            Parameters.Sprite.Draw(p, Parameters.Color, Parameters.Size.NumericValue, angle);
        }

        private static readonly Func<IDirected, float> adjustSpriteAngleToDirection =
            directed => (directed.Direction - 90.Degrees()).Radians;
    }
}
