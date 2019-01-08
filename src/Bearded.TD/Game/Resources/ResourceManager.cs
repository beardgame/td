﻿using System;
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
            currentResources += totalResourcesProvided * elapsedTime.NumericValue;
            var resourceOut = totalResourcesRequested * elapsedTime.NumericValue;

            if (resourceOut <= currentResources)
            {
                distributeAtFullMaxRates(elapsedTime);
            }
            else
            {
                distributedAtLimitedRates(elapsedTime, resourceOut);
            }
            
            resetForFrame();
        }

        private void distributeAtFullMaxRates(TimeSpan elapsedTime)
        {
            foreach (var request in requestedResources)
            {
                var grantedResources = elapsedTime.NumericValue * request.RatePerS;
                request.TryGrant(grantedResources, out var consumedResources);
                currentResources -= consumedResources;
            }
        }

        private void distributedAtLimitedRates(TimeSpan elapsedTime, double resourceOut)
        {
            var sortedRequests = requestedResources.OrderBy(consumer => consumer.Maximum / consumer.RatePerS);
            var resourceRatio = currentResources / resourceOut;
            
            foreach (var request in sortedRequests)
            {
                var grantedResources = request.RatePerS * resourceRatio * elapsedTime.NumericValue;

                var grantFullyConsumed = request.TryGrant(grantedResources, out var consumedResources);
                currentResources -= consumedResources;
                
                if (!grantFullyConsumed)
                {
                    resourceOut -= consumedResources;
                    resourceRatio = currentResources / resourceOut;
                }
            }
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
            
            public bool TryGrant(double resourcesAvailable, out double grantedResources)
            {
                if (resourcesAvailable >= Maximum)
                {
                    Consumer.ConsumeResources(new ResourceGrant(Maximum, true));
                    grantedResources = Maximum;
                    return false;
                }
                else
                {
                    Consumer.ConsumeResources(new ResourceGrant(resourcesAvailable, false));
                    grantedResources = resourcesAvailable;
                    return true;
                }
            }
        }
    }
}
