using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    static class FormControlFactories
    {
        public static Control Checkbox(Binding<bool> valueBinding)
        {
            var checkbox = new Button();
            checkbox.Clicked += toggleCheckboxState;
            valueBinding.ValueChanged += _ => updateCheckboxState();
            updateCheckboxState();
            return checkbox;

            void toggleCheckboxState()
            {
                valueBinding.Value = !valueBinding.Value;
                updateCheckboxState();
            }

            void updateCheckboxState()
            {
                checkbox.RemoveAllChildren();
                checkbox.Add(new Border());
                if (valueBinding.Value)
                {
                    checkbox.Add(new Label("x"));
                }
            }
        }

        public static Control DropdownSelect<T>(
            IEnumerable<T> options, Func<T, string> renderer, Binding<T> valueBinding)
        {
            // TODO: implement an actual dropdown select

            var optionsList = options.ToList();

            var button = ButtonFactories.Button(() => renderer(valueBinding.Value));
            button.Clicked += advanceSelection;
            return button;

            void advanceSelection()
            {
                var selectedIndex = optionsList.FindIndex(e => e.Equals(valueBinding.Value));
                DebugAssert.State.Satisfies(selectedIndex != -1);
                selectedIndex = (selectedIndex + 1) % optionsList.Count;
                valueBinding.Value = optionsList[selectedIndex];
            }
        }

        public static Control NumberSelect(int minValue, int maxValue, Binding<int> valueBinding)
        {
            var numericInput = new NumericInput(valueBinding.Value)
                {
                    MinValue = minValue,
                    MaxValue = maxValue
                };
            numericInput.ValueChanged += newValue => valueBinding.Value = newValue;
            valueBinding.ValueChanged += newValue => numericInput.Value = newValue;
            return numericInput;
        }

        public static FormFactories.FormBuilder AddCheckboxRow(
            this FormFactories.FormBuilder builder, string label, Binding<bool> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToLeft(
                    Checkbox(valueBinding).WrapVerticallyCentered(Constants.UI.Checkbox.Size),
                    Constants.UI.Checkbox.Size));
        }

        public static FormFactories.FormBuilder AddDropdownSelectRow(
            this FormFactories.FormBuilder builder,
            string label,
            IEnumerable<string> options,
            Binding<string> valueBinding) =>
            AddDropdownSelectRow(builder, label, options, s => s, valueBinding);

        public static FormFactories.FormBuilder AddDropdownSelectRow<T>(
            this FormFactories.FormBuilder builder,
            string label, IEnumerable<T> options,
            Func<T, string> renderer,
            Binding<T> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToRight(
                    DropdownSelect(options, renderer, valueBinding).WrapVerticallyCentered(Constants.UI.Button.Height),
                    Constants.UI.Button.Width));
        }

        public static FormFactories.FormBuilder AddNumberSelectRow(
            this FormFactories.FormBuilder builder,
            string label,
            int minValue,
            int maxValue,
            Binding<int> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToRight(
                    NumberSelect(minValue, maxValue, valueBinding).WrapVerticallyCentered(Constants.UI.Button.Height),
                    Constants.UI.Button.Width));
        }
    }
}
