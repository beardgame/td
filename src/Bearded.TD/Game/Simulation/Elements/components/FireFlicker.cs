using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

sealed class FireFlicker : Component, IListener<DrawComponents>
{
    private float fireRenderStrengthGoal = 1;
    private float fireRenderStrength;

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
            fireRenderStrengthGoal = StaticRandom.Float(0.5f, 1);
        }

        fireRenderStrength +=
            (fireRenderStrengthGoal - fireRenderStrength) * (1 - (float)Math.Pow(0.001, elapsedTime.NumericValue));
    }

    public void HandleEvent(DrawComponents e)
    {
        e.Core.PointLight.Draw(
            Owner.Position.NumericValue,
            1.5f * fireRenderStrength,
            Color.OrangeRed * fireRenderStrength
        );
    }
}
