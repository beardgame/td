using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Simulation.Statistics.Serialization;

sealed class SerializableTypedAccumulatedDamage
{
    private DamageType type;
    private float damageDone;
    private float attemptedDamage;

    public SerializableTypedAccumulatedDamage() { }

    public SerializableTypedAccumulatedDamage(TypedAccumulatedDamage instance)
    {
        type = instance.Type;
        damageDone = instance.DamageDone.Amount.NumericValue;
        attemptedDamage = instance.AttemptedDamage.Amount.NumericValue;
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref type);
        stream.Serialize(ref damageDone);
        stream.Serialize(ref attemptedDamage);
    }

    public TypedAccumulatedDamage ToInstance() => new(
        type,
        new AccumulatedDamage(
            new UntypedDamage(damageDone.HitPoints()),
            new UntypedDamage(attemptedDamage.HitPoints())));
}
