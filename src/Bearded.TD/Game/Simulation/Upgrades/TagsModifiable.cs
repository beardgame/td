using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class TagsModifiable : UpgradeEffectBase
{
    private readonly ImmutableArray<string> tagsToAdd;

    public TagsModifiable(ImmutableArray<string> tagsToAdd, UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.tagsToAdd = tagsToAdd;
    }

    public override bool ContributesComponent => true;

    public override IComponent CreateComponent() => ContributeTags.FromStringList(tagsToAdd);
}
