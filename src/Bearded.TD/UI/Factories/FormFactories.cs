using System;
using System.Collections.Generic;
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

        public static Control DenseForm(Action<FormBuilder> builderFunc)
        {
            var builder = new FormBuilder().MakeDense();
            builderFunc(builder);
            return builder.Build();
        }

        public static LayoutFactories.IColumnBuilder AddForm(
            this LayoutFactories.IColumnBuilder columnBuilder, Action<FormBuilder> builderFunc)
        {
            var builder = new FormBuilder();
            builderFunc(builder);
            columnBuilder.Add(builder.Build(), builder.Height);
            return columnBuilder;
        }

        public class FormBuilder
        {
            private bool isDense;
            private bool isScrollable;
            private readonly List<(string, Action<LayoutFactories.LayoutBuilder>)> rows =
                new List<(string, Action<LayoutFactories.LayoutBuilder>)>();

            private double rowHeight =>
                isDense ? Constants.UI.Form.DenseFormRowHeight : Constants.UI.Form.FormRowHeight;

            public double Height => rowHeight * rows.Count;

            public FormBuilder MakeDense()
            {
                isDense = true;
                return this;
            }

            public FormBuilder MakeScrollable()
            {
                isScrollable = true;
                return this;
            }

            public FormBuilder AddFormRow(string label, Action<LayoutFactories.LayoutBuilder> builderFunc)
            {
                rows.Add((label, builderFunc));
                return this;
            }

            public Control Build()
            {
                var control = new CompositeControl();
                buildRows(isScrollable ? control.BuildScrollableColumn() : control.BuildFixedColumn());
                return control;
            }

            private void buildRows(LayoutFactories.IColumnBuilder columnBuilder)
            {
                foreach (var row in rows)
                {
                    var rowControl = new CompositeControl();
                    var rowLayout = rowControl.BuildLayout();
                    row.Item2(rowLayout);
                    rowLayout.FillContent(Label(row.Item1, Label.TextAnchorLeft));
                    columnBuilder.Add(rowControl, rowHeight);
                }
            }
        }
    }
}
