using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnActivate")]
sealed class PlaySoundOnActivate : Component<PlaySoundOnActivate.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    public PlaySoundOnActivate(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        Owner.Game.Meta.SoundScape.PlaySoundAt(Parameters.Sound.Sound, Owner.Position);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}
