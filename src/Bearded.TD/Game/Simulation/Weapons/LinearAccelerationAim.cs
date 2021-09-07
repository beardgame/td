using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons
{
    [Component("linearAccelerationAim")]
    class LinearAccelerationAim : Component<Weapon, ILinearAccelerationAimParameters>
    {
        private AngularVelocity angularVelocity;

        public LinearAccelerationAim(ILinearAccelerationAimParameters parameters) : base(parameters) { }

        protected override void Initialize()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            accelerateTowardsGoal(elapsedTime);
            applyVelocity(elapsedTime);
            dampVelocity(elapsedTime);
        }

        private void accelerateTowardsGoal(TimeSpan elapsedTime)
        {
            Owner.AimDirection.Match(
                aimDirection =>
                {
                    var angularDirection = Angle.Between(Owner.CurrentDirection, aimDirection).Sign();
                    var acceleration = Parameters.Acceleration * angularDirection;

                    angularVelocity += acceleration * elapsedTime;
                }
            );
        }

        private void applyVelocity(TimeSpan elapsedTime)
        {
            Owner.Turn(angularVelocity * elapsedTime);
        }

        private void dampVelocity(TimeSpan elapsedTime)
        {
            var damping = 1 - 1 / (Parameters.DragInverse + 1);
            var velocityMultiplier = (float)Math.Pow(damping, elapsedTime.NumericValue);

            angularVelocity *= velocityMultiplier;
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
