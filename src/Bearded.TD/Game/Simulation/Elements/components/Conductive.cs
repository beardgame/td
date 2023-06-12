using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using static System.Math;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

interface IConductive
{
    void Conduct(ref ArcTree.ArcContinuation arc);
}

[Component("conductive")]
sealed class Conductive : Component, IConductive
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        Owner.TrackTilePresenceInLayer(Owner.Game.ConductiveLayer);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void Conduct(ref ArcTree.ArcContinuation arc)
    {
        arc = arc with
        {
            BouncesLeft = arc.BouncesLeft + 1,
            Branches = Max(arc.Branches, 1),
            MaxBounceDistance = Max(arc.MaxBounceDistance, 2),
        };
    }
}
