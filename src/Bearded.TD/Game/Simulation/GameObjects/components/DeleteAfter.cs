using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("deleteAfter")]
sealed class DeleteAfter : Component<DeleteAfter.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan TimeSpan { get; }
    }

    private TimeSpan timeAlive = TimeSpan.Zero;

    public DeleteAfter(IParameters parameters) : base(parameters)
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
