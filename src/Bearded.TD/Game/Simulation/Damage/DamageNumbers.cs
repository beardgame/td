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

    private readonly record struct Number(
        string Amount, Color Color, Instant StartTime, Position3 Origin, Velocity3 Velocity);

    private readonly Queue<Number> numbers = new();

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
        var amount = e.Result.TotalDiscreteDamage;

        if (amount <= HitPoints.Zero)
        {
            return;
        }

        var p = e.Target.Position;
        var damage = e.Result.TotalExactDamage;
        var type = damage.Type;

        var color = type.ToElement().GetColor();

        var number = new Number(
            amount.NumericValue.ToString(CultureInfo.InvariantCulture),
            color, Owner.Game.Time,
            p, new Velocity3(0, 0.5f, 0));

        numbers.Enqueue(number);
    }

    public void HandleEvent(DrawComponents e)
    {
        var now = Owner.Game.Time;

        while (numbers.TryPeek(out var number) && now > number.StartTime + lifeTime)
            numbers.Dequeue();

        foreach (var number in numbers)
        {
            var elapsedTime = now - number.StartTime;
            var position = number.Origin + number.Velocity * elapsedTime;

            e.Core.InGameConsoleFont.DrawLine(
                number.Color,
                position.NumericValue,
                number.Amount,
                0.25f,
                0.5f, 0.5f
                );
        }
    }
}
