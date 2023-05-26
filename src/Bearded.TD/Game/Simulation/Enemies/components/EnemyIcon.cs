using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Enemies;

[Component("enemyIcon")]
sealed class EnemyIcon : Component<EnemyIcon.IParameters>, IListener<DrawComponents>, IEnemyIcon
{
    private static readonly Difference3 iconOffset = new(-0.25f, -0.25f, 0.1f);
    private const float size = 0.3f;

    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public Color IconColor => Parameters.Element.GetColor();

    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Icon { get; }
        Element Element { get; }
    }

    public EnemyIcon(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        sprite = MakeIconSprite(Owner.Game);
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(DrawComponents @event)
    {
        var pos = Owner.Position + iconOffset;
        @event.Drawer.DrawSprite(sprite, pos.NumericValue, size, 0, IconColor);
    }

    public SpriteDrawInfo<UVColorVertex, Color> MakeIconSprite(GameState game)
    {
        return SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Icon);
    }
}
