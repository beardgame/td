using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatusControl : CompositeControl
{
    public static readonly Vector2d Size = (100, 20);

    private readonly BuildingStatus model;
    private readonly List<Control> statusIcons = [];

    public BuildingStatusControl(BuildingStatus model)
    {
        this.model = model;
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new BackgroundBox());
        Add(TextFactories.Label(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview")));

        model.Statuses.CollectionSize<ImmutableArray<Status>, Status>().SourceUpdated += updateStatusIconCount;
        updateStatusIconCount(model.Statuses.Value.Length);
    }

    private void updateStatusIconCount(int newCount)
    {
        var currentCount = statusIcons.Count;
        if (currentCount == newCount) return;

        if (currentCount > newCount)
        {
            for (var i = newCount; i < currentCount; i++)
            {
                Remove(statusIcons[i]);
            }

            statusIcons.RemoveRange(newCount, currentCount - newCount);
        }
        else
        {
            for (var i = currentCount; i < newCount; i++)
            {
                var button = statusIconForIndex(i);
                statusIcons.Add(button);
                Add(button);
            }
        }
    }

    private Control statusIconForIndex(int i)
    {
        var binding = model.Statuses.ListElementByIndex<ImmutableArray<Status>, Status>(i);
        var control = StatusIconFactories.StatusIcon(binding);
        control.Anchor(a => a
            .Left(margin: buttonLeftMargin(i), width: buttonSize)
            .Top(relativePercentage: 0.5, margin: -0.5 * buttonSize, height: buttonSize)
        );
        return control;
    }

    private const double buttonBetweenMargin = Constants.UI.Button.Margin;
    private const double buttonSize = Constants.UI.Button.SquareButtonSize;
    private static double buttonLeftMargin(int i) => i * (buttonSize + buttonBetweenMargin);
}
