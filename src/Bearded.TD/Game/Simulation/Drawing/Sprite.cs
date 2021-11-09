using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing
{
    [Component("sprite")]
    class Sprite<T> : Component<T, ISpriteParameters>
        where T : IGameObject, IPositionable
    {
        private Maybe<IDirected> ownerAsDirected;

        private IDrawableSprite<Color> sprite = null!;

        public Sprite(ISpriteParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            ownerAsDirected = Maybe.FromNullable(Owner as IDirected);
            sprite = Parameters.Sprite.MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, UVColorVertex.Create,
                Parameters.Shader ?? Owner.Game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("default-sprite")]);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            var p = Owner.Position.NumericValue;
            p.Z += Parameters.HeightOffset.NumericValue;

            var angle = ownerAsDirected
                .Select(adjustSpriteAngleToDirection)
                .ValueOrDefault(0);

            sprite.Draw(p, Parameters.Size.NumericValue, angle, Parameters.Color);
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Func<IDirected, float> adjustSpriteAngleToDirection =
            directed => (directed.Direction - 90.Degrees()).Radians;
    }
}
