using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("deleteOnTrigger")]
sealed class DeleteOnTrigger(DeleteOnTrigger.IParameters parameters)
    : Component<DeleteOnTrigger.IParameters>(parameters)
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ITrigger Trigger { get; }

        [Modifiable(1)]
        int Count { get; }
    }

    private int counted;
    private ITriggerSubscription subscription = null!;

    protected override void OnAdded()
    {
        subscription = Parameters.Trigger.Subscribe(Events, onTrigger);
    }

    protected override void OnRemovedInternal()
    {
        subscription.Unsubscribe(Events);
    }

    private void onTrigger()
    {
        counted++;

        if (counted >= Parameters.Count)
        {
            Owner.Delete();
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
