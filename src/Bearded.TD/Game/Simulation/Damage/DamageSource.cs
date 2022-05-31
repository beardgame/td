using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

interface IDamageSource
{
    void AttributeDamage(DamageResult result);
    void AttributeKill();
}

sealed class DamageSource : Component, IDamageSource
{
    private IIdProvider? idProvider;
    public Id<GameObject> Id => idProvider?.Id ?? Id<GameObject>.Invalid;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IIdProvider>(Owner, Events, provider => idProvider = provider);
    }

    public void AttributeDamage(DamageResult damageResult)
    {
        Events.Send(new CausedDamage(damageResult));
    }

    public void AttributeKill()
    {
        Events.Send(new CausedKill());
    }

    public override void Update(TimeSpan elapsedTime) { }
}
