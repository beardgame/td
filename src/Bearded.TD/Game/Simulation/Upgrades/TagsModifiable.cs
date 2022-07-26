using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class TagsModifiable : UpgradeEffectBase
{
    private readonly ImmutableArray<string> tagsToAdd;

    public TagsModifiable(ImmutableArray<string> tagsToAdd, UpgradePrerequisites prerequisites) : base(prerequisites)
    {
        this.tagsToAdd = tagsToAdd;
    }

    public override void ApplyTo(GameObject subject)
    {
        subject.AddComponent(ContributeTags.FromStringList(tagsToAdd));
        base.ApplyTo(subject);
    }
}
