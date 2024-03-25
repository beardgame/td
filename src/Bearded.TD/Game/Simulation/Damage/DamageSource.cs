using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

interface IDamageSource
{
    void AttributeDamage(FinalDamageResult result, GameObject damagedObject);
}

sealed class DamageSource : Component, IDamageSource
{
    private IIdProvider? idProvider;
    public Id<GameObject> Id => idProvider?.Id ?? Id<GameObject>.Invalid;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IIdProvider>(Owner, Events, provider => idProvider = provider);
    }

    public void AttributeDamage(FinalDamageResult damageResult, GameObject damagedObject)
    {
        Events.Send(new CausedDamage(damageResult, damagedObject));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
