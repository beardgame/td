using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.GameUI.StatusDisplay;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusDisplay : Component, IStatusDisplay, IListener<DrawComponents>
{
    private readonly IStatusDisplayCondition? condition;

    private Position3 center => Owner.Position + Offset;

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

    public override void Update(TimeSpan elapsedTime)
    {
        statuses.RemoveAll(s => s.Expiry is { } expiry && expiry <= Owner.Game.Time);
    }

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
