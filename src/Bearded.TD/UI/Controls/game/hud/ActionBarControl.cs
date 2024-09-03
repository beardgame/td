using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.UI.Controls;

sealed class ActionBarControl : CompositeControl
{
    private readonly ActionBar model;
    private readonly UIContext uiContext;
    private readonly List<Button> buttons = [];

    public ActionBarControl(ActionBar model, UIContext uiContext)
    {
        this.model = model;
        this.uiContext = uiContext;
        IsClickThrough = true;

        model.Entries
            .CollectionSize<ImmutableArray<ActionBarEntry?>, ActionBarEntry?>().SourceUpdated += updateButtonCount;

        addResourceBox();
        updateButtonCount(model.Entries.Value.Length);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    private void addResourceBox()
    {
        var control = new CompositeControl { new BackgroundBox() };
        var content = new CompositeControl();
        var layout = content.BuildFixedColumn();
        layout
            .AddLabel("Resources", Label.TextAnchorLeft)
            .AddLabel(
                model.CurrentResources.Transform(r => r.Value.ToString()),
                Label.TextAnchorRight,
                Binding.Constant(ResourcesColor));
        control.Add(content
            .WrapVerticallyCentered(layout.Height)
            .Anchor(a => a.MarginAllSides(Constants.UI.LayoutMargin)));

        control.Anchor(a => a
            .Right(margin: -barLeftMargin + buttonBetweenMargin, width: resourceBoxWidth, relativePercentage: 0.5)
            .Bottom(margin: buttonBottomMargin, height: buttonSize));
        Add(control);
    }

    private void updateButtonCount(int newCount)
    {
        var currentCount = buttons.Count;
        if (currentCount == newCount) return;

        if (currentCount > newCount)
        {
            for (var i = newCount; i < currentCount; i++)
            {
                Remove(buttons[i]);
            }
            buttons.RemoveRange(newCount, currentCount - newCount);
        }
        else
        {
            for (var i = currentCount; i < newCount; i++)
            {
                var button = buttonForIndex(i);
                buttons.Add(button);
                Add(button);
            }
        }
    }

    private Button buttonForIndex(int i)
    {
        var binding = model.Entries.ListElementByIndex<ImmutableArray<ActionBarEntry?>, ActionBarEntry?>(i);
        var button = uiContext.Factories.StandaloneIconButton(b => b
            .WithEnabled(binding.Transform(e => e is not null))
            .WithIcon(binding.Transform(e => e?.Icon ?? default))
            .WithIconScale(0.75f)
            .WithOnClick(() => binding.Value?.OnClick())
            .MakeHexagon()
            .WithShadow()
            .WithBlurredBackground()
            .WithBackgroundColors(Constants.UI.Button.DefaultBackgroundColors * 0.8f)
            .WithTooltip(binding.Transform(e => e?.Label ?? ""), TooltipAnchor.Direction.Top)
        ).Anchor(a => a
            .Left(margin: buttonLeftMargin(i), width: buttonSize, relativePercentage: 0.5)
            .Bottom(margin: buttonBottomMargin, height: buttonSize)
        );
        return button;
    }

    private const double buttonBottomMargin = Constants.UI.Button.Margin * 2;
    private const double buttonBetweenMargin = Constants.UI.Button.Margin;
    private const double buttonSize = Constants.UI.Button.SquareButtonSize;
    private const double barLeftMargin = -0.5 * ActionBarSize * (buttonSize + buttonBetweenMargin);
    private const double resourceBoxWidth = 1.8 * buttonSize;
    private static double buttonLeftMargin(int i) => barLeftMargin + i * (buttonSize + buttonBetweenMargin);
}
