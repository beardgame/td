using System;
using System.Collections.Generic;
using System.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    class ResourceManager
    {
        private readonly IList<IResourceConsumer> resourceConsumers = new List<IResourceConsumer>();
        private double totalResourcesRequested;
        private double totalResourcesProvided;

        private double currentResources;
        private double currentIncome;

        public long CurrentResources => (long) currentResources;
        public int CurrentIncome => (int) currentIncome;

        public void ProvideOneTimeResource(double amount)
        {
            currentResources += amount;
        }

        public void ProvideResourcesOverTime(double ratePerS)
        {
            totalResourcesProvided += ratePerS;
        }

        public void RegisterConsumer(IResourceConsumer consumer)
        {
            resourceConsumers.Add(consumer);
            totalResourcesRequested += consumer.RatePerS;
        }

        public void DistributeResources(TimeSpan elapsedTime)
        {
            currentResources += elapsedTime.NumericValue * totalResourcesProvided;
            var resourceOut = elapsedTime.NumericValue * totalResourcesRequested;

            if (resourceOut <= currentResources)
            {
                foreach (var consumer in resourceConsumers)
                {
                    var consumerGrantedResources = elapsedTime.NumericValue * consumer.RatePerS;
                    var grant = new ResourceGrant(
                        Math.Min(consumer.Maximum, consumerGrantedResources),
                        consumerGrantedResources >= consumer.Maximum);
                    currentResources -= grant.Amount;
                    consumer.ConsumeResources(grant);
                }
                resetForFrame();
                return;
            }

            var sortedConsumers = resourceConsumers.OrderBy((consumer) => (consumer.Maximum / consumer.RatePerS));
            var resourceRatio = currentResources / resourceOut;
            foreach (var consumer in sortedConsumers)
            {
                var consumerGrantedResources = resourceRatio * elapsedTime.NumericValue * consumer.RatePerS;
                if (consumer.Maximum <= consumerGrantedResources)
                {
                    consumer.ConsumeResources(new ResourceGrant(consumer.Maximum, true));
                    resourceOut -= consumer.Maximum;
                    currentResources -= consumer.Maximum;
                    resourceRatio = currentResources / resourceOut;
                }
                else
                {
                    consumer.ConsumeResources(new ResourceGrant(consumerGrantedResources, false));
                    currentResources -= consumerGrantedResources;
                }
            }
                
            resetForFrame();
        }

        private void resetForFrame()
        {
            currentIncome = totalResourcesProvided - totalResourcesRequested;
            resourceConsumers.Clear();
            totalResourcesRequested = 0;
            totalResourcesProvided = 0;
        }
    }
}
