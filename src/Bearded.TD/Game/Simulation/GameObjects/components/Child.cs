using Bearded.TD.Game.Simulation.Modules;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("child")]
sealed class Child : Component<Child.IParameters>, IListener<ObjectDeleting>
{
    private GameObject child = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
    }

    public Child(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        child = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position, Owner.Direction);
        child.AddComponent(new AttachToParent());
        Owner.Game.Add(child);

        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        child.Delete();
    }
}
