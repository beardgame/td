using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("inheritFireWeaponEvent")]
sealed class InheritFireWeaponEvent : Component
{
    private GameObject? parent;
    private Listener? listener;

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        parent = Owner.Parent;

        if (parent == null)
            return;

        listener = new Listener(this);
        parent.AddComponent(listener);

        Events.Subscribe(new EventListener<ObjectDeleting>(_ => OnRemovedInternal()));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (parent == null)
            return;

        Owner.Direction = parent.Direction;
    }

    protected override void OnRemovedInternal()
    {
        if (parent != null && listener != null)
        {
            parent.RemoveComponent(listener);
            listener = null;
        }
    }

    private sealed class Listener(InheritFireWeaponEvent owner) : Component, IListener<FireWeapon>
    {
        protected override void OnAdded() => Events.Subscribe(this);
        protected override void OnRemovedInternal() => Events.Unsubscribe(this);

        public override void Update(TimeSpan elapsedTime) { }

        public void HandleEvent(FireWeapon e) => owner.Events.Send(e);
    }
}
