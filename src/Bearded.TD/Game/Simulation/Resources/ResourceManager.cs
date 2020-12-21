using System;
using System.Collections.Generic;
using Bearded.Utilities.IO;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Resources
{
    sealed class ResourceManager
    {
        private readonly Logger logger;
        private readonly List<ResourceTicket> outstandingTickets = new();
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
                outstandingTickets.Add(ticket);
            }
            return ticket;
        }

        public void DistributeResources()
        {
            while (outstandingTickets.Count > 0
                && outstandingTickets[0].ResourcesLeftToClaim <= ResourcesAfterCommitments)
            {
                commitResources(outstandingTickets[0]);
                outstandingTickets.RemoveAt(0);
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

        private void cancelTicket(ResourceTicket ticket)
        {
            if (committedTickets.Remove(ticket))
            {
                committedResources -= ticket.ResourcesLeftToClaim;
                return;
            }

            if (outstandingTickets.Remove(ticket))
            {
                reservedResources -= ticket.ResourcesLeftToClaim;
                return;
            }

            throw new InvalidOperationException("Ticket must be either committed or reserved.");
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
            void CancelRemainingResources();
        }

        private sealed class ResourceTicket : IResourceTicket
        {
            private readonly ResourceManager resourceManager;

            private bool isCancelled;
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
                State.Satisfies(!isCancelled);
                IsCommitted = true;
            }

            public void ClaimResources(ResourceAmount amount)
            {
                State.Satisfies(IsCommitted);
                State.Satisfies(!isCancelled);
                Argument.Satisfies(amount <= ResourcesLeftToClaim);
                resourceManager.consumeResources(amount);
                ResourcesLeftToClaim -= amount;

                if (ResourcesLeftToClaim == ResourceAmount.Zero)
                {
                    resourceManager.onTicketCompleted(this);
                }
            }

            public void CancelRemainingResources()
            {
                if (ResourcesLeftToClaim == ResourceAmount.Zero)
                {
                    return;
                }
                resourceManager.cancelTicket(this);
                isCancelled = true;
            }
        }
    }
}
