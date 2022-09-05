using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("markEnemiesInRange")]
sealed class MarkEnemiesInRange
    : Component<MarkEnemiesInRange.IParameters>,
        IListener<FireWeapon>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        int MaxEnemyCount { get; }

        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }
    }

    private PassabilityLayer passabilityLayer = null!;
    private UnitLayer unitLayer = null!;

    public MarkEnemiesInRange(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        base.Activate();

        passabilityLayer = Owner.Game.PassabilityManager.GetLayer(Passability.Projectile);
        unitLayer = Owner.Game.UnitLayer;
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(FireWeapon @event)
    {
        var visibilityChecker = new LevelVisibilityChecker();
        // TODO: benchmark and see if this should be cached
        var tilesInRange = visibilityChecker.EnumerateVisibleTiles(
                Owner.Game.Level,
                Owner.Position.XY(),
                Parameters.Range,
                t => !Owner.Game.Level.IsValid(t) && passabilityLayer[t].IsPassable)
            .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2);
        var enemiesInRange = tilesInRange
            .Select(t => t.tile)
            .SelectMany(t => unitLayer.GetUnitsOnTile(t))
            .Where(u => (u.Position - Owner.Position).LengthSquared <= Parameters.Range.Squared);
        var selectedEnemies = enemiesInRange.RandomSubset(Parameters.MaxEnemyCount).ToImmutableArray();
        if (selectedEnemies.IsEmpty)
        {
            return;
        }

        var damagePerEnemy = @event.Damage / selectedEnemies.Length;
        foreach (var enemy in selectedEnemies)
        {
            Events.Send(new EnemyMarked(enemy, damagePerEnemy));
        }
    }
}
