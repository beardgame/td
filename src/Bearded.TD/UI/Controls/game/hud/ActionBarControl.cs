using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class ActionBarControl : CompositeControl
{
    private const float buttonHeightPercentage = 1f / Constants.Game.GameUI.ActionBarSize;

    private readonly ActionBar model;
    private readonly List<Button> buttons = [];

    public ActionBarControl(ActionBar model)
    {
        this.model = model;

        Add(new BackgroundBox());

        model.Entries
            .CollectionSize<ImmutableArray<ActionBarEntry?>, ActionBarEntry?>().SourceUpdated += updateButtonCount;
        updateButtonCount(model.Entries.Value.Length);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

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
        var button = ButtonFactories.Button(b => b
            .WithEnabled(binding.Transform(e => e is not null))
            .WithLabel(binding.Transform(e => e?.Label ?? ""))
            .WithResourceCost(binding.Transform(e => e?.Cost ?? ResourceAmount.Zero))
            .WithOnClick(() => binding.Value?.OnClick())
        ).Anchor(a => a
            .Top(relativePercentage: i * buttonHeightPercentage)
            .Bottom(relativePercentage: (i + 1) * buttonHeightPercentage));
        tempAddIcon(button, binding);
        return button;
    }

    private void tempAddIcon(Button button, IReadonlyBinding<ActionBarEntry?> binding)
    {
        var sprite = new Sprite { SpriteId = binding.Value?.Icon ?? default, Color = Color.White * 0.5f };
        sprite.BindIsVisible(binding.Transform(e => e != null));
        binding.SourceUpdated += newEntry =>
        {
            if (newEntry is not null)
            {
                sprite.SpriteId = newEntry.Icon;
            }
        };
        button.Add(sprite.Anchor(a => a.Left(margin: 4, width: 24)));
    }
}
