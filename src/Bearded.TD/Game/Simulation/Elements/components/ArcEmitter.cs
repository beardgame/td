using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("arcEmitter")]
sealed class ArcEmitter : Component<ArcEmitter.IParameters>, IListener<FireWeapon>
{
    private IWeaponRange range;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Arc { get; }
        TimeSpan LifeTime { get; }
    }

    public ArcEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponRange>(Owner, Events, r => range = r);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(FireWeapon e)
    {
        var targetLayer = Owner.Game.TargetLayer;
        var conductiveLayer = Owner.Game.ConductiveLayer;

        var tilesInRange = range.GetTilesInRange();
        var target = tilesInRange
            .SelectMany(
                t => targetLayer.GetObjectsOnTile(t)
                    .Concat(conductiveLayer.GetObjectsOnTile(t)))
            .Distinct()
            .RandomElement();

        var arc = ArcFactory.CreateArc(Parameters.Arc, Owner, target, e.Damage, Parameters.LifeTime);
        Owner.Game.Add(arc);
    }
}
