using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
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
}
