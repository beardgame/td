using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("trail")]
sealed class Trail : Component<ITrailParameters>, IListener<DrawComponents>
{
    private readonly TrailTracer tracer;
    private TrailDrawer drawer = null!;

    public Trail(ITrailParameters parameters) : base(parameters)
    {
        tracer = new TrailTracer(parameters.Timeout);
    }

    protected override void OnAdded()
    {
        if (Parameters.SurviveObjectDeletion)
        {
            Owner.Deleting += persistTrail;
        }

        drawer = new TrailDrawer(Owner.Game, Parameters.Sprite);

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        if (Parameters.SurviveObjectDeletion)
        {
            Owner.Deleting -= persistTrail;
        }

        Events.Unsubscribe(this);
    }

    private void persistTrail()
    {
        var obj = ComponentGameObjectFactory.CreateWithDefaultRenderer(Owner.Game, null, Owner.Position);
        obj.AddComponent(new PersistentTrail(drawer, Parameters, tracer));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tracer.Update(Owner.Game.Time, Owner.Position);
    }

    public void HandleEvent(DrawComponents e)
    {
        drawTrail(drawer, tracer, Parameters, Owner.Game);
    }

    private sealed class PersistentTrail : Component, IListener<DrawComponents>
    {
        private readonly TrailDrawer drawer;
        private readonly ITrailParameters parameters;
        private readonly TrailTracer tracer;
        private Instant deleteAt;

        public PersistentTrail(TrailDrawer drawer, ITrailParameters parameters, TrailTracer tracer)
        {
            this.drawer = drawer;
            this.parameters = parameters;
            this.tracer = tracer;
        }

        protected override void OnAdded()
        {
            deleteAt = Owner.Game.Time + parameters.Timeout;
            Events.Subscribe(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tracer.Update(Owner.Game.Time, Owner.Position, true);

            if (deleteAt <= Owner.Game.Time)
                Owner.Delete();
        }

        public void HandleEvent(DrawComponents e)
        {
            drawTrail(drawer, tracer, parameters, Owner.Game);
        }
    }

    private static void drawTrail(TrailDrawer drawer, TrailTracer tracer, ITrailParameters parameters, GameState game)
    {
        drawer.DrawTrail(
            tracer, parameters.Width.NumericValue,
            game.Time, parameters.Timeout,
            parameters.Color
        );
    }
}
