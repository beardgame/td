using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

abstract class WeaponCycleHandler<TParameters> : Component<TParameters>
    where TParameters : IParametersTemplate<TParameters>
{
    private IWeaponTrigger? trigger;
    protected GameState Game => Owner.Game;

    protected WeaponCycleHandler(TParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        ComponentDependencies.DependDynamic<IWeaponTrigger>(Owner, Events, c => trigger = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (trigger?.TriggerPulled ?? false)
        {
            UpdateShooting();
        }
        else
        {
            UpdateIdle();
        }
    }

    protected virtual void UpdateShooting() {}

    protected virtual void UpdateIdle() {}
}
