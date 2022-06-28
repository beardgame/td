using System.Collections.Immutable;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("tags")]
sealed class GameObjectTags : Component<GameObjectTags.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableHashSet<string> Tags { get; }
    }

    public GameObjectTags(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Update(TimeSpan elapsedTime) { }

    public bool HasTag(string tag) => Parameters.Tags.Contains(tag);
}
