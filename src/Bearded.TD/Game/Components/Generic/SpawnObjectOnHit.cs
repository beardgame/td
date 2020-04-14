using Bearded.TD.Content.Models;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("spawnObjectOnHit")]
    class SpawnObjectOnHit<T> : Component<T, ISpawnObjectOnHitParameters>, IListener<HitLevel>, IListener<HitEnemy>
        where T : GameObject, IPositionable, IComponentOwner
    {
        public SpawnObjectOnHit(ISpawnObjectOnHitParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            if(Parameters.OnHitEnemy)
                Events.Subscribe<HitEnemy>(this);

            if(Parameters.OnHitLevel)
                Events.Subscribe<HitLevel>(this);
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
            var obj = new ComponentGameObject(Parameters.Object, Owner, Owner.Position, Direction2.Zero);
            Owner.Game.Add(obj);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}
