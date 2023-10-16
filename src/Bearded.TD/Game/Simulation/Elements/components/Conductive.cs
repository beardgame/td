using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static System.Math;

namespace Bearded.TD.Game.Simulation.Elements;

interface IConductive
{
    void Conduct(ref ArcTree.Continuation arc);
}

[Component("conductive")]
sealed class Conductive : Component<Conductive.IParameters>, IConductive
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        public int AddsBranching { get; }
    }

    private readonly MutableArea area = new();

    public Conductive(IParameters parameters) : base(parameters) { }

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
            Branches = Max(arc.Branches + Parameters.AddsBranching, 1),
            MaxBounceDistance = Unit.Max(arc.MaxBounceDistance, 2.U()),
            CoveringTiles = area
        };
    }
}
