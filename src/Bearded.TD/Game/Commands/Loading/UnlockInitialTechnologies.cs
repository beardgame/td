using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class UnlockInitialTechnologies
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
                foreach (var blueprint in game.Blueprints.Buildings.All)
                {
                    faction.Technology.UnlockBuilding(blueprint);
                }

                foreach (var upgrade in game.Blueprints.Upgrades.Values)
                {
                    faction.Technology.UnlockUpgrade(upgrade);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(faction);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Faction> faction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction)
            {
                this.faction = faction.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, game.State.FactionFor(faction));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
            }
        }
    }
}

