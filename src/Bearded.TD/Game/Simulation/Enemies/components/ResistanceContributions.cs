using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Enemies;

[Component("resistanceContributions")]
sealed class ResistanceContributions : Component<ResistanceContributions.IParameters>, IResistanceContributions
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableDictionary<SocketShape, Resistance> Factors { get; }
    }

    public ImmutableDictionary<SocketShape, Resistance> Factors => Parameters.Factors;

    public ResistanceContributions(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }
}
