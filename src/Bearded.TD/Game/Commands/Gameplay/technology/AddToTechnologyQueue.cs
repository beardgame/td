using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class AddToTechnologyQueue
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, ITechnologyBlueprint technology)
            => new Implementation(faction, technology);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;
            private readonly ITechnologyBlueprint technology;

            public Implementation(Faction faction, ITechnologyBlueprint technology)
            {
                this.technology = technology;
                this.faction = faction;
            }

            public override bool CheckPreconditions(Player actor) =>
                faction.SharesTechnologyWith(actor.Faction)
                && faction.Technology.CanQueueTechnology(technology)
                && !faction.Technology.IsTechnologyQueued(technology);

            public override void Execute()
            {
                faction.Technology.AddToTechnologyQueue(technology);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, technology);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private ModAwareId technology;

            [UsedImplicitly]
            public Serializer()
            {
            }

            public Serializer(Faction faction, ITechnologyBlueprint technology)
            {
                this.faction = faction.Id;
                this.technology = technology.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
                new Implementation(game.State.Factions.Resolve(faction), game.Blueprints.Technologies[technology]);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref technology);
            }
        }
    }
}
