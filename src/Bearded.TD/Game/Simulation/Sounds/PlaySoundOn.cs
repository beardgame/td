using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Sounds;

abstract class PlaySoundOn<TParameters, TEvent> : Component<TParameters>, IListener<TEvent>
    where TParameters : IParametersTemplate<TParameters>
    where TEvent : struct, IComponentEvent
{
    protected abstract ISoundEffect SoundEffect { get; }

    protected PlaySoundOn(TParameters parameters) : base(parameters) { }

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        Events.Subscribe(this);
    }

    public void HandleEvent(TEvent @event)
    {
        Owner.Game.Meta.SoundScape.PlaySoundAt(SoundEffect.Sound, Owner.Position, SoundEffect.PitchRange.ChooseRandomPitch());
    }

    public override void Update(TimeSpan elapsedTime) { }
}
