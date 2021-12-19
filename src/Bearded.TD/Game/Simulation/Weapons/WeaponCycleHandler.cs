using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

abstract class WeaponCycleHandler<TParameters> : Component<ComponentGameObject, TParameters>
    where TParameters : IParametersTemplate<TParameters>
{
    private IWeaponTrigger? trigger;
    protected IWeaponState Weapon { get; private set; } = null!;
    protected GameState Game => Owner.Game;

    protected WeaponCycleHandler(TParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => Weapon = c);
        ComponentDependencies.DependDynamic<IWeaponTrigger>(Owner, Events, c => trigger = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (trigger?.TriggerPulled ?? false)
        {
            UpdateShooting(elapsedTime);
        }
        else
        {
            UpdateIdle(elapsedTime);
        }
    }

    protected virtual void UpdateShooting(TimeSpan elapsedTime)
    {
    }

    protected virtual void UpdateIdle(TimeSpan elapsedTime)
    {
    }
}
