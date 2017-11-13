using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AddFaction
    {
        public static ICommand<GameInstance> Command(GameInstance game, Faction faction)
            => new Implementation(game, faction);

        private class Implementation : ICommand<GameInstance>
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
            private Color? color;

            public Serializer(Faction faction)
            {
                id = faction.Id;
                parent = faction.Parent?.Id ?? Id<Faction>.Invalid;
                hasResources = faction.HasResources;
                color = faction.Color;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(
                    game,
                    new Faction(id, parent.IsValid ? game.State.FactionFor(parent) : null, hasResources, color));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref parent);
                stream.Serialize(ref hasResources);
                stream.Serialize(ref color, 0U);
            }
        }
    }
}
