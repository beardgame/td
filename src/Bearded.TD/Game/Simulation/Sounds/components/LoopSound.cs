using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("loopSound")]
sealed class LoopSound : Component<LoopSound.IParameters>, IListener<ObjectDeleting>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    private ISoundLoop? soundLoop;

    public LoopSound(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        startSound();
    }

    private void startSound()
    {
        soundLoop = Owner.Game.Meta.SoundScape.LoopSoundAt(Parameters.Sound.Sound, Owner.Position);
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        stopSound();
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        stopSound();
        Events.Unsubscribe(this);
    }

    private void stopSound()
    {
        soundLoop?.Stop();
        soundLoop = null;
    }

    public override void Update(TimeSpan elapsedTime) { }
}
