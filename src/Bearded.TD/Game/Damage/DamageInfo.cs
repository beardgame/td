using Bearded.TD.Game.Buildings;
using Bearded.Utilities;

namespace Bearded.TD.Game.Damage
{
    struct DamageInfo
    {
        public int Amount { get; }
        public DamageType Type { get; }
        public Maybe<Building> Source { get; }

        public DamageInfo(int amount, DamageType type, Building building)
        {
            Amount = amount;
            Type = type;
            Source = Maybe.Just(building);
        }

        public DamageInfo(int amount, DamageType type)
        {
            Amount = amount;
            Type = type;
            Source = Maybe.Nothing;
        }
    }
}
