using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthBars : Component, IListener<DrawComponents>
{
    private ImmutableArray<IHitPointsPool> hitPointsPools = ImmutableArray<IHitPointsPool>.Empty;
    private IBuildingStateProvider? building;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, b => building = b);
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Activate()
    {
        base.Activate();
        // TODO: only gathers health bars on activate, so doesn't like dynamic health pools
        //       however, this is only temporary since we're building a unified status surface for game objects soon
        hitPointsPools = Owner.GetComponents<IHitPointsPool>().OrderBy(p => p.Shell).ToImmutableArray();
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(DrawComponents e)
    {
        if (building?.State.IsCompleted == false || hitPointsPools.IsEmpty)
        {
            return;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // given the implementation of current/total, this is guaranteed by IEEE754
        if (percentage(hitPointsPools[0]) == 1 && !UserSettings.Instance.UI.AlwaysShowHealth)
        {
            return;
        }

        var d = e.Core.ConsoleBackground;

        for (var i = 0; i < hitPointsPools.Length; i++)
        {
            drawBar(d, hitPointsPools[i], i);
        }
    }

    private void drawBar(IShapeDrawer2<Color> drawer, IHitPointsPool pool, int i)
    {
        var size = new Vector2(1, .1f);
        var topLeft = Owner.Position.NumericValue - new Vector3(.5f, .5f, 0) - i * 1.2f * size.Y * Vector3.UnitY;

        var p = percentage(pool);
        var color = getBarColor(pool.Shell, p);

        drawer.FillRectangle(topLeft, size, Color.DarkGray);
        drawer.FillRectangle(topLeft, new Vector2(size.X * p, size.Y), color);
    }

    private static Color getBarColor(DamageShell shell, float percentage) => shell switch
    {
        DamageShell.Health => Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, percentage), .8f, .8f),
        DamageShell.Armor => Color.FromHSVA(Color.Teal.Hue, .8f, .8f),
        DamageShell.Shield => Color.FromHSVA(Color.SkyBlue.Hue, .8f, .8f),
        _ => throw new ArgumentOutOfRangeException(nameof(shell), shell, null)
    };

    private static float percentage(IHitPointsPool pool) => pool.CurrentHitPoints / pool.MaxHitPoints;
}
