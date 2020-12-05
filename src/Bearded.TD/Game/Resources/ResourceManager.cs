using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Units;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Resources;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    sealed class ResourceManager : IListener<EnemyKilled>
    {
        private readonly Random random = new();

        private readonly IList<ResourceRequest> requestedResources = new List<ResourceRequest>();
        private ResourceRate totalResourcesRequested;
        private ResourceRate totalResourcesProvided;

        public ResourceAmount CurrentResources { get; private set; }
        public ResourceRate CurrentIncome { get; private set; }

        public ResourceManager(GlobalGameEvents events)
        {
            CurrentResources = InitialResources;
            events.Subscribe(this);
        }

        public void HandleEvent(EnemyKilled @event)
        {
            if (@event.KillingFaction.Resources == this)
            {
                ProvideOneTimeResource(new ResourceAmount((long) (ResourcesOnKillFactor * @event.Unit.Value)));
            }
        }

        public void ProvideOneTimeResource(ResourceAmount amount)
        {
            CurrentResources += amount;
        }

        public void ProvideResourcesOverTime(ResourceRate ratePerS)
        {
            totalResourcesProvided += ratePerS;
        }

        public void RegisterConsumer(IResourceConsumer consumer, ResourceRate ratePerS, ResourceAmount maximum)
        {
            requestedResources.Add(new ResourceRequest(consumer, ratePerS, maximum));
            totalResourcesRequested += ratePerS;
        }

        public void DistributeResources(TimeSpan elapsedTime)
        {
            CurrentResources += discretizedRate(totalResourcesProvided, elapsedTime);
            var resourceOut = discretizedRate(totalResourcesRequested, elapsedTime);

            if (resourceOut <= CurrentResources)
            {
                distributeAtMaxRates(elapsedTime);
            }
            else
            {
                distributedAtLimitedRates(elapsedTime, resourceOut);
            }

            resetForFrame();
        }

        private void distributeAtMaxRates(TimeSpan elapsedTime)
        {
            foreach (var request in requestedResources)
            {
                var grantedResources = discretizedRate(request.RatePerS, elapsedTime);
                request.TryGrant(grantedResources, out var consumedResources);
                CurrentResources -= consumedResources;
            }
        }

        private void distributedAtLimitedRates(TimeSpan elapsedTime, ResourceAmount resourceOut)
        {
            var sortedRequests = requestedResources.OrderBy(
                consumer => consumer.Maximum.NumericValue / consumer.RatePerS.NumericValue);
            var resourceRatio = (double) CurrentResources.NumericValue / resourceOut.NumericValue;

            foreach (var request in sortedRequests)
            {
                var grantedResources = discretizedRate(request.RatePerS, elapsedTime, resourceRatio);

                var grantFullyConsumed = request.TryGrant(grantedResources, out var consumedResources);
                CurrentResources -= consumedResources;
                resourceOut -= consumedResources;

                if (!grantFullyConsumed)
                {
                    resourceRatio = (double) CurrentResources.NumericValue / resourceOut.NumericValue;
                }
            }
        }

        private ResourceAmount discretizedRate(ResourceRate rate, TimeSpan time) => discretizedRate(rate, time, 1);

        private ResourceAmount discretizedRate(ResourceRate rate, TimeSpan time, double ratio)
        {
            // TODO: get rid of the random.Discretise. It is only there because we're dealing with resources of time
            // still, but that will go away as we make resources completely time-independent.
            return random.Discretise((float) (ratio * rate.NumericValue * time.NumericValue)).Resources();
        }

        private void resetForFrame()
        {
            CurrentIncome = totalResourcesProvided - totalResourcesRequested;
            requestedResources.Clear();
            totalResourcesRequested = ResourceRate.Zero;
            totalResourcesProvided = ResourceRate.Zero;;
        }

        private readonly struct ResourceRequest
        {
            private readonly IResourceConsumer consumer;
            public ResourceRate RatePerS { get; }
            public ResourceAmount Maximum { get; }

            public ResourceRequest(IResourceConsumer consumer, ResourceRate ratePerS, ResourceAmount maximum)
            {
                this.consumer = consumer;
                RatePerS = ratePerS;
                Maximum = maximum;
            }

            public bool TryGrant(ResourceAmount resourcesAvailable, out ResourceAmount grantedResources)
            {
                if (resourcesAvailable >= Maximum)
                {
                    consumer.ConsumeResources(new ResourceGrant(Maximum, true));
                    grantedResources = Maximum;
                    return false;
                }
                else
                {
                    consumer.ConsumeResources(new ResourceGrant(resourcesAvailable, false));
                    grantedResources = resourcesAvailable;
                    return true;
                }
            }
        }
    }
}
