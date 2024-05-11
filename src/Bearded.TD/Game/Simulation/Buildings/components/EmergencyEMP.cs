using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("emergencyEMP")]
sealed class EmergencyEMP : Component<EmergencyEMP.IParameters>, IListener<WaveStarted>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
    }

    public bool Available { get; private set; } = true;

    public EmergencyEMP(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        Owner.Game.Meta.Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(WaveStarted e)
    {
        Available = true;
    }

    public void Fire()
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position);
        Owner.Game.Add(obj);

        Available = false;
    }
}
