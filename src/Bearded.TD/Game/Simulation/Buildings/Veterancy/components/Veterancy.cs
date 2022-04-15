using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

[Component("veterancy")]
sealed class Veterancy : Component, IListener<GainXp>
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
        addXp(@event.Amount);
    }

    private void addXp(Experience amount)
    {
        experience += amount;
        checkForLevel();
    }

    private void checkForLevel()
    {
        while (level < levelThresholds.Length && experience >= levelThresholds[level])
        {
            levelUp();
        }
    }

    private void levelUp()
    {
        level++;
        Events.Send(new GainLevel());
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
}

interface IVeterancyReport : IReport
{
    public int CurrentVeterancyLevel { get; }
    public Experience CurrentExperience { get; }
    public Experience? NextLevelThreshold { get; }
    public double PercentageToNextLevel { get; }
}
