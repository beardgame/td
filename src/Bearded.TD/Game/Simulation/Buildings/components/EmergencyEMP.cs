using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("emergencyEMP")]
sealed class EmergencyEMP(EmergencyEMP.IParameters parameters)
    : Component<EmergencyEMP.IParameters>(parameters), IListener<AvailableResourcesChanged<CoreEnergy>>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }

        [Modifiable(0.5)]
        double RemainingCoreLiquidationEffectiveness { get; }
    }

    public bool Available { get; private set; } = true;

    protected override void OnAdded() {}

    public override void Activate()
    {
        Owner.Game.Meta.Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(AvailableResourcesChanged<CoreEnergy> e)
    {
        Available = e.NewAmount > Resource<CoreEnergy>.Zero;
    }

    public void Fire()
    {
        var faction = Owner.FindFaction();
        faction.TryGetBehaviorIncludingAncestors<FactionCoreDeposit>(out var coreDeposit);
        coreDeposit!.LiquidateImmediately(Parameters.RemainingCoreLiquidationEffectiveness);

        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position);
        Owner.Game.Add(obj);
    }
}
