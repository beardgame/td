using System;
using System.Collections.Immutable;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

interface ILevelable
{
    // Expected to be called from synchronized code.
    void LevelUp();
}

readonly record struct VeterancyStatus(
    int Level,
    double PercentageToNextLevel)
{
    public static readonly VeterancyStatus Initial = new(0, 0);

    public static VeterancyStatus From(
        int level, Experience experience, ImmutableArray<Experience> levelThresholds)
    {
        var previousThreshold = level <= 0
            ? Experience.Zero
            : levelThresholds[level - 1];

        if (level >= levelThresholds.Length)
        {
            return new VeterancyStatus(level, 1);
        }

        var nextThreshold = levelThresholds[level];
        var percentageToNextLevel =
            Math.Clamp((experience - previousThreshold) / (nextThreshold - previousThreshold), 0, 1);
        return new VeterancyStatus(level, percentageToNextLevel);
    }
}

interface IVeterancy
{
    VeterancyStatus GetVeterancyStatus();
    event GenericEventHandler<VeterancyStatus>? VeterancyStatusChanged;
}

[Component("veterancy")]
sealed class Veterancy : Component, IListener<GainXp>, IListener<WaveEnded>, ISyncable, ILevelable, IVeterancy
{
    private readonly ImmutableArray<Experience> levelThresholds;

    private int level;
    private Experience experience;
    private Experience previousExperience;
    private bool activated;

    public Veterancy() : this(Constants.Game.Building.VeterancyThresholds) {}

    private Veterancy(ImmutableArray<Experience> levelThresholds)
    {
        this.levelThresholds = levelThresholds;
    }

    protected override void OnAdded()
    {
        Events.Subscribe<GainXp>(this);
    }

    public override void Activate()
    {
        base.Activate();
        Owner.Game.Meta.Events.Subscribe<WaveEnded>(this);
        activated = true;
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe<GainXp>(this);
        if (activated)
        {
            Owner.Game.Meta.Events.Unsubscribe<WaveEnded>(this);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        notifyExperienceChangedIfNeeded();
    }

    private void notifyExperienceChangedIfNeeded()
    {
        if (experience == previousExperience)
            return;

        VeterancyStatusChanged?.Invoke(GetVeterancyStatus());
        previousExperience = experience;
    }

    public VeterancyStatus GetVeterancyStatus() => VeterancyStatus.From(level, experience, levelThresholds);

    public event GenericEventHandler<VeterancyStatus>? VeterancyStatusChanged;

    public void HandleEvent(GainXp @event)
    {
        AddXp(@event.Amount);
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (GetVeterancyStatus().PercentageToNextLevel >= 0.95)
        {
            Owner.Sync(LevelUpGameObject.Command);
        }
    }

    public void AddXp(Experience amount)
    {
        experience += amount;
        Owner.Sync(checkForLevel, this);
    }

    private static void checkForLevel(Veterancy self, ICommandDispatcher<GameInstance> dispatcher)
    {
        while (self.level < self.levelThresholds.Length && self.experience >= self.levelThresholds[self.level])
        {
            dispatcher.Dispatch(LevelUpGameObject.Command(self.Owner));
        }
    }

    public void ForceLevelUp()
    {
        if (level >= levelThresholds.Length)
        {
            return;
        }

        experience = levelThresholds[level];
        Owner.Sync(LevelUpGameObject.Command);
    }

    // Expected to be called from synchronized code.
    void ILevelable.LevelUp()
    {
        level++;
        Events.Send(new GainLevel());
        Owner.Game.Meta.Events.Send(new BuildingGainedLevel(Owner));
    }

    public static Veterancy WithLevelThresholds(params Experience[] levelThresholds) => new([..levelThresholds]);

    public IStateToSync GetCurrentStateToSync() => new VeterancySynchronizedState(this);

    private sealed class VeterancySynchronizedState(Veterancy source) : IStateToSync
    {
        private float currentExperience = source.experience.NumericValue;

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref currentExperience);
        }

        public void Apply()
        {
            source.experience = new Experience(currentExperience);
        }
    }
}
