using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    class UnitDeath : ICommand
    {
        public static ICommand Command(GameUnit unit, Faction faction) => new UnitDeath(unit, faction);

        private readonly GameUnit unit;
        private readonly Faction killingBlowFaction;

        private UnitDeath(GameUnit unit, Faction killingBlowFaction)
        {
            this.unit = unit;
            this.killingBlowFaction = killingBlowFaction;
        }

        public void Execute() => unit.Kill(killingBlowFaction);
        public ICommandSerializer Serializer { get; }
    }
}