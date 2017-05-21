using Bearded.TD.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    class UnitDeath : ICommand
    {
        public static ICommand Command(GameUnit unit) => new UnitDeath(unit);

        private readonly GameUnit unit;

        private UnitDeath(GameUnit unit)
        {
            this.unit = unit;
        }

        public void Execute() => unit.Kill();
        public ICommandSerializer Serializer { get; }
    }
}