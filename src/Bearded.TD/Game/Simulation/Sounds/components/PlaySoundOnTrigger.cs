using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnTrigger")]
sealed class PlaySoundOnTrigger : Component<PlaySoundOnTrigger.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ITrigger Trigger { get; }
        ISoundEffect Sound { get; }
    }

    private ITriggerSubscription? subscription;

    public PlaySoundOnTrigger(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        subscription = Parameters.Trigger.Subscribe(Events, playSound);
    }

    private void playSound()
    {
        Owner.Game.Meta.SoundScape.PlaySoundAt(
            Parameters.Sound.Sound, Owner.Position, Parameters.Sound.PitchRange.ChooseRandomPitch());
    }

    public override void OnRemoved()
    {
        subscription?.Unsubscribe(Events);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime) { }
}
