using System;
using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Sounds;

abstract class PlayGlobalSoundOn<TParameters, TEvent> : GameRule<TParameters>
    where TEvent : struct, IGlobalEvent
{
    protected abstract ISoundEffect SoundEffect { get; }

    protected PlayGlobalSoundOn(TParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(
            new Listener(context.GameState.Meta.SoundScape, ShouldPlaySoundEffect, () => SoundEffect));
    }

    protected virtual bool ShouldPlaySoundEffect(TEvent @event) => true;

    private sealed class Listener(
        ISoundScape soundScape, Func<TEvent, bool> shouldPlay, Func<ISoundEffect> getSoundEffect) : IListener<TEvent>
    {
        public void HandleEvent(TEvent @event)
        {
            if (!shouldPlay(@event)) return;
            var soundEffect = getSoundEffect();
            soundScape.PlayGlobalSound(soundEffect);
        }
    }
}
