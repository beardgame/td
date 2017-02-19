using OpenTK;

namespace Bearded.TD.Utilities
{
    struct Line
    {
        public Vector2 Start { get; }
        public Vector2 Direction { get; }

        private Line(Vector2 start, Vector2 direction)
        {
            Start = start;
            Direction = direction;
        }

        public static Line Between(Vector2 a, Vector2 b)
            => new Line(a, b - a);
        public static Line Between(float x1, float y1, float x2, float y2)
            => new Line(new Vector2(x1, x2), new Vector2(x2 - x1, y2 - y1));

        public bool IntersectsAsSegments(Line other)
        {
            var a = this.Start;
            var b = this.Direction;
            var c = other.Start;
            var d = other.Direction;

            var denominator = c.Y * a.X + c.X * a.Y;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (denominator == 0)
                return false;

            var numerator = (d.X - b.Y) * a.Y + (b.Y - d.Y) * a.X;

            var otherF = numerator / denominator;

            if (otherF <= 0 || otherF >= 1)
                return false;

            float thisF;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (a.X == 0)
            {
                thisF = (c.Y * otherF + d.Y - b.Y) / a.Y;
            }
            else
            {
                thisF = (c.X * otherF + d.X - b.X) / a.X;
            }

            return thisF > 0 && thisF < 1;
        }
    }
}