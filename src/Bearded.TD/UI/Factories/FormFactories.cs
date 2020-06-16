using System;
using System.Collections.Generic;
using Bearded.UI.Controls;
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

        public static Layouts.IColumnLayout AddForm(
            this Layouts.IColumnLayout columnLayout, Action<FormBuilder> builderFunc)
        {
            var builder = new FormBuilder();
            builderFunc(builder);
            columnLayout.Add(builder.Build(), builder.Height);
            return columnLayout;
        }

        public class FormBuilder
        {
            private bool isDense;
            private bool isScrollable;
            private readonly List<(string, Action<Layouts.Layout>)> rows =
                new List<(string, Action<Layouts.Layout>)>();

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

            public FormBuilder AddFormRow(string label, Action<Layouts.Layout> builderFunc)
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

            private void buildRows(Layouts.IColumnLayout columnLayout)
            {
                foreach (var row in rows)
                {
                    var rowControl = new CompositeControl();
                    var rowLayout = rowControl.BuildLayout();
                    row.Item2(rowLayout);
                    rowLayout.FillContent(TextFactories.Label(row.Item1, Label.TextAnchorLeft));
                    columnLayout.Add(rowControl, rowHeight);
                }
            }
        }
    }
}
