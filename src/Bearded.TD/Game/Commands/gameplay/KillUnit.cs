﻿using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class KillUnit
    {
        public static ISerializableCommand<GameInstance> Command(EnemyUnit unit, Faction faction) =>
            new Implementation(unit, faction);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly EnemyUnit unit;
            private readonly Faction? killingBlowFaction;

            public Implementation(EnemyUnit unit, Faction? killingBlowFaction)
            {
                this.unit = unit;
                this.killingBlowFaction = killingBlowFaction;
            }

            public void Execute() => unit.Kill(killingBlowFaction);
            public ICommandSerializer<GameInstance> Serializer => new Serializer(unit, killingBlowFaction);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<EnemyUnit> unit;
            private Id<Faction> killingBlowFaction;

            [UsedImplicitly]
            public Serializer()
            {
            }

            public Serializer(EnemyUnit unit, Faction? killingBlowFaction)
            {
                this.unit = unit.Id;
                this.killingBlowFaction = killingBlowFaction?.Id ?? Id<Faction>.Invalid;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new Implementation(
                    game.State.Find(unit),
                    killingBlowFaction.IsValid ? game.State.FactionFor(killingBlowFaction) : null);
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref unit);
                stream.Serialize(ref killingBlowFaction);
            }
        }
    }
}
