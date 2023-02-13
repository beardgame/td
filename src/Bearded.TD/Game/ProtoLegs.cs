using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using Extensions = Bearded.Utilities.Linq.Extensions;

namespace Bearded.TD.Game;

sealed partial class ProtoLegs : Component, IListener<DrawComponents>
{
    private static Unit neutralFootDistance => 2.U();
    private static Unit maxStepDistance => 1.U();
    private static Speed stepSpeed => 10.U() / 1.S();
    private static Unit offsetRadius => 5.U();
    private static AngularVelocity offsetSpeed => AngularVelocity.FromDegrees(90);
    private static int maxLiftedGroups => 2;

    private Position2 cursor => game.PlayerInput.CursorPosition;

    private readonly List<Leg> legs = new();
    private readonly List<LegGroup> legGroups = new();

    private List<LegGroup> liftedGroups = new();

    private Position2 position;

    private Velocity2 velocity;
    private Difference2 lastError;

    public override void Update(TimeSpan elapsedTime)
    {
        ensureInitialised();

        var offset = (Direction2.Zero + offsetSpeed * (Owner.Game.Time - Instant.Zero)) * offsetRadius;

        var target = cursor;
        //target += offset;

        {
            // pid
            var error = target - position;
            var derivative = (error - lastError) / (float)elapsedTime.NumericValue;
            var a = (error * 1 + derivative * 1) / 1.S() / 1.S();
            lastError = error;
            velocity += a * elapsedTime;
        }

        position += velocity * elapsedTime;

        updateLegs(elapsedTime);

    }

    private void updateLegs(TimeSpan elapsedTime)
    {
        liftedGroups.RemoveAll(g => g.Legs.All(l => l.OnGround));

        while(liftedGroups.Count < maxLiftedGroups && liftedGroups.Count < legGroups.Count)
        {
            var (group, error) = legGroups
                .Except(liftedGroups)
                .Select(g => (Group: g, Error: g.GetError(this)))
                .MaxBy(t => t.Error);

            if (error < 2 + liftedGroups.Count)
                break;

            group.StartStep(this);
            liftedGroups.Add(group);
        }

        foreach (var leg in legs)
        {
            leg.Update(elapsedTime, this);
        }
    }

    public void HandleEvent(DrawComponents e)
    {
        var g = e.Core.Primitives;

        foreach (var leg in legs)
        {
            //g.FillCircle(leg.TargetPosition.NumericValue, 0.2f, Color.Aquamarine * 0.5f);
            g.DrawLine(position.NumericValue, leg.Position.NumericValue, 0.1f, Color.Aquamarine);

            var optimalPosition = Leg.OptimalLocationFrom(this, leg.NeutralDirection);

            g.DrawCircle(optimalPosition.NumericValue, maxStepDistance.NumericValue, 0.02f, Color.Bisque);
        }
    }
}
