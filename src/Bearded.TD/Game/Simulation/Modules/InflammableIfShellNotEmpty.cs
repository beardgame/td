using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Game.Simulation.Modules.InflammableIfShellNotEmpty;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("inflammableIfShellNotEmpty")]
sealed class InflammableIfShellNotEmpty(IParameters parameters)
    : Component<IParameters>(parameters), IPreviewListener<PreviewElementalEffectAttempt<OnFire.Effect>>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        DamageShell Shell { get; }
    }

    private IHitPointsPool? pool;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IHitPointsPool>(Owner, Events, s => pool = s, s => s.Shell == Parameters.Shell);

        Events.Subscribe(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe(this);
    }

    public void PreviewEvent(ref PreviewElementalEffectAttempt<OnFire.Effect> e)
    {
        if (pool?.CurrentHitPoints == HitPoints.Zero)
            e = e.Cancelled();
    }
}
