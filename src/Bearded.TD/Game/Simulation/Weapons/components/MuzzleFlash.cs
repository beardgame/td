using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("muzzleFlash")]
sealed class MuzzleFlash : Component<MuzzleFlash.IParameters>,
    IListener<DrawComponents>, IListener<ShotProjectile>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Color Color { get; }
        Shader? Shader { get; }
        float Size { get; }
        Unit Offset { get; }

        [Modifiable(0.03)]
        TimeSpan MinDuration { get; }
    }

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

    public MuzzleFlash(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe<ShotProjectile>(this);
    }

    public override void Activate()
    {
        base.Activate();

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader);
        Events.Subscribe<DrawComponents>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<DrawComponents>(this);
        Events.Unsubscribe<ShotProjectile>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
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

    public void HandleEvent(DrawComponents e)
    {
        if (currentFlash is not { } flash)
            return;

        e.Drawer.DrawSprite(
            sprite,
            flash.Position.NumericValue,
            flash.Size,
            flash.Direction.Radians,
            Parameters.Color);

        e.Core.PointLight.Draw(
            flash.Position.NumericValue,
            2 * flash.Size,
            Parameters.Color.WithAlpha(255) * 0.5f);

        if (Owner.Game.Time > flash.DeathTime)
            currentFlash = null;
    }
}
