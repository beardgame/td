using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

[Component("threat")]
sealed class Threat : Component<Threat.IParameters>, IThreat
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable]
        float Threat { get; }
    }

    public float ThreatCost => Parameters.Threat;

    public Threat(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

interface IThreat
{
    float ThreatCost { get; }
}
