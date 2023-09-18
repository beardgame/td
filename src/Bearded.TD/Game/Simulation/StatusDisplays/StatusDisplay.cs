using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.GameUI.StatusDisplay;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class StatusDisplay : Component, IStatusDisplay, IListener<DrawComponents>
{
    private readonly IStatusDisplayCondition? condition;
    private readonly List<HitPointsBar> hitPointsBars = new();

    private Position3 center => (Owner.Position.XY() + Offset).WithZ(Z);

    public StatusDisplay() : this(null) { }

    public StatusDisplay(IStatusDisplayCondition? condition)
    {
        this.condition = condition;
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        condition?.Activate(Owner);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void AddHitPointsBar(HitPointsBar bar)
    {
        hitPointsBars.Add(bar);
        hitPointsBars.Sort(Comparer.Comparing<HitPointsBar, int>(b => (int) b.Shell));
    }

    public void HandleEvent(DrawComponents @event)
    {
        if (!condition?.ShouldDraw ?? false)
        {
            return;
        }

        drawHitPointsBars(@event.Core.ConsoleBackground);
    }

    private void drawHitPointsBars(IShapeDrawer2<Color> drawer)
    {
        // TODO: consider combining bars of the same shells in the some row
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // given the implementation of current/total, this is guaranteed by IEEE754
        if (hitPointsBars.Count == 0 ||
            (percentage(hitPointsBars[^1].Pool) == 1 && !UserSettings.Instance.UI.AlwaysShowHealth))
        {
            return;
        }

        for (var i = 0; i < hitPointsBars.Count; i++)
        {
            drawBar(drawer, hitPointsBars[i], i);
        }
    }

    private void drawBar(IShapeDrawer2<Color> drawer, HitPointsBar bar, int i)
    {
        var size = new Difference2(Width, i == 0 ? PrimaryHitPointsBarHeight : SecondaryHitPointsBarHeight);
        var barVerticalOffset =
            i == 0
                ? Unit.Zero
                : new Unit(i * (ElementMargin + SecondaryHitPointsBarHeight) +
                    0.5f * (PrimaryHitPointsBarHeight - SecondaryHitPointsBarHeight));
        var topLeft = center + (new Difference2(0.U(), barVerticalOffset) - 0.5f * size).WithZ();

        var p = percentage(bar.Pool);

        drawer.FillRectangle(topLeft.NumericValue, size.NumericValue, HitPointsBackgroundColor);
        drawer.FillRectangle(topLeft.NumericValue, new Difference2(size.X * p, size.Y).NumericValue, bar.Color);
    }

    private static float percentage(IHitPointsPool pool) => pool.CurrentHitPoints / pool.MaxHitPoints;
}
