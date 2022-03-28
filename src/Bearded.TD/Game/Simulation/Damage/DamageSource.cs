using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

interface IDamageSource
{
    void AttributeDamage(DamageResult result);
    void AttributeKill(IDamageTarget target);
}

sealed class DamageSource : Component, IDamageSource
{
    private IIdProvider? idProvider;
    public Id<ComponentGameObject> Id => idProvider?.Id ?? Id<ComponentGameObject>.Invalid;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IIdProvider>(Owner, Events, provider => idProvider = provider);
    }

    public void AttributeDamage(DamageResult damageResult)
    {
        Events.Send(new CausedDamage(damageResult));
    }

    public void AttributeKill(IDamageTarget target)
    {
        Events.Send(new CausedKill(target));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
