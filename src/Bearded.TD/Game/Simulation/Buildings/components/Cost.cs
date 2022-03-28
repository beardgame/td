using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("cost")]
sealed class Cost : Component<Content.Models.ICost>, ICost
{
    public ResourceAmount Resources => Parameters.Resources;

    public Cost(Content.Models.ICost parameters) : base(parameters) { }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

interface ICost
{
    ResourceAmount Resources { get; }
}
