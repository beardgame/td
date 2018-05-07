using OpenTK;

namespace Bearded.UI
{
    public struct Frame
    {
        public Interval X { get; }
        public Interval Y { get; }

        public Vector2d TopLeft => new Vector2d(X.Start, Y.Start);
        public Vector2d Size => new Vector2d(X.Size, Y.Size);
        
        public Frame(Interval x, Interval y)
        {
            X = x;
            Y = y;
        }
    }
}
