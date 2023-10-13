using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using static System.Math;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

interface IConductive
{
    void Conduct(ref ArcTree.Continuation arc);
}

[Component("conductive")]
sealed class Conductive : Component, IConductive
{
    private readonly MutableArea area = new();

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        Owner.TrackTilePresenceInLayer(Owner.Game.ConductiveLayer);
        Owner.GetTilePresence().ObserveChanges(area.Add, area.Remove);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void Conduct(ref ArcTree.Continuation arc)
    {
        arc = arc with
        {
            BouncesLeft = arc.BouncesLeft + 1,
            Branches = Max(arc.Branches, 1),
            MaxBounceDistance = Max(arc.MaxBounceDistance, 2),
            CoveringTiles = area
        };
    }
}
