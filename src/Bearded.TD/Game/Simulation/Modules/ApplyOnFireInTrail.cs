using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("applyOnFireInTrail")]
sealed class ApplyOnFireInTrail : Component<ApplyOnFireInTrail.IParameters>
{
    private Trail trail = null!;
    private Instant nextTick;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        UntypedDamagePerSecond DamagePerSecond { get; }
        TimeSpan Duration { get; }
        bool ExcludeSelf { get; }
        bool ExcludeParent { get; }
    }

    public ApplyOnFireInTrail(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<Trail>(Owner, Events, t => trail = t);

    }

    public override void Activate()
    {
        nextTick = Owner.Game.Time + TickDuration;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        while (nextTick < Owner.Game.Time)
        {
            applyFire();

            nextTick += TickDuration;
        }
    }

    private void applyFire()
    {
        if (trail.Parts.Count <= 1)
            return;

        var tiles = trail.Parts.Select(p => Level.GetTile(p.Point)).Distinct();

        var targets = tiles
            .SelectMany(Owner.Game.PhysicsLayer.GetObjectsOnTile)
            .Where(notExcluded);

        var effect = new OnFire.Effect(Parameters.DamagePerSecond, null, Parameters.Duration);

        foreach (var target in targets)
        {
            target.TryApplyEffect(effect);
        }
    }

    private bool notExcluded(GameObject o)
    {
        if (Parameters.ExcludeSelf && o == Owner)
            return false;
        if (Parameters.ExcludeParent && o == Owner.Parent)
            return false;
        return true;
    }
}

