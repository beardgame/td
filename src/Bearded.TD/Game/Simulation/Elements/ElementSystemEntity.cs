using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

interface IElementSystemEntity
{
    bool TryApplyEffect<T>(ElementalEffectAttempt<T> attempt) where T : IElementalEffect<T>;
}

sealed class ElementSystemEntity : Component, IElementSystemEntity
{
    private readonly Dictionary<Type, IElementalEffect.IScope> effectScopes = new();
    private TickCycle? tickCycle;

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        tickCycle = new TickCycle(Owner.Game, applyTicks);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (effectScopes.Count == 0) return;
        tickCycle?.Update();
    }

    private void applyTicks(Instant now)
    {
        foreach (var (_, scope) in effectScopes)
        {
            scope.ApplyTick(now);
        }
    }

    public bool TryApplyEffect<T>(ElementalEffectAttempt<T> attempt) where T : IElementalEffect<T>
    {
        applyModifiers(ref attempt);

        if (!Random.Shared.NextBool(attempt.Probability)) return false;

        applyEffect(attempt.Effect);
        return true;
    }

    private void applyModifiers<T>(ref ElementalEffectAttempt<T> attempt) where T : IElementalEffect<T>
    {
        var preview = new PreviewElementalEffectAttempt<T>(attempt);
        Events.Preview(ref preview);
        attempt = preview.Attempt;
    }

    private void applyEffect<T>(T effect) where T : IElementalEffect<T>
    {
        var scope = effectScopes.GetOrInsert(
            typeof(T), (Effect: effect, Owner), static ctx => ctx.Effect.NewScope(ctx.Owner));
        ((IElementalEffect<T>.IScope) scope).Adopt(effect, Owner.Game.Time);
    }
}
