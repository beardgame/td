using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.GameUI.StatusDisplay;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusRenderer : Component, IListener<DrawComponents>
{
    private readonly IStatusTracker tracker;
    private readonly IStatusDisplayCondition? condition;

    private Position3 center => Owner.Position + Offset;

    public StatusRenderer(IStatusTracker tracker, IStatusDisplayCondition? condition)
    {
        this.tracker = tracker;
        this.condition = condition;
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Activate()
    {
        condition?.Activate(Owner, Events);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(DrawComponents @event)
    {
        if (!condition?.ShouldDraw ?? false)
        {
            return;
        }

        drawHitPointsBars(@event.Core.ConsoleBackground);
        drawStatuses(@event.Core, @event.Drawer);
    }
}
