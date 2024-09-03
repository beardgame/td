using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplayControl : CompositeControl
{
    public ResourceDisplayControl(ResourceDisplay model)
    {
        Add(new BackgroundBox());

        var content = new CompositeControl();
        var layout = content.BuildFixedColumn();
        layout
            .AddLabel("Resources", Label.TextAnchorLeft)
            .AddLabel(
                model.CurrentScrap.Transform(r => r.Value.ToString()),
                Label.TextAnchorRight,
                Binding.Constant(Constants.Game.GameUI.ResourcesColor))
            .AddLabel(
                model.CurrentCoreEnergy.Transform(r => r.Value.ToString()),
                Label.TextAnchorRight,
                Binding.Constant(Constants.Game.GameUI.EnergyColor));
        Add(content
            .WrapVerticallyCentered(layout.Height)
            .Anchor(a => a.MarginAllSides(Constants.UI.LayoutMargin)));

    }
}
