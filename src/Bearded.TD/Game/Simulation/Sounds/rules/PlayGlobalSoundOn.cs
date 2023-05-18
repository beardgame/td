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
        context.Events.Subscribe(new Listener(context.GameState.Meta.SoundScape, () => SoundEffect));
    }

    private sealed class Listener : IListener<TEvent>
    {
        private readonly ISoundScape soundScape;
        private readonly Func<ISoundEffect> getSoundEffect;

        public Listener(ISoundScape soundScape, Func<ISoundEffect> getSoundEffect)
        {
            this.soundScape = soundScape;
            this.getSoundEffect = getSoundEffect;
        }

        public void HandleEvent(TEvent @event)
        {
            var soundEffect = getSoundEffect();
            soundScape.PlayGlobalSound(soundEffect);
        }
    }
}
