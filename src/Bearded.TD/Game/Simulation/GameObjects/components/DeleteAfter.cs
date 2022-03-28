using Bearded.TD.Content.Models;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("deleteAfter")]
sealed class DeleteAfter : Component<IDeleteAfterParameters>
{
    private TimeSpan timeAlive = TimeSpan.Zero;

    public DeleteAfter(IDeleteAfterParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        timeAlive += elapsedTime;
        if (timeAlive >= Parameters.TimeSpan)
        {
            Owner.Delete();
        }
    }
}
