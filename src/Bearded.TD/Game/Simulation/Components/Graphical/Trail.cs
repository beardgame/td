using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Graphical
{
    [Component("trail")]
    class Trail<T> : Component<T, ITrailParameters>
        where T : GameObject, IPositionable
    {
        private readonly TrailTracer tracer;

        public Trail(ITrailParameters parameters) : base(parameters)
        {
            tracer = new TrailTracer(parameters.Timeout);
        }

        protected override void Initialize()
        {
            if (Parameters.SurviveObjectDeletion)
            {
                Owner.Deleting += persistTrail;
            }
        }

        private void persistTrail()
        {
            var obj = new PersistentTrail(Parameters, tracer, Owner.Position);
            Owner.Game.Add(obj);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tracer.Update(Owner.Game.Time, Owner.Position);
        }

        public override void Draw(CoreDrawers drawers)
        {
            renderTrail(tracer, Parameters, Owner.Game);
        }


        class PersistentTrail : GameObject
        {
            private readonly ITrailParameters parameters;
            private readonly TrailTracer tracer;
            private readonly Position3 position;
            private Instant deleteAt;

            public PersistentTrail(ITrailParameters parameters, TrailTracer tracer, Position3 position)
            {
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
                renderTrail(tracer, parameters, Game);
            }
        }

        private static void renderTrail(TrailTracer tracer, ITrailParameters parameters, GameState game)
        {
            TrailRenderer.DrawTrail(
                tracer, parameters.Sprite, parameters.Width.NumericValue,
                game.Time, parameters.Timeout,
                parameters.Color
            );
        }
    }
}
