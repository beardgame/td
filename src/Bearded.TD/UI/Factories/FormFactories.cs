using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.UI.Controls;
using static Bearded.TD.UI.Factories.LabelFactories;
using Label = Bearded.TD.UI.Controls.Label;

namespace Bearded.TD.UI.Factories
{
    static class FormFactories
    {
        public static Control Form(Action<FormBuilder> builderFunc)
        {
            var builder = new FormBuilder();
            builderFunc(builder);
            return builder.Build();
        }

        public class FormBuilder
        {
            private readonly List<(string, Action<LayoutFactories.LayoutBuilder>)> rows =
                new List<(string, Action<LayoutFactories.LayoutBuilder>)>();

            public FormBuilder AddFormRow(string label, Action<LayoutFactories.LayoutBuilder> builderFunc)
            {
                rows.Add((label, builderFunc));
                return this;
            }

            public Control Build()
            {
                var controls = rows.Select(tuple =>
                {
                    var rowControl = new CompositeControl();
                    var rowLayout = rowControl.BuildLayout();
                    tuple.Item2(rowLayout);
                    rowLayout.FillContent(Label(tuple.Item1, Label.TextAnchorLeft));
                    return rowControl;
                }).ToImmutableList<Control>();

                return new ListControl
                {
                    ItemSource = new FixedListItemSource(controls, Constants.UI.Form.FormRowHeight)
                };
            }
        }
    }
}
