using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("randomShotDelay")]
sealed class RandomShotDelay(RandomShotDelay.IParameters parameters)
    : Component<RandomShotDelay.IParameters>(parameters), IPreviewListener<PreviewDelayNextShot>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0.75)]
        double MinimumFactor { get; }
        [Modifiable(1.25)]
        double MaximumFactor { get; }
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void PreviewEvent(ref PreviewDelayNextShot e)
    {
        var newDelay = e.Delay * StaticRandom.Double(Parameters.MinimumFactor, Parameters.MaximumFactor);
        e = new PreviewDelayNextShot(newDelay);
    }
}
