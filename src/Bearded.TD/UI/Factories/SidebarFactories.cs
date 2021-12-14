using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

static class SidebarFactories
{
    public static Layouts.Layout AddMainSidebar(
        this Layouts.Layout layout, Action<IControlParent> builderFunc)
    {
        return AddMainSidebar(layout, () =>
        {
            var control = new CompositeControl();
            builderFunc(control);
            return control;
        });
    }

    public static Layouts.Layout AddMainSidebar(
        this Layouts.Layout layout, Func<Control> controlFactory)
    {
        layout.DockFractionalSizeToLeft(controlFactory(), .33);
        return layout;
    }

    public static Layouts.Layout AddStatusSidebar(
        this Layouts.Layout layout, Action<IControlParent> controlConsumer)
    {
        return AddStatusSidebar(layout, () =>
        {
            var control = new CompositeControl();
            controlConsumer(control);
            return control;
        });
    }

    public static Layouts.Layout AddStatusSidebar(
        this Layouts.Layout layout, Func<Control> controlConsumer)
    {
        layout.DockFractionalSizeToRight(controlConsumer(), .2);
        return layout;
    }
}