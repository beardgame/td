using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.BuildingStatus;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl : CompositeControl
{
    private static readonly Vector2d buttonBetweenMargin = (Constants.UI.Button.Margin, Constants.UI.Button.Margin * 3);
    private static double buttonLeftMargin(int i) => i * (ButtonSize + buttonBetweenMargin.X);

    public Vector2d Size { get; }

    public BuildingStatusControl(BuildingStatus model, Animations animations, GameRequestDispatcher requestDispatcher)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new ComplexBox
        {
            Components = Background,
            CornerRadius = 5,
        });

        var innerContainer = new CompositeControl();
        Add(innerContainer.Anchor(a => a.MarginAllSides(Padding).Top()));

        var column = innerContainer.BuildFixedColumn();
        column
            .AddHeader(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview"))
            .Add(new VeterancyRow(model.Veterancy, animations), Veterancy.RowHeight)
            .Add(new IconRow<ObservableStatus>(
                    model.Statuses,
                    status => StatusIconFactories.StatusIcon(status, animations, requestDispatcher),
                    StatusRowBackground),
                ButtonSize + buttonBetweenMargin.Y)
            .Add(new IconRow<IReadonlyBinding<UpgradeSlot>>(
                    model.Upgrades, slot => StatusIconFactories.UpgradeSlot(slot, animations)),
                ButtonSize + buttonBetweenMargin.Y);

        Size = (300, column.Height + Padding);
    }
}
