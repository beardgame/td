using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class GameObjectTags : Component
{
    private readonly HashSet<string> tags = new();

    public ImmutableHashSet<string> Tags => tags.ToImmutableHashSet();

    protected override void OnAdded() { }

    public override void Update(TimeSpan elapsedTime) { }

    public void AddRange(IEnumerable<string> tagsToAdd)
    {
        tagsToAdd.ForEach(t => tags.Add(t));
    }
}
