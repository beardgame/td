using System.Collections.Immutable;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageResistances(ImmutableDictionary<DamageType, Resistance> resistances)
    : DamageModifier, IListener<DrawComponents>
{
    protected override DamageShell AffectedShell => DamageShell.Health;

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        base.OnAdded();
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe(this);
    }

    public override void ModifyDamage(ref DamagePreview preview)
    {
        if (resistances.TryGetValue(preview.DamageType, out var resistance))
        {
            preview.Resist(resistance);
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
