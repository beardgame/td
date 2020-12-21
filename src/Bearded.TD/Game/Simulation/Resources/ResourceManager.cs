using System.Collections.Generic;
using Bearded.Utilities.IO;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Resources
{
    sealed class ResourceManager
    {
        private readonly Logger logger;
        private readonly Queue<ResourceTicket> outstandingTickets = new();
        private readonly HashSet<ResourceTicket> committedTickets = new();

        private ResourceAmount reservedResources;
        private ResourceAmount committedResources;
        public ResourceAmount CurrentResources { get; private set; }

        public ResourceAmount ResourcesAfterCommitments => CurrentResources - committedResources;
        public ResourceAmount ResourcesAfterReservations => ResourcesAfterCommitments - reservedResources;

        public ResourceManager(Logger logger)
        {
            this.logger = logger;
            CurrentResources = Constants.Game.WaveGeneration.InitialResources;
        }

        public void ProvideResources(ResourceAmount amount)
        {
            CurrentResources += amount;
        }

        public IResourceTicket RequestResources(ResourceRequest request)
        {
            logger.Trace?.Log($"Received resource request for {request.AmountRequested} resources");
            var ticket = new ResourceTicket(this, request.AmountRequested);
            reservedResources += request.AmountRequested;
            if (outstandingTickets.Count == 0 && request.AmountRequested <= ResourcesAfterCommitments)
            {
                commitResources(ticket);
            }
            else
            {
                outstandingTickets.Enqueue(ticket);
            }
            return ticket;
        }

        public void DistributeResources()
        {
            while (outstandingTickets.TryPeek(out var ticket)
                && ticket.ResourcesLeftToClaim <= ResourcesAfterCommitments)
            {
                commitResources(outstandingTickets.Dequeue());
            }
        }

        private void commitResources(ResourceTicket ticket)
        {
            State.Satisfies(ResourcesAfterCommitments >= ticket.ResourcesLeftToClaim);
            committedTickets.Add(ticket);
            ticket.CommitResources();
            reservedResources -= ticket.ResourcesLeftToClaim;
            committedResources += ticket.ResourcesLeftToClaim;
            logger.Trace?.Log($"Committed {ticket.ResourcesLeftToClaim} resources");
        }

        private void consumeResources(ResourceAmount amount)
        {
            CurrentResources -= amount;
            committedResources -= amount;
        }

        private void onTicketCompleted(ResourceTicket ticket)
        {
            committedTickets.Remove(ticket);
        }

        public sealed record ResourceRequest
        {
            public ResourceAmount AmountRequested { get; }

            public ResourceRequest(ResourceAmount amountRequested)
            {
                AmountRequested = amountRequested;
            }
        }

        public interface IResourceTicket
        {
            bool IsCommitted { get; }
            ResourceAmount ResourcesLeftToClaim { get; }

            void ClaimResources(ResourceAmount amount);
        }

        private sealed class ResourceTicket : IResourceTicket
        {
            private readonly ResourceManager resourceManager;

            public bool IsCommitted { get; private set; }
            public ResourceAmount ResourcesLeftToClaim { get; private set; }

            public ResourceTicket(ResourceManager resourceManager, ResourceAmount requestedAmount)
            {
                this.resourceManager = resourceManager;
                ResourcesLeftToClaim = requestedAmount;
            }

            public void CommitResources()
            {
                State.Satisfies(!IsCommitted);
                IsCommitted = true;
            }

            public void ClaimResources(ResourceAmount amount)
            {
                State.Satisfies(IsCommitted);
                Argument.Satisfies(amount <= ResourcesLeftToClaim);
                resourceManager.consumeResources(amount);
                ResourcesLeftToClaim -= amount;

                if (ResourcesLeftToClaim == ResourceAmount.Zero)
                {
                    resourceManager.onTicketCompleted(this);
                }
            }
        }
    }
}
