using System.Collections.Generic;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    static class CollectionEditorFactories
    {
        public static Control CollectionEditor(List<(string label, Binding<bool> binding)> entries)
        {
            return new CompositeControl()
            {
                new Border(),
                FormFactories.DenseForm(builder =>
                {
                    builder.MakeScrollable();
                    foreach (var (label, binding) in entries)
                    {
                        builder.AddCheckboxRow(label, binding);
                    }
                })
            };
        }

        public static LayoutFactories.IColumnBuilder AddCollectionEditor(
            this LayoutFactories.IColumnBuilder columnBuilder,
            List<(string label, Binding<bool> binding)> entries,
            int numRowsShown = 5) =>
            columnBuilder.Add(CollectionEditor(entries), Constants.UI.Form.DenseFormRowHeight * numRowsShown);
    }
}
