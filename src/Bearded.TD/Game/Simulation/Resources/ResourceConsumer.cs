using System;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Resources;

sealed class ResourceConsumer
{
    private readonly Func<Instant> currentTimeProvider;
    private readonly FactionResources.IResourceReservation reservation;
    private readonly ResourceAmount resourcesRequested;

    public ResourceRate ConsumptionRate { get; private set; }

    private Instant? consumptionStartTime;
    private Instant time => currentTimeProvider();

    public bool CanConsume => reservation.IsCommitted;
    public double PercentageDone => ResourcesClaimed / resourcesRequested;
    public bool IsDone => reservation.ResourcesLeftToClaim == ResourceAmount.Zero;
    public ResourceAmount ResourcesClaimed => resourcesRequested - reservation.ResourcesLeftToClaim;

    public ResourceConsumer(GameState game, FactionResources.IResourceReservation reservation, ResourceRate consumptionRate)
        : this(() => game.Time, reservation, consumptionRate) { }

    public ResourceConsumer(
        Func<Instant> currentTimeProvider, FactionResources.IResourceReservation reservation, ResourceRate consumptionRate)
    {
        this.currentTimeProvider = currentTimeProvider;
        this.reservation = reservation;
        ConsumptionRate = consumptionRate;
        resourcesRequested = reservation.ResourcesLeftToClaim;
    }

    public void UpdateConsumptionRate(ResourceRate newRate)
    {
        ConsumptionRate = newRate;
    }

    public void PrepareIfNeeded()
    {
        if (!reservation.IsReadyToReceive)
        {
            reservation.MarkReadyToReceive();
        }
    }

    public void CompleteIfNeeded()
    {
        PrepareIfNeeded();
        State.Satisfies(reservation.IsCommitted);
        reservation.ClaimResources(reservation.ResourcesLeftToClaim);
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
            ConsumptionRate.InTime(time - consumptionStartTime.Value));
        var actualResourcesConsumed = resourcesRequested - reservation.ResourcesLeftToClaim;
        var resourcesToClaim = expectedResourcesConsumed - actualResourcesConsumed;

        State.Satisfies(resourcesToClaim >= ResourceAmount.Zero);

        reservation.ClaimResources(resourcesToClaim);
    }

    public void Abort()
    {
        reservation.CancelRemainingResources();
    }
}
