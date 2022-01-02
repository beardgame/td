using static System.Math;

namespace Bearded.TD.Tiles;

public readonly struct Step
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

    public static Step Between(Tile origin, Tile target) =>
        new(target.X - origin.X, target.Y - origin.Y);

    public static Step FromOriginTowards(Tile tile) => new(tile.X, tile.Y);

    public int Distance =>
        X * Y < 0
            ? Max(Abs(X), Abs(Y))
            : Abs(X + Y);

    public static Step operator *(Step step, int factor) => new(step.X * factor, step.Y * factor);
    public static Step operator *(int factor, Step step) => new(step.X * factor, step.Y * factor);
    public static Step operator +(Step left, Step right) => new(left.X + right.X, left.Y + right.Y);
    public static Tile operator +(Tile tile, Step step) => new(tile.X + step.X, tile.Y + step.Y);
}