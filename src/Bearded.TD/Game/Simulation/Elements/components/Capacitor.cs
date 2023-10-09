using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("capacitor")]
sealed partial class Capacitor : Component<Capacitor.IParameters>, ICapacitor
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.RechargeRate)]
        ElectricChargeRate RechargeRate { get; }

        [Modifiable(Type = AttributeType.ChargeCapacity)]
        ElectricCharge MaxCharge { get; }
    }

    private CapacitorStatus? status;
    private IBuildingStateProvider? buildingState;

    public ElectricCharge CurrentCharge { get; private set; }
    public ElectricCharge MaxCharge { get; private set; }

    public Capacitor(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, provider => buildingState = provider);
    }

    public override void Activate()
    {
        base.Activate();
        if (Owner.TryGetSingleComponent<IStatusDisplay>(out var statusDisplay))
        {
            status = new CapacitorStatus(Owner.Game, statusDisplay);
        }
        updateMaxCharge(Parameters.MaxCharge);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.MaxCharge != MaxCharge)
        {
            updateMaxCharge(Parameters.MaxCharge);
        }
        updateCharge(elapsedTime);
    }

    private void updateCharge(TimeSpan elapsedTime)
    {
        if (!buildingState?.State.IsFunctional ?? false)
        {
            return;
        }

        CurrentCharge = SpaceTime1MathF.Min(MaxCharge, CurrentCharge + elapsedTime * Parameters.RechargeRate);
        status?.UpdateCharge(CurrentCharge, MaxCharge);
    }

    public override void OnRemoved()
    {
        status?.Detach();
        base.OnRemoved();
    }

    private void updateMaxCharge(ElectricCharge newMax)
    {
        var oldMax = MaxCharge;
        MaxCharge = newMax;
        if (newMax < oldMax)
        {
            CurrentCharge = SpaceTime1MathF.Min(CurrentCharge, MaxCharge);
        }
        if (newMax > oldMax)
        {
            CurrentCharge += newMax - oldMax;
        }
        status?.UpdateCharge(CurrentCharge, MaxCharge);
    }

    public ElectricCharge Discharge()
    {
        var charge = CurrentCharge;
        CurrentCharge = ElectricCharge.Zero;
        return charge;
    }
}

interface ICapacitor
{
    ElectricCharge CurrentCharge { get; }
    ElectricCharge MaxCharge { get; }
    public ElectricCharge Discharge();
}
