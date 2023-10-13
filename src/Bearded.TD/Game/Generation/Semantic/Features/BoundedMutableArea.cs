using System;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features;

sealed class BoundedMutableArea : MutableArea
{
    private readonly IArea bounds;

    public BoundedMutableArea(IArea bounds)
    {
        this.bounds = bounds;
        AddRange(bounds);
    }

    public override void Add(Tile tile)
    {
        if (!bounds.Contains(tile))
            throw new ArgumentOutOfRangeException(nameof(tile), "Cannot add tile out of bounds.");

        base.Add(tile);
    }

    public void Reset()
    {
        foreach (var tile in bounds)
        {
            base.Add(tile);
        }
    }
}
