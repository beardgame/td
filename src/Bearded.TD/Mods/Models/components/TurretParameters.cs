using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public TimeSpan ShootInterval { get; } = new TimeSpan(0.15);
        public TimeSpan IdleInterval { get; } = new TimeSpan(0.3);
        public TimeSpan ReCalculateTilesInRangeInterval { get; } = 1.S();

        public Unit Range { get; } = 5.U();
        public int Damage { get; } = 10;

        public TurretParameters()
        {
            
        }

        [JsonConstructor]
        public TurretParameters(TimeSpan? shootInterval, TimeSpan? idleInterval,
            TimeSpan? reCalculateTilesInRangeInterval,
            Unit? range, int? damage)
        {
            ShootInterval = shootInterval ?? ShootInterval;
            IdleInterval = idleInterval ?? IdleInterval;
            ReCalculateTilesInRangeInterval = reCalculateTilesInRangeInterval ?? ReCalculateTilesInRangeInterval;

            Range = range ?? Range;
            Damage = damage ?? Damage;
        }
    }
}
