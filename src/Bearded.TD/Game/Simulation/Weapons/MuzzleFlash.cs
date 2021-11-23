using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons
{
    interface IMuzzleFlashParameters : IParametersTemplate<IMuzzleFlashParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Color Color { get; }
        Shader? Shader { get; }
        float Size { get; }
        Unit Offset { get; }

        [Modifiable(0.03)]
        TimeSpan MinDuration { get; }
    }

    [Component("muzzleFlash")]
    sealed class MuzzleFlash<T> : Component<T, IMuzzleFlashParameters>, IDrawableComponent, IListener<ShotProjectile>
        where T : IGameObject, IPositionable, IDirected
    {
        private readonly struct Flash
        {
            public Position3 Position { get; }
            public Direction2 Direction { get; }
            public float Size { get; }
            public Instant DeathTime { get; }

            public Flash(Position3 position, Direction2 direction, float size, Instant deathTime)
            {
                Position = position;
                Direction = direction;
                Size = size;
                DeathTime = deathTime;
            }
        }

        private SpriteDrawInfo<UVColorVertex, Color> sprite;

        private Flash? currentFlash;

        public MuzzleFlash(IMuzzleFlashParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Shader, Parameters.Sprite);

            Events.Subscribe(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (currentFlash is not { } flash)
                return;

            drawers.PointLight.Draw(
                flash.Position.NumericValue,
                2 * flash.Size,
                Parameters.Color.WithAlpha(255) * 0.5f);
        }

        public void HandleEvent(ShotProjectile e)
        {
            currentFlash = new Flash(
                e.Position + (e.MuzzleDirection * Parameters.Offset).WithZ(),
                e.MuzzleDirection,
                Parameters.Size * StaticRandom.Float(0.75f, 1f),
                Owner.Game.Time + Parameters.MinDuration
            );
        }

        public void Draw(IComponentRenderer renderer)
        {
            if (currentFlash is not { } flash)
                return;

            renderer.DrawSprite(
                sprite,
                flash.Position.NumericValue,
                flash.Size,
                flash.Direction.Radians,
                Parameters.Color);

            if (Owner.Game.Time > flash.DeathTime)
                currentFlash = null;
        }
    }
}
