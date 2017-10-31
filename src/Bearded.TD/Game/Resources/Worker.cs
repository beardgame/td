using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    class Worker : GameObject
    {
        private readonly WorkerManager manager;
        
        public Position2 Position { get; private set; }
        private Position2 goalPosition;
        private Velocity2 velocity = Velocity2.Zero;

        public Faction Faction { get; }
        public double WorkingSpeed => Constants.Game.Worker.WorkerSpeed;
        public Squared<Unit> WorkRadiusSquared => Constants.Game.Worker.WorkerWorkRadiusSquared;

        private WorkerState currentState;

        public Worker(WorkerManager manager, Faction faction)
        {
            this.manager = manager;
            Faction = faction;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            manager.RegisterWorker(this);
            setState(WorkerState.Idle(manager, this));
        }

        public void AssignTask(WorkerTask task)
        {
            setState(WorkerState.ExecuteTask(manager, this, task));
        }

        private void setState(WorkerState newState)
        {
            if (currentState != null)
            {
                currentState.StateChanged -= setState;
                currentState.GoalPositionChanged -= setGoalPosition;
            }
            currentState = newState;
            currentState.StateChanged += setState;
            currentState.GoalPositionChanged += setGoalPosition;
            currentState.Start();
        }

        private void setGoalPosition(Position2 goalPos)
        {
            goalPosition = goalPos;
        }

        protected override void OnDelete()
        {
            base.OnDelete();

            manager.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState.Update(elapsedTime);
            updatePosition(elapsedTime);
        }

        private void updatePosition(TimeSpan elapsedTime)
        {
            var diff = goalPosition - Position;

            if (diff.LengthSquared > WorkRadiusSquared)
            {
                velocity += diff.Direction * Constants.Game.Worker.Acceleration * elapsedTime;
            }
            else if (velocity.LengthSquared >= Squared<Speed>.FromValue(.1f))
            {
                velocity -= diff.Direction * Constants.Game.Worker.Acceleration * elapsedTime;
            }
            velocity *= Constants.Game.Worker.Friction.Powed((float) elapsedTime.NumericValue);

            Position += velocity * elapsedTime;
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DeepPink;
            geo.DrawCircle(Position.NumericValue, .3f * Constants.Game.World.HexagonDiameter);
        }
    }
}
