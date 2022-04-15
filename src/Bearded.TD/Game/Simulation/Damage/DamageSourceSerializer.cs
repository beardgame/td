using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageSourceSerializer
{
    private enum SupportedImplementation : byte
    {
        None,
        GameObject,
        DivineIntervention,
    }

    private byte type;
    private int id;

    public void Populate(IDamageSource? damageSource)
    {
        switch (damageSource)
        {
            case null:
                type = (byte) SupportedImplementation.None;
                break;
            case DamageSource b:
                type = (byte) SupportedImplementation.GameObject;
                id = b.Id.Value;
                break;
            case DivineIntervention:
                type = (byte) SupportedImplementation.DivineIntervention;
                break;
        }
    }

    public IDamageSource? ToDamageSource(GameInstance instance)
    {
        return type switch
        {
            (byte) SupportedImplementation.None => null,
            (byte) SupportedImplementation.GameObject => instance.State.Find(new Id<GameObject>(id)).TryGetSingleComponent<IDamageSource>(out var s) ? s : null,
            (byte) SupportedImplementation.DivineIntervention => DivineIntervention.DamageSource,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void Serialize(INetBufferStream stream)
    {
        stream.Serialize(ref type);
        stream.Serialize(ref id);
    }
}
