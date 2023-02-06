using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Enemies;

[Component("archetype")]
sealed class ArchetypeProperty : Component<ArchetypeProperty.IParameters>, IProperty<Archetype>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Archetype Value { get; }
    }

    public Archetype Value => Parameters.Value;

    public ArchetypeProperty(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }
}
