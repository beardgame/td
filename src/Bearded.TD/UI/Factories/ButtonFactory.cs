using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Tooltips;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

sealed partial class ButtonFactory(Animations animations, TooltipFactory tooltips)
{
    public Button TextButton(BuilderFunc<TextButtonBuilder> f)
    {
        var builder = new TextButtonBuilder(animations, tooltips);
        f(builder);
        return builder.Build();
    }

    public Button IconButton(BuilderFunc<IconButtonBuilder> f)
    {
        var builder = new IconButtonBuilder(animations, tooltips);
        f(builder);
        return builder.Build();
    }
}
