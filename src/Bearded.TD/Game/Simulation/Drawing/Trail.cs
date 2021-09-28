using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing
{
    [Component("trail")]
    sealed class Trail<T> : Component<T, ITrailParameters>
        where T : GameObject, IPositionable
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
        }

        private void persistTrail()
        {
            var obj = new PersistentTrail(drawer, Parameters, tracer, Owner.Position);
            Owner.Game.Add(obj);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tracer.Update(Owner.Game.Time, Owner.Position);
        }

        public override void Draw(CoreDrawers drawers)
        {
            drawTrail(drawer, tracer, Parameters, Owner.Game);
        }


        sealed class PersistentTrail : GameObject
        {
            private readonly TrailDrawer drawer;
            private readonly ITrailParameters parameters;
            private readonly TrailTracer tracer;
            private readonly Position3 position;
            private Instant deleteAt;

            public PersistentTrail(TrailDrawer drawer, ITrailParameters parameters, TrailTracer tracer,
                Position3 position)
            {
                this.drawer = drawer;
                this.parameters = parameters;
                this.tracer = tracer;
                this.position = position;
            }

            protected override void OnAdded()
            {
                deleteAt = Game.Time + parameters.Timeout;
            }

            public override void Update(TimeSpan elapsedTime)
            {
                tracer.Update(Game.Time, position, true);

                if (deleteAt <= Game.Time)
                    Delete();
            }

            public override void Draw(CoreDrawers drawers)
            {
                drawTrail(drawer, tracer, parameters, Game);
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
}
