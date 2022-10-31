using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("spawnObjectOnLevelGain")]
sealed class SpawnObjectOnLevelGain
    : Component<SpawnObjectOnLevelGain.IParameters>, IListener<GainLevel>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }
    }

    public SpawnObjectOnLevelGain(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(GainLevel @event)
    {
        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position, Direction2.Zero);
        Owner.Game.Add(obj);
        Owner.RemoveComponent(this);
    }
}
