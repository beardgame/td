using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class TurretParameters
    {
        public TimeSpan ShootInterval { get; } = new TimeSpan(0.15);
        public TimeSpan IdleInterval { get; } = new TimeSpan(0.3);
        public TimeSpan ReCalculateTilesInRangeInterval { get; } = 1.S();

        public Unit Range { get; } = 5.U();
        public int Damage { get; } = 10;
    }
}
