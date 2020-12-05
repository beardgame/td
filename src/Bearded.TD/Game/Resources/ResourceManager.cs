using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Units;
using static Bearded.TD.Constants.Game.Resources;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    sealed class ResourceManager : IListener<EnemyKilled>
    {
        private readonly IList<ResourceRequest> requestedResources = new List<ResourceRequest>();
        private ResourceRate totalResourcesRequested;
        private ResourceRate  totalResourcesProvided;

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
                ProvideOneTimeResource(new ResourceAmount(ResourcesOnKillFactor * @event.Unit.Value));
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
            CurrentResources += totalResourcesProvided * elapsedTime;
            var resourceOut = totalResourcesRequested * elapsedTime;

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
                var grantedResources = request.RatePerS * elapsedTime;
                request.TryGrant(grantedResources, out var consumedResources);
                CurrentResources -= consumedResources;
            }
        }

        private void distributedAtLimitedRates(TimeSpan elapsedTime, ResourceAmount resourceOut)
        {
            var sortedRequests = requestedResources.OrderBy(
                consumer => consumer.Maximum.NumericValue / consumer.RatePerS.NumericValue);
            var resourceRatio = CurrentResources.NumericValue / resourceOut.NumericValue;

            foreach (var request in sortedRequests)
            {
                var grantedResources = resourceRatio * request.RatePerS * elapsedTime;

                var grantFullyConsumed = request.TryGrant(grantedResources, out var consumedResources);
                CurrentResources -= consumedResources;
                resourceOut -= consumedResources;

                if (!grantFullyConsumed)
                {
                    resourceRatio = CurrentResources.NumericValue / resourceOut.NumericValue;
                }
            }
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
