using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Sounds;

[Component("loopSoundWhileShooting")]
sealed class LoopSoundWhileShooting : Component<LoopSoundWhileShooting.IParameters>, IListener<ObjectDeleting>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISoundEffect Sound { get; }
    }

    private ISoundLoop? soundLoop;
    private IWeaponTrigger? trigger;

    public LoopSoundWhileShooting(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        ComponentDependencies.DependDynamic<IWeaponTrigger>(Owner, Events, t => trigger = t);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe(this);
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        stopSound();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        soundLoop?.MoveTo(Owner.Position);

        var triggerActive = trigger?.TriggerPulled ?? false;
        var soundActive = soundLoop is { };

        switch (triggerActive, soundActive)
        {
            case (true, false):
                startSound();
                break;
            case (false, true):
                stopSound();
                break;
        }
    }

    private void startSound()
    {
        State.Satisfies(soundLoop is null);
        soundLoop = Owner.Game.Meta.SoundScape.LoopSoundAt(Parameters.Sound.Sound, Owner.Position);
    }

    private void stopSound()
    {
        soundLoop?.Stop();
        soundLoop = null;
    }
}
