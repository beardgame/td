using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Rules;
using Bearded.TD.Game.Rules.Buildings;
using Bearded.TD.Game.Rules.Technologies;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Commands
{
    static class ApplyGameRules
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                foreach (var rule in hardCodedGameRules(game))
                {
                    rule.OnAdded(game.State, game.Meta.Events);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer() {}

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => new Implementation(game);

            public void Serialize(INetBufferStream stream) {}
        }

        private static IEnumerable<IGameRule<GameState>> hardCodedGameRules(GameInstance game)
        {
            var blueprints = game.Blueprints;

            yield return new UnlockTechnologiesRule(new UnlockTechnologiesRule.Parameters(new List<ITechnologyUnlock>
            {
                new BuildingUnlock(blueprints.Buildings["wall"]),
                new BuildingUnlock(blueprints.Buildings["triangleTurret"])
            }));

            yield return new PlaceBuildingRule(new PlaceBuildingRule.Parameters(
                blueprints.Buildings["base"],
                blueprints.Footprints["seven"].Positioned(0, game.State.Level, new Position2(0, 0))));
        }
    }
}
