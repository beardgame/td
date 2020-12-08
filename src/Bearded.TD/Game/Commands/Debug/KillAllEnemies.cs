using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class KillAllEnemies
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game, Faction faction)
            => new Implementation(game, faction);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction killingBlowFaction;

            public Implementation(GameInstance game, Faction killingBlowFaction)
            {
                this.game = game;
                this.killingBlowFaction = killingBlowFaction;
            }

            public override void Execute()
            {
                foreach (var enemy in game.State.GameObjects.OfType<EnemyUnit>())
                {
                    enemy.Execute(killingBlowFaction);
                }
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(killingBlowFaction);

            private class Serializer : UnifiedRequestCommandSerializer
            {
                private Id<Faction> faction;

                // ReSharper disable once UnusedMember.Local
                public Serializer() { }

                public Serializer(Faction faction)
                {
                    this.faction = faction.Id;
                }

                protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                    => new Implementation(game, game.State.FactionFor(faction));

                public override void Serialize(INetBufferStream stream)
                {
                    stream.Serialize(ref faction);
                }
            }
        }
    }
}
