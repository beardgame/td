using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Resources;

static class GrantResources
{
    public static IRequest<Player, GameInstance> Request<T>(Faction faction, Resource<T> amount)
        where T : IResourceType
        => new Implementation(faction, getResourceTypeId<T>(), amount.Value);

    private static byte getResourceTypeId<T>() where T : IResourceType
    {
        return default(T) switch
        {
            Scrap => 1,
            CoreEnergy => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(T), typeof(T), null),
        };
    }

    private static Action<FactionResources, double> providerFunction(byte resourceTypeId)
    {
        return resourceTypeId switch
        {
            1 => (r, v) => r.ProvideResources(new Resource<Scrap>(v)),
            2 => (r, v) => r.ProvideResources(new Resource<CoreEnergy>(v)),
            _ => throw new ArgumentOutOfRangeException(nameof(resourceTypeId), resourceTypeId, null),
        };
    }

    private sealed class Implementation(Faction faction, byte id, double value)
        : UnifiedDebugRequestCommand
    {

        protected override bool CheckPreconditionsDebug(Player player) =>
            faction.TryGetBehavior<FactionResources>(out _);

        public override void Execute()
        {
            if (!faction.TryGetBehavior<FactionResources>(out var resources))
            {
                throw new InvalidOperationException(
                    "Cannot add resources without resources for the faction. Precondition should have failed.");
            }

            providerFunction(id)(resources, value);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, id, value);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private byte id;
        private double amount;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(Faction faction, byte id, double amount)
        {
            this.faction = faction.Id;
            this.id = id;
            this.amount = amount;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            => new Implementation(game.State.Factions.Resolve(faction), id, amount);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref id);
            stream.Serialize(ref amount);
        }
    }
}
