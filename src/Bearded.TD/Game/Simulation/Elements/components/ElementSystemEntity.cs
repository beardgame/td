using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

interface IElementSystemEntity
{
    void ApplyEffect<T>(T effect) where T : IElementalEffect;
}

sealed class ElementSystemEntity : Component, IElementSystemEntity
{
    private readonly Dictionary<Type, IElementalPhenomenon.IScope> effectScopes = new();
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

    public void ApplyEffect<T>(T effect) where T : IElementalEffect
    {
        if (!effectScopes.TryGetValue(typeof(T), out var scope))
        {
            if (effect.Phenomenon.EffectType != typeof(T))
            {
                throw new InvalidOperationException("Type of effect must be same as phenomenon effect type.");
            }
            scope = effect.Phenomenon.NewScope(Owner);
            effectScopes.Add(typeof(T), scope);
        }

        ((IElementalPhenomenon.IScope<T>) scope).Adopt(effect, Owner.Game.Time);
    }
}
