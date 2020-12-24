using System;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Resources
{
    sealed class ResourceConsumer
    {
        private readonly Func<Instant> currentTimeProvider;
        private readonly ResourceManager.IResourceReservation reservation;
        private readonly ResourceAmount resourcesRequested;
        private readonly ResourceRate consumptionRate;

        private Instant? consumptionStartTime;
        private Instant time => currentTimeProvider();

        public bool CanConsume => reservation.IsCommitted;
        public double PercentageDone => (resourcesRequested - reservation.ResourcesLeftToClaim) / resourcesRequested;
        public bool IsDone => reservation.ResourcesLeftToClaim == ResourceAmount.Zero;

        public ResourceConsumer(GameState game, ResourceManager.IResourceReservation reservation, ResourceRate consumptionRate)
            : this(() => game.Time, reservation, consumptionRate) { }

        public ResourceConsumer(
            Func<Instant> currentTimeProvider, ResourceManager.IResourceReservation reservation, ResourceRate consumptionRate)
        {
            this.currentTimeProvider = currentTimeProvider;
            this.reservation = reservation;
            this.consumptionRate = consumptionRate;
            resourcesRequested = reservation.ResourcesLeftToClaim;
        }

        public void PrepareIfNeeded()
        {
            if (!reservation.IsReadyToReceive)
            {
                reservation.MarkReadyToReceive();
            }
        }

        public void Update()
        {
            if (consumptionStartTime == null)
            {
                if (reservation.IsCommitted)
                {
                    consumptionStartTime = time;
                }
                return;
            }

            var expectedResourcesConsumed = DiscreteSpaceTime1Math.Min(
                resourcesRequested,
                consumptionRate.InTime(time - consumptionStartTime.Value));
            var actualResourcesConsumed = resourcesRequested - reservation.ResourcesLeftToClaim;
            var resourcesToClaim = expectedResourcesConsumed - actualResourcesConsumed;

            DebugAssert.State.Satisfies(resourcesToClaim >= ResourceAmount.Zero);

            reservation.ClaimResources(resourcesToClaim);
        }

        public void Abort()
        {
            reservation.CancelRemainingResources();
        }
    }
}
