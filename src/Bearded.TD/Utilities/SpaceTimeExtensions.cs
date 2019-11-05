using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities
{
    static class SpaceTimeExtensions
    {
        public static TimeSpan S(this int val) => new TimeSpan(val);
        public static TimeSpan S(this double val) => new TimeSpan(val);

        public static Speed UnitsPerSecond(this int val) => new Speed(val);
        public static Speed UnitsPerSecond(this float val) => new Speed(val);

        public static Frequency PerSecond(this int val) => new Frequency(val);
        public static Frequency PerSecond(this double val) => new Frequency(val);

        public static Acceleration UnitsPerSecondSquared(this int val) => new Acceleration(val);
        public static Acceleration UnitsPerSecondSquared(this float val) => new Acceleration(val);
    }
}
