using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering.Vertices;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing
{
    [Component("sprite")]
    class Sprite<T> : Component<T, ISpriteParameters>, IDrawableComponent
        where T : IGameObject, IPositionable
    {
        private IDirected? ownerAsDirected;
        private SpriteDrawInfo<UVColorVertex, Color> sprite;

        public Sprite(ISpriteParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Shader, Parameters.Sprite);

            ownerAsDirected = Owner as IDirected;
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public void Draw(IComponentDrawer drawer)
        {
            var p = Owner.Position.NumericValue;
            p.Z += Parameters.HeightOffset.NumericValue;

            var angle = ownerAsDirected != null
                ? (ownerAsDirected.Direction - 90.Degrees()).Radians
                : 0f;

            drawer.DrawSprite(sprite, p, Parameters.Size.NumericValue, angle, Parameters.Color);
        }
    }
}
