using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features;

class MutableArea : IArea
{
    private readonly HashSet<Tile> tiles = new();

    public int Count => tiles.Count;

    public bool Contains(Tile tile) => tiles.Contains(tile);

    public void AddRange(IEnumerable<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            Add(tile);
        }
    }

    public virtual void Add(Tile tile)
    {
        tiles.Add(tile);
    }

    public void Remove(Tile tile) => tiles.Remove(tile);

    public void RemoveAll() => tiles.Clear();

    public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();
}
