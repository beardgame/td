using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    static class SidebarFactories
    {
        public static LayoutFactories.LayoutBuilder AddMainSidebar(
            this LayoutFactories.LayoutBuilder layout, Action<IControlParent> builderFunc)
        {
            return AddMainSidebar(layout, () =>
            {
                var control = new CompositeControl();
                builderFunc(control);
                return control;
            });
        }

        public static LayoutFactories.LayoutBuilder AddMainSidebar(
            this LayoutFactories.LayoutBuilder layout, Func<Control> controlFactory)
        {
            layout.DockFractionalSizeToLeft(controlFactory(), .33);
            return layout;
        }

        public static LayoutFactories.LayoutBuilder AddStatusSidebar(
            this LayoutFactories.LayoutBuilder layout, Action<IControlParent> builderFunc)
        {
            return AddStatusSidebar(layout, () =>
            {
                var control = new CompositeControl();
                builderFunc(control);
                return control;
            });
        }

        public static LayoutFactories.LayoutBuilder AddStatusSidebar(
            this LayoutFactories.LayoutBuilder layout, Func<Control> controlFactory)
        {
            layout.DockFractionalSizeToRight(controlFactory(), .2);
            return layout;
        }
    }
}
