using System;

namespace Bearded.UI.Controls
{
    static class Extensions
    {
        public static T Anchor<T>(this T control, Func<AnchorTemplate, AnchorTemplate> build)
            where T : Control
        {
            build(new AnchorTemplate(control)).ApplyTo(control);

            return control;
        }
    }
}
