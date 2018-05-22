using OpenTK;

namespace Bearded.UI
{
    static class FrameExtensions
    {
        public static bool ContainsCoordinate(this Interval interval, double coordinate)
        {
            return coordinate >= interval.Start && coordinate <= interval.End;
        }

        public static bool ContainsPoint(this Frame frame, Vector2d point)
        {
            return frame.X.ContainsCoordinate(point.X) && frame.Y.ContainsCoordinate(point.Y);
        }
    }
}
