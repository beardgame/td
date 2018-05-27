using System;

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
    }
}
