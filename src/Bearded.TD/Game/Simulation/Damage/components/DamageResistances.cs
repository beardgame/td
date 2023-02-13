using System.Collections.Immutable;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageResistances : Component, IPreviewListener<PreviewTakeDamage>, IListener<DrawComponents>
{
    private readonly ImmutableDictionary<DamageType, Resistance> resistances;

    public DamageResistances(ImmutableDictionary<DamageType, Resistance> resistances)
    {
        this.resistances = resistances;
    }

    protected override void OnAdded()
    {
        Events.Subscribe<PreviewTakeDamage>(this);
        Events.Subscribe<DrawComponents>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<PreviewTakeDamage>(this);
        Events.Unsubscribe<DrawComponents>(this);
        base.OnRemoved();
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }

    public void PreviewEvent(ref PreviewTakeDamage @event)
    {
        if (resistances.TryGetValue(@event.TypedDamage.Type, out var resistance))
        {
            @event = @event.ResistedWith(resistance);
        }
    }

    public void HandleEvent(DrawComponents @event)
    {
        if (UserSettings.Instance.Debug.DamageResistances)
        {
            debugDrawResistances(@event.Core);
        }
    }

    private void debugDrawResistances(CoreDrawers coreDrawers)
    {
        const float radius = 0.3f;
        const float fullResistanceLineWidth = radius / 6;

        var currentRadius = radius;
        foreach (var (damageType, amount) in resistances)
        {
            var lineWidth = fullResistanceLineWidth * amount.NumericValue;
            coreDrawers.Primitives.DrawCircle(
                Owner.Position.NumericValue.Xy, currentRadius, lineWidth, damageType.GetColor());
            currentRadius -= lineWidth;
        }
    }
}
