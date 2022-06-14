using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.UI.Controls;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("sprite")]
class Sprite : Component<Sprite.IParameters>, IListener<DrawComponents>
{
    internal enum ColorMode
    {
        Default = 0,
        Faction
    }

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        Color Color { get; }
        ColorMode ColorMode { get;  }
        ISpriteBlueprint Sprite { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }
        [Modifiable(1)]
        Unit Size { get; }
        Unit HeightOffset { get; }
        Difference2 Offset { get; }
    }


    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public Sprite(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

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
        var color = Parameters.ColorMode switch
        {
            ColorMode.Faction when
                Owner.TryGetSingleComponentInOwnerTree<IFactionProvider>(out var factionProvider)
                => factionProvider.Faction.Color,
            _ => Parameters.Color,
        };

        var p = Owner.Position.NumericValue;
        p.Z += Parameters.HeightOffset.NumericValue;

        if (Parameters.Offset is var offset && offset != Difference2.Zero)
        {
            var unitY = Owner.Direction.Vector;

            var o = unitY * offset.Y + unitY.PerpendicularRight * offset.X;

            p.Xy += o.NumericValue;
        }

        // sprites are normalised to point 'up', this turns them to point 'right'
        var angle = (Owner.Direction - 90.Degrees()).Radians;

        e.Drawer.DrawSprite(sprite, p, Parameters.Size.NumericValue, angle, color);
    }
}
