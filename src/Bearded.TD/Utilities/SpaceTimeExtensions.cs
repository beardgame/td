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


        public static Velocity3 WithZ(this Velocity2 xy) => xy.WithZ(Speed.Zero);

        public static Position3 WithZ(this Position2 xy) => xy.WithZ(Unit.Zero);

        public static Velocity3 WithZ(this Velocity2 xy, Speed z) => new Velocity3(xy.X, xy.Y, z);

        public static Acceleration3 WithZ(this Acceleration2 xy, Acceleration z) => new Acceleration3(xy.X, xy.Y, z);

        public static Position2 XY(this Position3 xyz) => new Position2(xyz.X, xyz.Y);

        public static Difference2 XY(this Difference3 xyz) => new Difference2(xyz.X, xyz.Y);

        public static Velocity2 XY(this Velocity3 xyz) => new Velocity2(xyz.X, xyz.Y);
    }
}
