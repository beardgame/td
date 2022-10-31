using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

sealed class IncompleteRepair
    : Component,
        IIncompleteRepair,
        IRepairSyncer,
        ProgressTracker.IProgressSubject
{
    private readonly Faction repairingFaction;
    private readonly ProgressTracker progressTracker;
    private IHealth? health;
    private IHealthEventReceiver? healthEventReceiver;

    public double PercentageComplete { get; private set; }
    private HitPoints hitPointsHealed = HitPoints.Zero;
    private HitPoints? hitPointsToHeal;

    public IncompleteRepair(Faction repairingFaction)
    {
        this.repairingFaction = repairingFaction;
        progressTracker = new ProgressTracker(this);
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);
        ComponentDependencies.Depend<IHealthEventReceiver>(
            Owner, Events, receiver => healthEventReceiver = receiver);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void SendSyncStart()
    {
        Owner.Sync(SyncBuildingRepairStart.Command);
    }

    public void SendSyncComplete()
    {
        Owner.Sync(SyncBuildingRepairCompletion.Command);
    }

    public void OnStart()
    {
        if (health != null && healthEventReceiver != null)
        {
            hitPointsToHeal = health.MaxHealth - health.CurrentHealth;
        }
    }

    public void OnProgressSet(double percentage)
    {
        PercentageComplete = percentage;
        updateHealing(percentage);
    }

    private void updateHealing(double percentage)
    {
        if (!hitPointsToHeal.HasValue)
        {
            return;
        }

        var expectedHitPointsHealed =
            new HitPoints(MoreMath.CeilToInt(percentage * hitPointsToHeal.Value.NumericValue));
        addHitPoints(expectedHitPointsHealed - hitPointsHealed);
        hitPointsHealed = expectedHitPointsHealed;
    }

    public void OnComplete()
    {
        if (hitPointsToHeal.HasValue)
        {
            addHitPoints(hitPointsToHeal.Value - hitPointsHealed);
        }

        Events.Send(new RepairFinished(repairingFaction));
        Owner.Game.Meta.Events.Send(new BuildingRepairFinished(Owner));
    }

    private void addHitPoints(HitPoints hitPoints)
    {
        if (hitPoints != HitPoints.Zero)
        {
            healthEventReceiver!.Heal(new HealInfo(hitPoints));
        }
    }

    public void OnCancel()
    {
        Events.Send(new RepairCancelled());
    }

    public bool IsStarted => progressTracker.IsStarted;
    public bool IsCompleted => progressTracker.IsCompleted;
    public bool IsCancelled => progressTracker.IsCancelled;
    public void StartRepair() => progressTracker.Start();
    public void SetRepairProgress(double percentage) => progressTracker.SetProgress(percentage);
    public void CompleteRepair() => progressTracker.Complete();
    public void CancelRepair() => progressTracker.Cancel();

    public void SyncStartRepair() => progressTracker.SyncStart();
    public void SyncCompleteRepair() => progressTracker.SyncComplete();
}

interface IIncompleteRepair
{
    bool IsStarted { get; }
    bool IsCompleted { get; }
    bool IsCancelled { get; }
    double PercentageComplete { get; }

    public void StartRepair();
    public void SetRepairProgress(double percentage);
    public void CompleteRepair();
    public void CancelRepair();
}

interface IRepairSyncer
{
    void SyncStartRepair();
    void SyncCompleteRepair();
}
