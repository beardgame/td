using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Sounds.PlaySoundOnTrigger;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("playSoundOnTrigger")]
sealed class PlaySoundOnTrigger(IParameters parameters) : Component<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ITrigger Trigger { get; }
        ISoundEffect Sound { get; }
        TimeSpan Cooldown { get; }
    }

    private ITriggerSubscription? subscription;
    private Instant lastPlayed;

    protected override void OnAdded() { }

    public override void Activate()
    {
        subscription = Parameters.Trigger.Subscribe(Events, playSound);
    }

    private void playSound()
    {
        if (Owner.Game.Time - lastPlayed < Parameters.Cooldown)
        {
            return;
        }
        Owner.Game.Meta.SoundScape.PlaySoundAt(Parameters.Sound, Owner.Position);
        lastPlayed = Owner.Game.Time;
    }

    protected override void OnRemovedInternal()
    {
        subscription?.Unsubscribe(Events);
        base.OnRemovedInternal();
    }

    public override void Update(TimeSpan elapsedTime) { }
}
