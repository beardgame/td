using System;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Window;

namespace Bearded.TD.UI.Factories;

static class WindowFactories
{
    public static Control Window(BuilderFunc<Builder> builderFunc)
    {
        var windowBuilder = new Builder();
        builderFunc(windowBuilder);
        var control = windowBuilder.Build();
        return control;
    }

    public static T AnchorAsWindow<T>(this T control, double contentWidth, double contentHeight) where T : Control
    {
        var actualWidth = contentWidth;
        var actualHeight = contentHeight + TitlebarHeight;
        control.Anchor(a => a
            .Left(relativePercentage: 0.5, margin: -0.5 * actualWidth, width: actualWidth)
            .Top(relativePercentage: 0.5, margin: -0.5 * actualHeight, height: actualHeight));
        return control;
    }

    public sealed class Builder
    {
        private string? title;
        private VoidEventHandler? onClose;
        private Control? content;
        private Shadow? shadow;

        public Builder WithTitle(string title)
        {
            this.title = title;
            return this;
        }

        public Builder WithOnClose(VoidEventHandler onClose)
        {
            this.onClose = onClose;
            return this;
        }

        public Builder WithContent(Control control)
        {
            content = control;
            return this;
        }

        public Builder WithShadow(Shadow? shadow = null)
        {
            this.shadow = shadow ?? Constants.UI.Window.Shadow;
            return this;
        }

        public Control Build()
        {
            validate();

            var control = new OnTopCompositeControl();
            var background = new ComplexBox
            {
                CornerRadius = CornerRadius,
                Components = BackgroundComponents,
            };
            control.Add(shadow != null
                ? background.WithDropShadow(shadow.Value)
                : [background]);

            var titleBar = new CompositeControl();
            var titleRow = titleBar.BuildFixedRow()
                .AddButtonRight(b => b.WithLabel("close").WithOnClick(onClose!));
            if (title != null)
                titleRow.AddHeaderLeft(title, 100);

            control.BuildLayout()
                .ForFullScreen()
                .DockFixedSizeToTop(titleBar, TitlebarHeight)
                .FillContent(content!);

            return control;
        }

        private void validate()
        {
            if (onClose is null)
            {
                throw new InvalidOperationException("Cannot make a window without action on close");
            }

            if (content is null)
            {
                throw new InvalidOperationException("Cannot make a window without content");
            }
        }
    }
}
