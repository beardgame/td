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
        Faction,
        FactionMultiplied
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

    protected override void OnAdded() {}

    public override void Activate()
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
        var color = GetColor(Owner, Parameters.ColorMode, Parameters.Color);

        var p = Owner.Position.NumericValue;
        p.Z += Parameters.HeightOffset.NumericValue;

        if (Parameters.Offset is var offset && offset != Difference2.Zero)
        {
            var unitY = Owner.Direction.Vector;

            var o = unitY * offset.Y + unitY.PerpendicularRight * offset.X;

            p.Xy += o.NumericValue;
        }

        e.Drawer.DrawSprite(sprite, p, Parameters.Size.NumericValue, Owner.Direction.Radians, color);
    }

    public static Color GetColor(GameObject obj, ColorMode mode, Color defaultColor)
    {
        return mode switch
        {
            ColorMode.Faction when getFactionColor(obj, out var factionColor) => factionColor,
            ColorMode.FactionMultiplied when getFactionColor(obj, out var factionColor) => multiply(factionColor, defaultColor),
            _ => defaultColor,
        };

        static Color multiply(Color c1, Color c2)
        {
            return new Color(
                (byte)(c1.R * c2.R / 255),
                (byte)(c1.G * c2.G / 255),
                (byte)(c1.B * c2.B / 255),
                (byte)(c1.A * c2.A / 255)
                );
        }

        static bool getFactionColor(GameObject obj, out Color color)
        {
            if (obj.TryGetSingleComponentInOwnerTree<IFactionProvider>(out var factionProvider))
            {
                color = factionProvider.Faction.Color;
                return true;
            }

            color = default;
            return false;
        }
    }
}
