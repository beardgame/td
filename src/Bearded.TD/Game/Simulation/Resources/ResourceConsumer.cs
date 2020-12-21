using System;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Resources
{
    sealed class ResourceConsumer
    {
        private readonly Func<Instant> currentTimeProvider;
        private readonly ResourceManager.IResourceTicket ticket;
        private readonly ResourceAmount resourcesRequested;
        private readonly ResourceRate consumptionRate;

        private Instant? consumptionStartTime;
        private Instant time => currentTimeProvider();

        public bool CanConsume => ticket.IsCommitted;
        public double PercentageDone => (resourcesRequested - ticket.ResourcesLeftToClaim) / resourcesRequested;
        public bool IsDone => ticket.ResourcesLeftToClaim == ResourceAmount.Zero;

        public ResourceConsumer(GameState game, ResourceManager.IResourceTicket ticket, ResourceRate consumptionRate)
            : this(() => game.Time, ticket, consumptionRate) { }

        public ResourceConsumer(
            Func<Instant> currentTimeProvider, ResourceManager.IResourceTicket ticket, ResourceRate consumptionRate)
        {
            this.currentTimeProvider = currentTimeProvider;
            this.ticket = ticket;
            this.consumptionRate = consumptionRate;
            resourcesRequested = ticket.ResourcesLeftToClaim;
        }

        public void Update()
        {
            if (consumptionStartTime == null)
            {
                if (ticket.IsCommitted)
                {
                    consumptionStartTime = time;
                }
                return;
            }

            var expectedResourcesConsumed = DiscreteSpaceTime1Math.Min(
                resourcesRequested,
                consumptionRate.InTime(time - consumptionStartTime.Value));
            var actualResourcesConsumed = resourcesRequested - ticket.ResourcesLeftToClaim;
            var resourcesToClaim = expectedResourcesConsumed - actualResourcesConsumed;

            DebugAssert.State.Satisfies(resourcesToClaim >= ResourceAmount.Zero);

            ticket.ClaimResources(resourcesToClaim);
        }
    }
}
