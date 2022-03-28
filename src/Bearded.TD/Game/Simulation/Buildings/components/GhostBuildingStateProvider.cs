using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class GhostBuildingStateProvider : Component, IBuildingStateProvider
{
    public IBuildingState State { get; } = new GhostBuildingState();

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}
