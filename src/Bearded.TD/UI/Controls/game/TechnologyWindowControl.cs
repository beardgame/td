using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class TechnologyWindowControl : CompositeControl
{
    private const double contentWidth = 960;
    private const double contentHeight = 720;
    private const double minRowHeight = 48;

    private static readonly IReadOnlyList<TechnologyTier> shownTiers =
        [TechnologyTier.Low, TechnologyTier.Medium, TechnologyTier.High];

    private static readonly double tierColumnWidth = contentWidth / shownTiers.Count;

    private readonly TechnologyWindow model;
    private readonly UIFactories factories;
    private readonly Control window;

    private bool dragging;
    private Vector2d lastDragMousePosition;

    public TechnologyWindowControl(TechnologyWindow model, UIFactories factories)
    {
        this.model = model;
        this.factories = factories;
        IsClickThrough = true;
        window = factories.Window(b => b
            .WithTitle("Technology")
            .WithOnClose(model.CloseWindow)
            .WithContent(buildContent())
            .WithShadow()
        ).AnchorAsWindow(contentWidth, contentHeight);
        Add(window);
    }

    public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.MouseButton == MouseButton.Left)
        {
            dragging = true;
            lastDragMousePosition = eventArgs.MousePosition;
            eventArgs.Handled = true;
        }
    }

    public override void PreviewMouseMoved(MouseEventArgs eventArgs)
    {
        if (dragging == false)
        {
            return;
        }

        if (!eventArgs.MouseButtons.Left)
        {
            dragging = false;
            return;
        }

        var move = eventArgs.MousePosition - lastDragMousePosition;

        var currentPosition = window.Frame.TopLeft;
        var newPosition = currentPosition + move;

        window.Anchor(a => a
            .Top(margin: newPosition.Y, height: contentHeight)
            .Left(margin: newPosition.X, width: contentWidth)
        );

        lastDragMousePosition = eventArgs.MousePosition;
        eventArgs.Handled = true;
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    private Control buildContent()
    {
        var control = new CompositeControl
        {
            buildHeaderRow().Anchor(a => a.Top(height: minRowHeight)),
            buildBranchRows().Anchor(a => a.Top(margin: minRowHeight)),
        };
        return control;
    }

    private Control buildHeaderRow()
    {
        var control = new CompositeControl();
        control.BuildFixedRow()
            .AddColumnHeader("I", tierColumnWidth)
            .AddColumnHeader("II", tierColumnWidth)
            .AddColumnHeader("III", tierColumnWidth);
        return control;
    }

    private Control buildBranchRows()
    {
        var control = new CompositeControl();
        var column = control.BuildScrollableColumn();
        foreach (var branch in Enum.GetValues<TechnologyBranch>())
        {
            var row = buildBranchRow(branch, out var height);
            column.Add(row, height);
        }

        control.Add(
            new ComplexBox
            {
                CornerRadius = 2,
                Components = [
                    Edge.Outer(1, Colors.Get(BackgroundColor.WindowInsetLine)),
                    Glow.Outer(4, Shadows.Default.Color * 0.5f),
                ],
                IsClickThrough = true,
            }
        );

        return control;
    }

    private Control buildBranchRow(TechnologyBranch branch, out double height)
    {
        var branchModel = model.TechTree.Branches[branch];
        var branchColor = branch.GetColor();
        var headerBackgroundColor = Colors.Get(BackgroundColor.HeaderBackground);

        var isEmpty = shownTiers.All(t => branchModel.Tiers[t].Technologies.IsEmpty);

        var disabledHeaderColor = Color.Lerp(branchColor, Colors.Get(ForeGroundColor.DisabledText), 0.8f);
        var headerColor = isEmpty ? disabledHeaderColor : branchColor;

        var control = new CompositeControl
        {
            TextFactories.Header(
                    $"{branch}",
                    color: isEmpty ? disabledHeaderColor : branchColor,
                    backgroundColor: ShapeColor.From(
                        [
                            (0.2, Color.Lerp(headerColor, headerBackgroundColor, 0.8f) * 0.9f),
                            (0.6, Color.Lerp(headerColor, headerBackgroundColor, 0.5f) * 0.9f),
                        ],
                        GradientDefinition.Linear(
                            AnchorPoint.Relative((0, -10)),
                            AnchorPoint.Relative((1, 10))
                        )
                    ))
                .Anchor(a => a
                    .MarginAllSides(0.5 * LayoutMarginSmall)
                    .Top(margin: 0.5 * LayoutMarginSmall, height: Text.HeaderLineHeight - LayoutMarginSmall)
                ),
        };

        height = Text.HeaderLineHeight;

        if (isEmpty)
        {
            control.Add(
                TextFactories.Header("coming soon", color: disabledHeaderColor, textAnchor: (0.5, 0.5))
            );
            return control;
        }

        control.Add(
            buildBranchRowContent(branchModel, branchColor, out var rowHeight)
                .Anchor(a => a.Top(margin: Text.HeaderLineHeight))
        );
        height += rowHeight;
        return control;
    }

    private Control buildBranchRowContent(TechTree.Branch branchModel, Color backgroundColor, out double height)
    {
        var control = new CompositeControl();
        var row = control.BuildFixedRow();

        height = minRowHeight;

        foreach (var tier in shownTiers)
        {
            var tierModel = branchModel.Tiers[tier];
            var techList = tierModel.Technologies;

            var buttons = new CompositeControl();
            var buttonColumn = buttons.BuildFixedColumn();
            foreach (var tech in techList)
            {
                buttonColumn.AddCenteredButton(factories, b => b
                    .WithLabel(tech.Blueprint.Name)
                    .WithEnabled(tech.IsUnlockedBinding.Or(model.CanUnlockTechnologyNowBinding))
                    .WithActive(tech.IsUnlockedBinding)
                    .WithTooltip(tech.Blueprint.Unlocks.Select(u => u.Description).ToImmutableArray())
                    .WithOnClick(args =>
                    {
                        if (args.ModifierKeys.IsSupersetOf(Constants.Input.DebugForceModifier))
                        {
                            model.ForceTechnologyUnlock(tech.Blueprint);
                        }
                        else
                        {
                            model.RequestTechnologyUnlock(tech.Blueprint);
                        }
                    }));
            }

            var tierControl = new CompositeControl
            {
                new BackgroundBox(backgroundColor * 0.1f),
                ProgressBarFactories.BareProgressBar(tierModel.CompletionPercentageBinding, backgroundColor * 0.15f),
                buttons.SurroundWithMargins(LayoutMargin),
            };

            row.AddLeft(
                tierControl.SurroundWithMargins(0.5 * LayoutMarginSmall),
                tierColumnWidth);
            height = Math.Max(height, buttonColumn.Height + 2 * LayoutMargin + LayoutMarginSmall);
        }

        return control;
    }
}
