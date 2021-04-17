namespace Bearded.TD.Tiles
{
    readonly struct Step
    {
        public readonly int X;
        public readonly int Y;

        public Step(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Step(Direction direction)
        {
            this = direction.Step();
        }

        public static Step operator *(Step step, int factor) => new(step.X * factor, step.Y * factor);
        public static Step operator *(int factor, Step step) => new(step.X * factor, step.Y * factor);
        public static Step operator +(Step left, Step right) => new(left.X + right.X, left.Y + right.Y);
    }
}
