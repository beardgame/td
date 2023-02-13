using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageResistances : Component, IPreviewListener<PreviewTakeDamage>
{
    private readonly ImmutableDictionary<DamageType, float> resistances;

    public DamageResistances(ImmutableDictionary<DamageType, float> resistances)
    {
        this.resistances = resistances;
    }

    protected override void OnAdded() { }
    protected override void OnAdded()
    {
        Events.Subscribe<PreviewTakeDamage>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<PreviewTakeDamage>(this);
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
}
