using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("muzzleFlash")]
sealed class MuzzleFlash(MuzzleFlash.IParameters parameters)
    : Component<MuzzleFlash.IParameters>(parameters), IListener<DrawComponents>, IListener<ShotProjectile>
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

    private readonly record struct Flash(Position3 Position, Direction2 Direction, float Size, Instant DeathTime);

    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    private readonly List<Flash> currentFlashes = [];

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

    protected override void OnRemoved()
    {
        Events.Unsubscribe<DrawComponents>(this);
        Events.Unsubscribe<ShotProjectile>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(ShotProjectile e)
    {
        currentFlashes.Add(new Flash(
            e.Position + (e.MuzzleDirection * Parameters.Offset).WithZ(),
            e.MuzzleDirection,
            Parameters.Size * Random.Shared.NextFloat(0.75f, 1f),
            Owner.Game.Time + Parameters.MinDuration
        ));
    }

    public void HandleEvent(DrawComponents e)
    {
        foreach (var flash in currentFlashes)
        {
            e.Drawer.DrawSprite(
                sprite,
                flash.Position.NumericValue,
                flash.Size,
                flash.Direction,
                Parameters.Color);

            e.Core.PointLight.Draw(
                flash.Position.NumericValue,
                2 * flash.Size,
                Parameters.Color.WithAlpha(255) * 0.5f);
        }

        currentFlashes.RemoveAll(f => Owner.Game.Time > f.DeathTime);
    }
}
