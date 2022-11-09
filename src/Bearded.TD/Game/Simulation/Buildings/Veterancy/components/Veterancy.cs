using System.Collections.Immutable;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

interface ILevelable
{
    // Expected to be called from synchronized code.
    void LevelUp();
}

[Component("veterancy")]
sealed class Veterancy : Component, IListener<GainXp>, ISyncable, ILevelable
{
    private readonly ImmutableArray<Experience> levelThresholds;

    private int level;
    private Experience experience;

    public Veterancy() : this(Constants.Game.Building.VeterancyThresholds) {}

    private Veterancy(ImmutableArray<Experience> levelThresholds)
    {
        this.levelThresholds = levelThresholds;
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        ReportAggregator.Register(Events, new VeterancyReport(this));
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(GainXp @event)
    {
        AddXp(@event.Amount);
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

    public static Veterancy WithLevelThresholds(params Experience[] levelThresholds) =>
        new(levelThresholds.ToImmutableArray());

    private sealed class VeterancyReport : IVeterancyReport
    {
        public ReportType Type => ReportType.EntityProgression;

        public int CurrentVeterancyLevel => subject.level;

        public Experience CurrentExperience => subject.experience;

        public Experience? NextLevelThreshold => subject.level < subject.levelThresholds.Length
            ? subject.levelThresholds[subject.level]
            : null;

        private Experience previousLevelThreshold => subject.level > 0
            ? subject.levelThresholds[subject.level - 1]
            : Experience.Zero;

        public double PercentageToNextLevel => NextLevelThreshold.HasValue
            ? (CurrentExperience - previousLevelThreshold) / (NextLevelThreshold.Value - previousLevelThreshold)
            : 1;

        private readonly Veterancy subject;

        public VeterancyReport(Veterancy subject)
        {
            this.subject = subject;
        }
    }

    public IStateToSync GetCurrentStateToSync() => new VeterancySynchronizedState(this);

    private sealed class VeterancySynchronizedState : IStateToSync
    {
        private readonly Veterancy source;
        private int currentExperience;

        public VeterancySynchronizedState(Veterancy source)
        {
            this.source = source;
            currentExperience = source.experience.NumericValue;
        }

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

interface IVeterancyReport : IReport
{
    public int CurrentVeterancyLevel { get; }
    public Experience CurrentExperience { get; }
    public Experience? NextLevelThreshold { get; }
    public double PercentageToNextLevel { get; }
}
