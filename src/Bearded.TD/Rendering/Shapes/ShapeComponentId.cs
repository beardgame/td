namespace Bearded.TD.Rendering.Shapes;

readonly struct ShapeComponentId(uint value)
{
    public static ShapeComponentId None => default;
    public readonly uint Value = value;

    public bool IsNone => Value == 0;

    public override string ToString() => IsNone ? "None" : $"Id({Value})";
}

readonly record struct ShapeComponentIds(ShapeComponentId First, int Count)
{
    public static implicit operator ShapeComponentIds((ShapeComponentId First, int Count) tuple)
        => new(tuple.First, tuple.Count);

    public override string ToString() => Count == 0 ? "None" : $"First: {First}, Count: {Count}";
}
