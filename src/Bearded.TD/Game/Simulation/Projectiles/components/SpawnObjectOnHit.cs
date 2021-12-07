using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [Component("spawnObjectOnHit")]
    sealed class SpawnObjectOnHit<T>
        : Component<T, ISpawnObjectOnHitParameters>, IListener<HitLevel>, IListener<HitEnemy>
        where T : GameObject, IPositionable, IComponentOwner
    {
        public SpawnObjectOnHit(ISpawnObjectOnHitParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            if (Parameters.OnHitEnemy)
            {
                Events.Subscribe<HitEnemy>(this);
            }

            if (Parameters.OnHitLevel)
            {
                Events.Subscribe<HitLevel>(this);
            }
        }

        public override void OnRemoved()
        {
            if (Parameters.OnHitEnemy)
            {
                Events.Unsubscribe<HitEnemy>(this);
            }

            if (Parameters.OnHitLevel)
            {
                Events.Unsubscribe<HitLevel>(this);
            }
        }

        public void HandleEvent(HitLevel @event)
        {
            onHit();
        }

        public void HandleEvent(HitEnemy @event)
        {
            onHit();
        }

        private void onHit()
        {
            ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Owner.Game, Parameters.Object, Owner, Owner.Position, Direction2.Zero);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }
    }
}
