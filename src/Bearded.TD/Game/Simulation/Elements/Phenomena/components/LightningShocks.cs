using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

sealed class LightningShocks : Component, IListener<DrawComponents>
{
    private float shocksRenderStrengthGoal = 1;
    private float shocksRenderStrength;

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (StaticRandom.Bool(elapsedTime.NumericValue * 10))
        {
            shocksRenderStrengthGoal = StaticRandom.Float(0.5f, 1);
        }

        shocksRenderStrength +=
            (shocksRenderStrengthGoal - shocksRenderStrength) * (1 - (float)Math.Pow(0.001, elapsedTime.NumericValue));
    }

    public void HandleEvent(DrawComponents e)
    {
        e.Core.PointLight.Draw(
            Owner.Position.NumericValue,
            1.5f * shocksRenderStrength,
            Color.AliceBlue * shocksRenderStrength
        );
    }
}
