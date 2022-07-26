using System.Collections.Immutable;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("tags")]
sealed class ContributeTags : Component<ContributeTags.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableArray<string> Tags { get; }
    }

    public ContributeTags(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        if (Owner.TryGetSingleComponent<GameObjectTags>(out var tags))
        {
            tags.AddRange(Parameters.Tags);
        }
    }

    public override void Update(TimeSpan elapsedTime) {}

    public static ContributeTags FromStringList(ImmutableArray<string> tagsToAdd)
    {
        return new ContributeTags(new ContributeTagsParametersTemplate(tagsToAdd));
    }
}
