using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AddFaction
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Faction faction)
            => new Implementation(game, faction);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Faction faction;

            public Implementation(GameInstance game, Faction faction)
            {
                this.game = game;
                this.faction = faction;
            }

            public void Execute()
            {
                game.State.AddFaction(faction);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(faction);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Faction> id;
            private Id<Faction> parent;
            private bool hasResources;
            private bool hasWorkerNetwork;
            private bool hasWorkers;
            private string name;
            private Color? color;

            public Serializer(Faction faction)
            {
                id = faction.Id;
                parent = faction.Parent?.Id ?? Id<Faction>.Invalid;
                hasResources = faction.HasResources;
                hasWorkers = faction.HasWorkers;
                name = faction.Name;
                color = faction.Color;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(
                    game,
                    new Faction(
                        id,
                        parent.IsValid ? game.State.FactionFor(parent) : null,
                        hasResources,
                        hasWorkers,
                        hasWorkerNetwork,
                        name,
                        color));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref parent);
                stream.Serialize(ref hasResources);
                stream.Serialize(ref hasWorkerNetwork);
                stream.Serialize(ref hasWorkers);
                stream.Serialize(ref name);
                stream.Serialize(ref color, 0U);
            }
        }
    }
}
