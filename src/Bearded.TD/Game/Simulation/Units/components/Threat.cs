using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

[Component("threat")]
sealed class Threat : Component<Content.Models.IThreat>, IThreat
{
    public float ThreatCost => Parameters.Threat;

    public Threat(Content.Models.IThreat parameters) : base(parameters) { }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

interface IThreat
{
    float ThreatCost { get; }
}
