using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class AddTags : AddComponent
{
    private readonly ImmutableArray<string> tagsToAdd;

    public AddTags(ImmutableArray<string> tagsToAdd, UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.tagsToAdd = tagsToAdd;
    }

    protected override IComponent CreateComponent() => ContributeTags.FromStringList(tagsToAdd);
}
