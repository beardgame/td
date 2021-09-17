using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities
{
    static class SpaceTimeExtensions
    {
        public static TimeSpan S(this int val) => new(val);
        public static TimeSpan S(this double val) => new(val);

        public static Speed UnitsPerSecond(this int val) => new(val);
        public static Speed UnitsPerSecond(this float val) => new(val);

        public static Frequency PerSecond(this int val) => new(val);
        public static Frequency PerSecond(this double val) => new(val);

        public static Acceleration UnitsPerSecondSquared(this int val) => new(val);
        public static Acceleration UnitsPerSecondSquared(this float val) => new(val);


        public static Velocity3 WithZ(this Velocity2 xy) => xy.WithZ(Speed.Zero);

        public static Position3 WithZ(this Position2 xy) => xy.WithZ(Unit.Zero);

        public static Velocity3 WithZ(this Velocity2 xy, Speed z) => new(xy.X, xy.Y, z);

        public static Acceleration3 WithZ(this Acceleration2 xy, Acceleration z) => new(xy.X, xy.Y, z);

        public static Position2 XY(this Position3 xyz) => new(xyz.X, xyz.Y);

        public static Difference2 XY(this Difference3 xyz) => new(xyz.X, xyz.Y);

        public static Velocity2 XY(this Velocity3 xyz) => new(xyz.X, xyz.Y);
    }
}
