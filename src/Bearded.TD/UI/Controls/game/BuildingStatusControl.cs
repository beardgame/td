using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.BuildingStatus;
using Animations = Bearded.TD.UI.Animation.Animations;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl : CompositeControl
{
    private static readonly Vector2d buttonBetweenMargin = (Constants.UI.Button.Margin, Constants.UI.Button.Margin * 3);
    private static readonly double rowHeight = ButtonSize + buttonBetweenMargin.Y;
    private static double buttonLeftMargin(int i) => i * (ButtonSize + buttonBetweenMargin.X);

    public Vector2d Size { get; }

    public BuildingStatusControl(
        BuildingStatus model,
        UIContext uiContext,
        GameRequestDispatcher requestDispatcher)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new BlurBackground());

        addBackground(model, uiContext.Animations);
        var innerContainer = new CompositeControl();
        Add(innerContainer.Anchor(a => a.MarginAllSides(Padding).Top()));

        var column = innerContainer.BuildFixedColumn();
        column.AddHeader(Binding.Constant(model.BuildingName));

        if (model.ShowVeterancy)
        {
            column.Add(new VeterancyRow(model.Veterancy, uiContext.Animations), Veterancy.RowHeight);
        }

        column
            .Add(new IconRow<ObservableStatus>(
                    model.Statuses,
                    status => uiContext.Factories.StatusIcon(status, requestDispatcher),
                    StatusRowBackground),
                rowHeight);

        if (model.ShowUpgrades)
        {
            column
                .Add(new IconRow<IReadonlyBinding<UpgradeSlot>>(
                        model.Upgrades,
                        slot => uiContext.Factories.UpgradeSlot(slot,
                            model.AvailableUpgrades.IsCountPositive()
                                .And(Binding.Combine(slot, model.ActiveUpgradeSlot, (s, i) => s.Index == i)),
                            model.ToggleUpgradeSelect)),
                    rowHeight)
                .Add(new UpgradeSelectRow(
                        model.AvailableUpgrades,
                        model.ActiveUpgradeSlot,
                        model.CurrentResources,
                        model.ApplyUpgrade,
                        uiContext.Factories).BindIsVisible(model.ShowUpgradeSelect),
                    rowHeight);
        }

        if (model.ShowDeletion)
        {
            column
                .AddLeftButton(uiContext.Factories, b => b
                    .WithLabel("Delete")
                    .WithOnClick(model.DeleteBuilding));
        }

        Size = (300, column.Height + Padding);
    }

    private void addBackground(BuildingStatus model, Animations animations)
    {
        const int fillIndex = 1;

        IAnimationController? backgroundAnimation = null;

        var components = new[]
        {
            Fill.With(GradientDefinition.BlurredBackground()),
            Fill.With(expectedColor()), // Index should match constant
            Edge.Inner((float) EdgeWidth, Colors.Get(BackgroundColor.TooltipOutline)),
            BackgroundFade,
        };

        this.Add(new ComplexBox
        {
            Components = ShapeComponents.FromMutable(components),
            CornerRadius = 5
        }.WithDropShadow(Shadow, ShadowFade));

        model.ShowExpanded.ControlUpdated += _ => updateBackground();
        model.ShowExpanded.SourceUpdated += _ => updateBackground();

        return;

        void updateBackground()
        {
            backgroundAnimation?.Cancel();
            var currentColor = components[fillIndex].Color.Definition.Color;
            var targetColor = expectedColor();
            backgroundAnimation = animations.Start(AnimationFunction
                .ColorFromTo(AnimationDurations.Short)
                .WithState((c => components[fillIndex] = Fill.With(c), currentColor, targetColor)));
        }

        Color expectedColor() => model.ShowExpanded.Value ? ExpandedBackgroundColor : PreviewBackgroundColor;
    }
}
