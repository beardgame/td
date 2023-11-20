using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IFuelTank
{
    float FilledPercentage { get; }
    void Consume();
    void Refill();

    bool IsEmpty => FilledPercentage <= 0;
}

[Component("fuelTank")]
sealed class FuelTank : Component<FuelTank.IParameters>, IFuelTank
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.FuelCapacity)]
        public int FuelCapacity { get; }
    }

    private int fuelUsed;

    public float FilledPercentage => 1 - fuelUsed / (float)Parameters.FuelCapacity;

    public FuelTank(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Update(TimeSpan elapsedTime) { }

    public void Consume()
    {
        fuelUsed++;
    }

    public void Refill()
    {
        fuelUsed = 0;
    }
}
