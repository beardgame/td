using System;
using System.Diagnostics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game;

sealed partial class ProtoLegs
{
    private sealed class Leg
    {
        public Direction2 NeutralDirection { get; set; }
        public Position2 Position { get; set; }
        public Velocity2 Velocity { get; set; }
        public Position2 TargetPosition { get; set; }

        public bool OnGround { get; set; } = true;

        private Difference2 lastError;
        private Difference2 errorIntegral;

        public void Update(TimeSpan elapsedTime, ProtoLegs body)
        {
            if (OnGround)
            {
                updateOnGround(body);
            }
            else
            {
                updateStepping(body, elapsedTime);
            }
        }

        public void StartStep(ProtoLegs body)
        {
            OnGround = false;
        }

        private void updateOnGround(ProtoLegs body)
        {
        }

        private void updateStepping(ProtoLegs body, TimeSpan elapsedTime)
        {
            var optimalLocation = OptimalLocationFrom(body, NeutralDirection);

            var velocityVector = body.velocity.NumericValue;
            var clampedBodyVelocity = velocityVector.LengthSquared < 1
                ? velocityVector
                : velocityVector.Normalized();
            
            TargetPosition = optimalLocation + clampedBodyVelocity * maxStepDistance;

            var difference = TargetPosition - Position;
            var distanceSquared = difference.LengthSquared;

            {
                // pid
                var pidStrength = body.velocity.Length.NumericValue;

                pidStrength = Math.Max(pidStrength, 5);
                
                var error = difference;
                var derivative = (error - lastError) / (float)elapsedTime.NumericValue;
                var a = (error * 2 + derivative * 2 + errorIntegral * 2)  / 1.S() / 1.S() * pidStrength;

                lastError = error;
                errorIntegral += error;
                
                // integral drag
                var dragForce = errorIntegral.LengthSquared.NumericValue * 1f;
                var direction = errorIntegral.NumericValue.NormalizedSafe();
                var drag = new Velocity2(direction * -dragForce);
                errorIntegral += drag * elapsedTime;
                
                if (float.IsNaN(errorIntegral.X.NumericValue) || float.IsInfinity(errorIntegral.X.NumericValue)
                    || Math.Abs(Position.X.NumericValue) > 1e+4 )
                {
                }
                
                Velocity += a * elapsedTime;
            }

            {
                // drag
                var dragForce = Velocity.LengthSquared.NumericValue * 1f;
                var direction = Velocity.NumericValue.NormalizedSafe();
                var drag = new Acceleration2(direction * -dragForce);
                //Velocity += drag * elapsedTime;
            }
            
            var step = Velocity * elapsedTime;

            if (distanceSquared < 0.5.U().Squared)// || Vector2.Dot(Velocity.NumericValue, difference.NumericValue) <= 0)
            {
                EndStep();
                return;
            }


            Position += step;
            
            
        }


        public static Position2 OptimalLocationFrom(ProtoLegs body, Direction2 neutralDirection)
            => body.position + neutralDirection * neutralFootDistance;

        public void EndStep()
        {
            TargetPosition = Position;
            Velocity = Velocity2.Zero;
            lastError = Difference2.Zero;
            errorIntegral = Difference2.Zero;
            OnGround = true;
        }
    }
}
