using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using static Bearded.TD.Game.Simulation.Enemies.SpawnEnemiesOnKilled;

namespace Bearded.TD.Game.Simulation.Enemies;

[Component("spawnEnemiesOnKilled")]
sealed class SpawnEnemiesOnKilled(IParameters parameters) : Component<IParameters>(parameters), IListener<ObjectKilled>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        int Count { get; }

        EnemyFormRecipe Form { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(ObjectKilled @event)
    {
        Owner.Sync(() =>
        {
            var meta = Owner.Game.Meta;
            var form = Parameters.Form.ToForm(Owner.Game.Meta.Blueprints);
            var ids = meta.Ids.GetBatch<GameObject>(Parameters.Count);
            return SpawnEnemies.Command(Owner.Game, form, ids, Owner.Position);
        });
    }
}
