using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using TextInput = Bearded.TD.UI.Controls.TextInput;

namespace Bearded.TD.UI.Factories
{
    static class FormControlFactories
    {
        public static Control Checkbox(Binding<bool> valueBinding)
        {
            var checkbox = new Button();
            checkbox.Clicked += toggleCheckboxState;
            valueBinding.SourceUpdated += _ => updateCheckboxState();
            updateCheckboxState();
            return checkbox;

            void toggleCheckboxState()
            {
                valueBinding.SetFromControl(!valueBinding.Value);
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
                valueBinding.SetFromControl(optionsList[selectedIndex]);
            }
        }

        public static Control NumberSelect(int minValue, int maxValue, Binding<int> valueBinding)
        {
            var numericInput = new NumericInput(valueBinding.Value)
                {
                    MinValue = minValue,
                    MaxValue = maxValue
                };
            numericInput.ValueChanged += valueBinding.SetFromControl;
            valueBinding.SourceUpdated += newValue => numericInput.Value = newValue;
            return numericInput;
        }

        public static Control TextInput(Binding<string> valueBinding)
        {
            var textInput = new TextInput
            {
                FontSize = Constants.UI.Text.FontSize,
                Text = valueBinding.Value
            };
            textInput.MoveCursorToEnd();
            textInput.TextChanged += () => valueBinding.SetFromControl(textInput.Text);
            valueBinding.SourceUpdated += newValue => textInput.Text = newValue;
            return textInput;
        }

        public static FormFactories.Builder AddCheckboxRow(
            this FormFactories.Builder builder, string label, Binding<bool> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToLeft(
                    Checkbox(valueBinding).WrapVerticallyCentered(Constants.UI.Checkbox.Size),
                    Constants.UI.Checkbox.Size));
        }

        public static FormFactories.Builder AddButtonRow(
            this FormFactories.Builder builder,
            string label,
            VoidEventHandler onClick) =>
            AddButtonRow(builder, b => b.WithLabel(label).WithOnClick(onClick));

        public static FormFactories.Builder AddButtonRow(
            this FormFactories.Builder builder,
            BuilderFunc<ButtonFactories.Builder> builderFunc)
        {
            return builder.AddFormRow(null, layout => layout.DockFixedSizeToRight(
                ButtonFactories.Button(builderFunc).WrapVerticallyCentered(Constants.UI.Button.Height),
                Constants.UI.Button.Width));
        }

        public static FormFactories.Builder AddDropdownSelectRow(
            this FormFactories.Builder builder,
            string label,
            IEnumerable<string> options,
            Binding<string> valueBinding) =>
            AddDropdownSelectRow(builder, label, options, s => s, valueBinding);

        public static FormFactories.Builder AddDropdownSelectRow<T>(
            this FormFactories.Builder builder,
            string label, IEnumerable<T> options,
            Func<T, string> renderer,
            Binding<T> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToRight(
                    DropdownSelect(options, renderer, valueBinding).WrapVerticallyCentered(Constants.UI.Form.InputHeight),
                    Constants.UI.Form.InputWidth));
        }

        public static FormFactories.Builder AddNumberSelectRow(
            this FormFactories.Builder builder,
            string label,
            int minValue,
            int maxValue,
            Binding<int> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToRight(
                    NumberSelect(minValue, maxValue, valueBinding).WrapVerticallyCentered(Constants.UI.Form.InputHeight),
                    Constants.UI.Form.InputWidth));
        }

        public static FormFactories.Builder AddTextInputRow(
            this FormFactories.Builder builder,
            string label,
            Binding<string> valueBinding)
        {
            return builder.AddFormRow(
                label,
                layout => layout.DockFixedSizeToRight(
                    TextInput(valueBinding).WrapVerticallyCentered(Constants.UI.Form.InputHeight),
                    Constants.UI.Form.InputWidth));
        }
    }
}
