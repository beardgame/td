using System;
using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.UpdateLoop;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Sounds;

abstract class PlayGlobalSoundOn<TParameters, TEvent>(TParameters parameters) : GameRule<TParameters>(parameters)
    where TEvent : struct, IGlobalEvent
{
    protected abstract ISoundEffect SoundEffect { get; }
    protected abstract TimeSpan Cooldown { get; }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(
            context.GameState.Meta.SoundScape,
            context.GameState.GameTime,
            ShouldPlaySoundEffect,
            () => SoundEffect,
            () => Cooldown));
    }

    protected virtual bool ShouldPlaySoundEffect(TEvent @event) => true;

    private sealed class Listener(
        ISoundScape soundScape,
        GameTime gameTime,
        Func<TEvent, bool> shouldPlay,
        Func<ISoundEffect> getSoundEffect,
        Func<TimeSpan> getCooldown) : IListener<TEvent>
    {
        private Instant lastPlayed;

        public void HandleEvent(TEvent @event)
        {
            if (!shouldPlay(@event) || gameTime.Time - lastPlayed < getCooldown()) return;
            var soundEffect = getSoundEffect();
            soundScape.PlayGlobalSound(soundEffect);
            lastPlayed = gameTime.Time;
        }
    }
}
