using System.Collections.Generic;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class GameObjectTags : Component
{
    private readonly HashSet<string> tags = new();

    protected override void OnAdded() { }

    public override void Update(TimeSpan elapsedTime) { }

    public bool HasTag(string tag) => tags.Contains(tag);

    public void AddRange(IEnumerable<string> tagsToAdd)
    {
        tagsToAdd.ForEach(t => tags.Add(t));
    }
}
