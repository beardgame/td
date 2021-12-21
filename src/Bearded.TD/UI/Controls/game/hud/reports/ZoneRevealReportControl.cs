using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ZoneRevealReportControl : ReportControl
{
    private readonly GameInstance game;
    private readonly IZoneRevealReport report;
    private readonly FactionExploration? exploration;
    private readonly Binding<bool> canReveal = new();

    public override double Height { get; }

    public ZoneRevealReportControl(GameInstance game, IZoneRevealReport report)
    {
        this.game = game;
        this.report = report;
        game.Me.Faction.TryGetBehaviorIncludingAncestors(out exploration);

        var column = this.BuildFixedColumn();
        column.AddButton(
            b => b.WithLabel("Reveal (consumes token)").WithEnabled(canReveal).WithOnClick(revealZone));
        Height = column.Height;
    }

    private void revealZone()
    {
        game.Request(RevealZone.Request, game.Me.Faction, report.Zone);
    }

    public override void Update()
    {
        canReveal.SetFromSource(exploration != null && report.CanRevealNow(exploration));
    }

    public override void Dispose() {}
}
