using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("sprite")]
class Sprite<T> : Component<T, ISpriteParameters>, IListener<DrawComponents>
    where T : IGameObject, IPositionable
{
    private IDirected? ownerAsDirected;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public Sprite(ISpriteParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

        ownerAsDirected = Owner as IDirected;

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var p = Owner.Position.NumericValue;
        p.Z += Parameters.HeightOffset.NumericValue;

        var angle = ownerAsDirected != null
            ? (ownerAsDirected.Direction - 90.Degrees()).Radians
            : 0f;

        e.Drawer.DrawSprite(sprite, p, Parameters.Size.NumericValue, angle, Parameters.Color);
    }
}
