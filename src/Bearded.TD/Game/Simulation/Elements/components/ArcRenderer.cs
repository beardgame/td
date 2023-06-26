using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("arc")]
sealed class ArcRenderer : Component<ArcRenderer.IParameters>, IListener<DrawComponents>
{
    private static readonly Color defaultColor = Color.LightCyan;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }

        Color? Color { get; }

        [Modifiable(1)]
        Unit Size { get; }
    }

    private GameObject source = null!;
    private GameObject target = null!;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public ArcRenderer(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        base.Activate();

        source = Owner.TryGetSingleComponent<IProperty<Source>>(out var sourceProperty)
            ? sourceProperty.Value.Object
            : Owner.Parent ?? Owner;

        if (!Owner.TryGetSingleComponent<IProperty<Target>>(out var targetProperty))
        {
            throw new InvalidOperationException("Cannot draw arc without target.");
        }

        target = targetProperty.Value.Object;
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(DrawComponents @event)
    {
        var baseHeight = sprite.Sprite.SpriteParameters.BaseSize.Y;
        var halfDrawWidth = baseHeight * Parameters.Size.NumericValue;
        var from = source.Position;
        var to = target.Position;
        var diff = (to - from).XY();
        var directionV = diff.NumericValue.Normalized();
        var offset = new Difference2(directionV.PerpendicularRight * halfDrawWidth).WithZ();

        @event.Drawer.DrawQuad(
            sprite,
            (from - offset).NumericValue,
            (to - offset).NumericValue,
            (to + offset).NumericValue,
            (from + offset).NumericValue,
            Parameters.Color ?? defaultColor);
    }
}
