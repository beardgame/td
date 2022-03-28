using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IRecoilParameters : IParametersTemplate<IRecoilParameters>
{
    AngularVelocity Impulse { get; }
}

[Component("recoil")]
sealed class Recoil : Component<IRecoilParameters>, IListener<ShotProjectile>
{
    private IAngularAccelerator? accelerator;

    public Recoil(IRecoilParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IAngularAccelerator>(Owner, Events, a => accelerator = a);

        Events.Subscribe(this);
    }

    public void HandleEvent(ShotProjectile e)
    {
        accelerator?.Impact(
            Parameters.Impulse * StaticRandom.Float(0.5f, 1) * StaticRandom.Sign()
        );
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
