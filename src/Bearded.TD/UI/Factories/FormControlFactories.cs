using System;
using System.Collections.Generic;
using static Bearded.TD.UI.Factories.LabelFactories;

namespace Bearded.TD.UI.Factories
{
    static class FormControlFactories
    {
        public static FormFactories.FormBuilder AddCheckboxRow(
            this FormFactories.FormBuilder builder, string label, bool isChecked = false)
        {
            return builder.AddFormRow(label, layout => layout.DockFixedSizeToRight(Label("X"), 48));
        }

        public static FormFactories.FormBuilder AddSelectRow(
            this FormFactories.FormBuilder builder, string label, IEnumerable<string> options) =>
            AddSelectRow(builder, label, options, s => s);

        public static FormFactories.FormBuilder AddSelectRow<T>(
            this FormFactories.FormBuilder builder, string label, IEnumerable<T> options, Func<T, string> renderer)
        {
            return builder;
        }
    }
}
