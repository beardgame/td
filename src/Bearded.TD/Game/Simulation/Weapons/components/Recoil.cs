using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("recoil")]
sealed class Recoil(Recoil.IParameters parameters)
    : Component<Recoil.IParameters>(parameters), IListener<ShotProjectiles>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        AngularVelocity Impulse { get; }
    }

    private IAngularAccelerator? accelerator;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IAngularAccelerator>(Owner, Events, a => accelerator = a);

        Events.Subscribe(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(ShotProjectiles e)
    {
        accelerator?.Impact(
            Parameters.Impulse * StaticRandom.Float(0.5f, 1) * StaticRandom.Sign()
        );
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
