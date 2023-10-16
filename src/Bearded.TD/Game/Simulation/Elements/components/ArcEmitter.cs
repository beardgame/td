using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("arcEmitter")]
sealed class ArcEmitter : ArcEmitterBase<ArcEmitter.IParameters>, IListener<FireWeapon>
{
    private IWeaponRange range = null!;

    public interface IParameters : IArcEmissionParameters, IParametersTemplate<IParameters> { }

    public ArcEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponRange>(Owner, Events, r => range = r);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(FireWeapon e)
    {
        EmitArc(e.Damage, range.GetTilesInRange());
    }
}
