using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game;

sealed partial class ProtoLegs
{
    private static int numberOfLegs => 6;
    private static int numberOfLegGroups => 1;
    
    private readonly GameInstance game;

    private bool initialised;

    public ProtoLegs(GameInstance game)
    {
        this.game = game;
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }
    
    private void ensureInitialised()
    {
        if (initialised)
            return;

        position = cursor;

        foreach (var _ in ..numberOfLegGroups)
        {
            legGroups.Add(new LegGroup());
        }

        foreach (var i in ..numberOfLegs)
        {
            var direction = Direction2.FromDegrees(360f * i / numberOfLegs);
            var p = Leg.OptimalLocationFrom(this, direction);

            var leg = new Leg
            {
                NeutralDirection = direction,
                Position = p,
                TargetPosition = p,
            };
            legs.Add(leg);
            legGroups[i % numberOfLegGroups].Legs.Add(leg);
        }

        initialised = true;
    }
}
