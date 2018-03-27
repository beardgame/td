using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class WeaponBlueprint : IBlueprint
    {
        public string Id { get; }
        public TimeSpan ShootInterval { get; }
        public TimeSpan IdleInterval { get; }
        public TimeSpan ReCalculateTilesInRangeInterval { get; }
        public Unit Range { get; }
        public int Damage { get; }

        public WeaponBlueprint(
                string id,
                TimeSpan shootInterval,
                TimeSpan idleInterval,
                TimeSpan reCalculateTilesInRangeInterval,
                Unit range,
                int damage)
        {
            Id = id;
            ShootInterval = shootInterval;
            IdleInterval = idleInterval;
            ReCalculateTilesInRangeInterval = reCalculateTilesInRangeInterval;
            Range = range;
            Damage = damage;
        }
    }
}
