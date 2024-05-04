using System.Collections.Generic;
using System.Globalization;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("damageNumbers")]
sealed class DamageNumbers : Component, IListener<CausedDamage>, IListener<DrawComponents>
{
    private static readonly TimeSpan lifeTime = 0.5.S();
    private static readonly TimeSpan mergeTime = 0.1.S();
    private static readonly Squared<Unit> mergeDistance = 0.5.U().Squared;

    private readonly record struct Number(
        TypedDamage Damage, string Amount, Color Color, Instant StartTime, Position3 Origin, Velocity3 Velocity);

    private readonly List<Number> numbers = [];

    protected override void OnAdded()
    {
        Events.Subscribe<CausedDamage>(this);
        Events.Subscribe<DrawComponents>(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(CausedDamage e)
    {
        var damage = new TypedDamage(e.Result.TotalDiscreteDamage, e.Result.TotalExactDamage.Type);

        if (damage.Amount <= HitPoints.Zero)
        {
            return;
        }

        var now = Owner.Game.Time;
        var p = e.Target.Position;

        for (var i = 0; i < numbers.Count; i++)
        {
            if (mergeIfPossible(numbers[i], now, damage, p) is { } merged)
            {
                numbers[i] = merged;
                return;
            }
        }

        var color = damage.Type.ToElement().GetColor();

        var number = new Number(
            damage,
            numberString(damage.Amount),
            color, now,
            p, new Velocity3(0, 0.5f, 0));

        numbers.Add(number);
    }

    private static Number? mergeIfPossible(
        Number target, Instant now, TypedDamage damage, Position3 position)
    {
        var differentType = damage.Type != target.Damage.Type;
        var tooMuchTimePassed = now > target.StartTime + mergeTime;
        var tooFarAway = (position - target.Origin).LengthSquared > mergeDistance;
        if (differentType || tooMuchTimePassed || tooFarAway)
        {
            return null;
        }

        var newDamage = new TypedDamage(damage.Amount + target.Damage.Amount, damage.Type);

        return target with
        {
            Damage = newDamage,
            Amount = numberString(newDamage.Amount),
        };
    }

    private static string numberString(HitPoints amount)
        => amount.NumericValue.ToString(CultureInfo.InvariantCulture);

    public void HandleEvent(DrawComponents e)
    {
        var now = Owner.Game.Time;

        numbers.RemoveAll(number => now > number.StartTime + lifeTime);

        foreach (var number in numbers)
        {
            var elapsedTime = now - number.StartTime;
            var position = number.Origin + number.Velocity * elapsedTime;

            e.Core.InGameFont.DrawLine(
                number.Color,
                position.NumericValue,
                number.Amount,
                0.25f,
                0.5f, 0.5f
                );
        }
    }
}
