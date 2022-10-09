using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("buoyancy")]
sealed class Buoyancy : Component<Buoyancy.IParameters>
{
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        float Amount { get; }
    }

    public Buoyancy(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var standardBuoyancy = -Constants.Game.Physics.Gravity3;
        var buoyancy = standardBuoyancy * Parameters.Amount;

        physics.ApplyVelocityImpulse(buoyancy * elapsedTime);
    }
}

