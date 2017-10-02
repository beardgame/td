using System;
using System.Collections.Generic;
using System.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    class ResourceManager
    {
        private readonly IList<ResourceRequest> requestedResources = new List<ResourceRequest>();
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

        public void RegisterConsumer(IResourceConsumer consumer, double ratePerS, double maximum)
        {
            requestedResources.Add(new ResourceRequest(consumer, ratePerS, maximum));
            totalResourcesRequested += ratePerS;
        }

        public void DistributeResources(TimeSpan elapsedTime)
        {
            currentResources += elapsedTime.NumericValue * totalResourcesProvided;
            var resourceOut = elapsedTime.NumericValue * totalResourcesRequested;

            if (resourceOut <= currentResources)
            {
                foreach (var request in requestedResources)
                {
                    var consumerGrantedResources = elapsedTime.NumericValue * request.RatePerS;
                    var grant = new ResourceGrant(
                        Math.Min(request.Maximum, consumerGrantedResources),
                        consumerGrantedResources >= request.Maximum);
                    currentResources -= grant.Amount;
                    request.Consumer.ConsumeResources(grant);
                }
                resetForFrame();
                return;
            }

            var sortedRequests = requestedResources.OrderBy((consumer) => (consumer.Maximum / consumer.RatePerS));
            var resourceRatio = currentResources / resourceOut;
            foreach (var request in sortedRequests)
            {
                var requestGrantedResources = resourceRatio * elapsedTime.NumericValue * request.RatePerS;
                if (request.Maximum <= requestGrantedResources)
                {
                    request.Consumer.ConsumeResources(new ResourceGrant(request.Maximum, true));
                    resourceOut -= request.Maximum;
                    currentResources -= request.Maximum;
                    resourceRatio = currentResources / resourceOut;
                }
                else
                {
                    request.Consumer.ConsumeResources(new ResourceGrant(requestGrantedResources, false));
                    currentResources -= requestGrantedResources;
                }
            }
                
            resetForFrame();
        }

        private void resetForFrame()
        {
            currentIncome = totalResourcesProvided - totalResourcesRequested;
            requestedResources.Clear();
            totalResourcesRequested = 0;
            totalResourcesProvided = 0;
        }

        private struct ResourceRequest
        {
            public IResourceConsumer Consumer { get; }
            public double RatePerS { get; }
            public double Maximum { get; }

            public ResourceRequest(IResourceConsumer consumer, double ratePerS, double maximum)
            {
                Consumer = consumer;
                RatePerS = ratePerS;
                Maximum = maximum;
            }
        }
    }
}
