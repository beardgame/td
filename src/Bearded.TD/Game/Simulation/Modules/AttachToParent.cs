using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("attachToParent")]
sealed class AttachToParent : Component
{
    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        if (Owner.Parent is not { } parent)
            return;

        var moving = parent.GetComponents<IMoving>().SingleOrDefault();

        if (moving != null)
            Owner.AddComponent(new AttachedVelocity(moving));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.Parent is not { } parent)
            return;

        Owner.Position = parent.Position;
        Owner.Direction = parent.Direction;
    }

    private sealed class AttachedVelocity : Component, IMoving
    {
        private readonly IMoving moving;
        public Velocity3 Velocity => moving.Velocity;

        public AttachedVelocity(IMoving moving)
        {
            this.moving = moving;
        }

        protected override void OnAdded() { }
        public override void Update(TimeSpan elapsedTime) { }
    }
}

