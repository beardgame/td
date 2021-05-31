﻿using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Factions;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Resources
{
    [FactionBehavior("resources")]
    sealed class ResourceManager : FactionBehavior<Faction>
    {
        private readonly HashSet<ResourceReservation> outstandingReservations = new();
        private readonly List<ResourceReservation> reservationQueue = new();
        private readonly HashSet<ResourceReservation> committedReservations = new();

        private ResourceAmount reservedResources;
        private ResourceAmount committedResources;
        public ResourceAmount CurrentResources { get; private set; }

        public ResourceAmount AvailableResources => CurrentResources - committedResources;
        public ResourceAmount ResourcesAfterQueue => AvailableResources - reservedResources;

        protected override void Execute() {}

        public void ProvideResources(ResourceAmount amount)
        {
            CurrentResources += amount;
        }

        public IResourceReservation ReserveResources(ResourceRequest request)
        {
            var reservation = new ResourceReservation(this, request.AmountRequested);
            reservedResources += request.AmountRequested;
            outstandingReservations.Add(reservation);
            return reservation;
        }

        public void DistributeResources()
        {
            while (reservationQueue.Count > 0
                && reservationQueue[0].ResourcesLeftToClaim <= AvailableResources)
            {
                commitResources(reservationQueue[0]);
                reservationQueue.RemoveAt(0);
            }
        }

        private void commitResources(ResourceReservation reservation)
        {
            State.Satisfies(AvailableResources >= reservation.ResourcesLeftToClaim);
            committedReservations.Add(reservation);
            reservation.CommitResources();
            reservedResources -= reservation.ResourcesLeftToClaim;
            committedResources += reservation.ResourcesLeftToClaim;
        }

        private void requestResources(ResourceReservation reservation)
        {
            if (!outstandingReservations.Remove(reservation))
            {
                throw new InvalidOperationException();
            }

            if (reservationQueue.Count == 0 && reservation.ResourcesLeftToClaim <= AvailableResources)
            {
                commitResources(reservation);
            }
            else
            {
                reservationQueue.Add(reservation);
            }
        }

        private void consumeResources(ResourceAmount amount)
        {
            CurrentResources -= amount;
            committedResources -= amount;
        }

        private void onReservationCompleted(ResourceReservation reservation)
        {
            committedReservations.Remove(reservation);
        }

        private void cancelReservation(ResourceReservation reservation)
        {
            if (committedReservations.Remove(reservation))
            {
                committedResources -= reservation.ResourcesLeftToClaim;
                return;
            }

            if (outstandingReservations.Remove(reservation) || reservationQueue.Remove(reservation))
            {
                reservedResources -= reservation.ResourcesLeftToClaim;
                return;
            }

            throw new InvalidOperationException("Reservation wasn't found.");
        }

        public readonly struct ResourceRequest
        {
            public ResourceAmount AmountRequested { get; }

            public ResourceRequest(ResourceAmount amountRequested)
            {
                AmountRequested = amountRequested;
            }
        }

        public interface IResourceReservation
        {
            bool IsReadyToReceive { get; }
            bool IsCommitted { get; }
            ResourceAmount ResourcesLeftToClaim { get; }

            void MarkReadyToReceive();
            void ClaimResources(ResourceAmount amount);
            void CancelRemainingResources();
        }

        private sealed class ResourceReservation : IResourceReservation
        {
            private readonly ResourceManager resourceManager;

            private bool isCancelled;
            public bool IsReadyToReceive { get; private set; }
            public bool IsCommitted { get; private set; }
            public ResourceAmount ResourcesLeftToClaim { get; private set; }

            public ResourceReservation(ResourceManager resourceManager, ResourceAmount requestedAmount)
            {
                this.resourceManager = resourceManager;
                ResourcesLeftToClaim = requestedAmount;
            }

            public void CommitResources()
            {
                State.Satisfies(IsReadyToReceive);
                State.Satisfies(!IsCommitted);
                State.Satisfies(!isCancelled);
                IsCommitted = true;
            }

            public void MarkReadyToReceive()
            {
                State.Satisfies(!IsReadyToReceive);
                State.Satisfies(!isCancelled);
                IsReadyToReceive = true;
                resourceManager.requestResources(this);
            }

            public void ClaimResources(ResourceAmount amount)
            {
                State.Satisfies(IsReadyToReceive);
                State.Satisfies(IsCommitted);
                State.Satisfies(!isCancelled);
                Argument.Satisfies(amount <= ResourcesLeftToClaim);
                resourceManager.consumeResources(amount);
                ResourcesLeftToClaim -= amount;

                if (ResourcesLeftToClaim == ResourceAmount.Zero)
                {
                    resourceManager.onReservationCompleted(this);
                }
            }

            public void CancelRemainingResources()
            {
                if (ResourcesLeftToClaim == ResourceAmount.Zero)
                {
                    return;
                }
                resourceManager.cancelReservation(this);
                isCancelled = true;
            }
        }
    }
}
