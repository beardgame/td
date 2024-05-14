using System;
using System.Collections.Generic;
using Bearded.UI.Controls;
using Label = Bearded.TD.UI.Controls.Label;

namespace Bearded.TD.UI.Factories;

static class FormFactories
{
    public static Control Form(this UIFactories factories, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder(factories);
        builderFunc(builder);
        return builder.Build();
    }

    public static Control DenseForm(this UIFactories factories, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder(factories).MakeDense();
        builderFunc(builder);
        return builder.Build();
    }

    public static Layouts.IColumnLayout AddForm(
        this Layouts.IColumnLayout columnLayout, UIFactories factories, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder(factories);
        builderFunc(builder);
        columnLayout.Add(builder.Build(), builder.Height);
        return columnLayout;
    }

    public sealed class Builder(UIFactories factories)
    {
        public UIFactories Factories { get; } = factories;

        private bool isDense;
        private bool isScrollable;
        private readonly List<(string?, Action<Layouts.Layout>)> rows = new();

        private double rowHeight =>
            isDense ? Constants.UI.Form.DenseFormRowHeight : Constants.UI.Form.FormRowHeight;

        public double Height => rowHeight * rows.Count;

        public Builder MakeDense()
        {
            isDense = true;
            return this;
        }

        public Builder MakeScrollable()
        {
            isScrollable = true;
            return this;
        }

        public Builder AddFormRow(string? label, Action<Layouts.Layout> layoutFunc)
        {
            rows.Add((label, layoutFunc));
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
                if (row.Item1 != null)
                {
                    rowLayout.FillContent(TextFactories.Label(row.Item1, Label.TextAnchorLeft));
                }
                columnLayout.Add(rowControl, rowHeight);
            }
        }
    }
}
