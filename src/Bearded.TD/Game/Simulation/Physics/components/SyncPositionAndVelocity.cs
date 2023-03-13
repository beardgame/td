using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class SyncPositionAndVelocity : Component, ISyncable
{
    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public IStateToSync GetCurrentStateToSync()
    {
        return State.From(Owner);
    }

    private sealed class State : IStateToSync
    {
        private readonly GameObject obj;
        private Position3 position;
        private Velocity3 velocity;

        public static IStateToSync From(GameObject owner)
        {
            return new State(owner,
                owner.Position,
                owner.GetComponents<IPhysics>().FirstOrDefault()?.Velocity ?? Velocity3.Zero);
        }

        private State(GameObject obj, Position3 position, Velocity3 velocity)
        {
            this.obj = obj;
            this.position = position;
            this.velocity = velocity;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref position);
            stream.Serialize(ref velocity);
        }

        public void Apply()
        {
            obj.Position = position;
            if (obj.GetComponents<IPhysics>().FirstOrDefault() is { } physics)
            {
                physics.ApplyVelocityImpulse(velocity - physics.Velocity);
            }
        }
    }
}

