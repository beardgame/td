using System;
using System.Linq;

namespace Bearded.UI.Controls
{
    public static class Extensions
    {
        public static T Anchor<T>(this T control, Func<AnchorTemplate, AnchorTemplate> build)
            where T : Control
        {
            build(new AnchorTemplate(control)).ApplyTo(control);

            return control;
        }

        public static T Subscribe<T>(this T control, Action<T> subscribe)
            where T : Control
        {
            subscribe(control);
            return control;
        }

        public static T FirstChildOfType<T>(this IControlParent parent)
        {
            return parent.Children.OfType<T>().FirstOrDefault();
        }
        
        public static bool IsDescendantOf(this Control control, IControlParent parent)
        {
            return control.Parent == parent
                || (control.Parent is Control parentControl && parentControl.IsDescendantOf(parent));
        }
    }
}
