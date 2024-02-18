using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using TextInput = Bearded.TD.UI.Controls.TextInput;

namespace Bearded.TD.UI.Factories;

static class FormControlFactories
{
    public static Control Checkbox(Binding<bool> valueBinding)
    {
        var checkbox = ButtonFactories.IconButtonBuilder
            .ForStandaloneButton()
            .WithCustomSize(Constants.UI.Checkbox.Size)
            .WithIcon(Constants.Content.CoreUI.Sprites.CheckMark)
            .Build();

        var check = checkbox.Children.First(c => c is Sprite);

        checkbox.Clicked += _ => toggleCheckboxState();
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
            check.IsVisible = valueBinding.Value;
        }
    }

    public static Control ButtonSelect<T>(
        IEnumerable<T> options, Func<T, string> renderer, Binding<T> valueBinding, out double width)
    {
        var control = new CompositeControl();
        var row = control.BuildFixedRow();
        foreach (var o in options)
        {
            var button = ButtonFactories.Button(b => b
                .WithLabel(renderer(o))
                .WithActive(valueBinding.Transform(v => Equals(v, o)))
                .WithOnClick(() => valueBinding.SetFromControl(o)));
            row.AddLeft(button, Constants.UI.Form.InputWidth);
        }
        width = row.Width;
        return control;
    }

    public static Control DropdownSelect<T>(
            IEnumerable<T> options, Func<T, string> renderer, Binding<T> valueBinding) =>
        DropdownSelect(Binding.Create(options), renderer, valueBinding);

    public static Control DropdownSelect<T>(
        Binding<IEnumerable<T>> options, Func<T, string> renderer, Binding<T> valueBinding)
    {
        // TODO: implement an actual dropdown select

        List<T> optionsList;
        var errorBinding = Binding.Create(false);

        updateOptions(options.Value);
        options.SourceUpdated += updateOptions;
        valueBinding.SourceUpdated += _ => updateValidity();

        var button = ButtonFactories.Button(b => b
            .WithLabel(valueBinding.Transform(renderer))
            .WithError(errorBinding)
            .WithOnClick(advanceSelection));
        return button;

        void updateOptions(IEnumerable<T> newOptions)
        {
            optionsList = newOptions.ToList();
            updateValidity();
        }

        void updateValidity()
        {
            var selectedIndex = optionsList.FindIndex(e => e.Equals(valueBinding.Value));
            errorBinding.SetFromControl(selectedIndex == -1);
        }

        void advanceSelection()
        {
            if (optionsList.Count == 0)
            {
                return;
            }

            var selectedIndex = optionsList.FindIndex(e => e.Equals(valueBinding.Value));
            // If the index is not found, selectedIndex is -1, so this will set the selection to the first possible
            // field
            selectedIndex = (selectedIndex + 1) % optionsList.Count;
            valueBinding.SetFromControl(optionsList[selectedIndex]);
            errorBinding.SetFromControl(false);
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
        BuilderFunc<ButtonFactories.TextButtonBuilder> builderFunc)
    {
        return builder.AddFormRow(null, layout => layout.DockFixedSizeToRight(
            ButtonFactories.Button(builderFunc).WrapVerticallyCentered(Constants.UI.Button.Height),
            Constants.UI.Button.Width));
    }

    public static FormFactories.Builder AddButtonSelectRow(
        this FormFactories.Builder builder,
        string label,
        IEnumerable<string> options,
        Binding<string> valueBinding) =>
        AddButtonSelectRow(builder, label, options, s => s, valueBinding);

    public static FormFactories.Builder AddButtonSelectRow<T>(
        this FormFactories.Builder builder,
        string label, IEnumerable<T> options,
        Func<T, string> renderer,
        Binding<T> valueBinding)
    {
        return builder.AddFormRow(
            label,
            layout => layout.DockFixedSizeToRight(
                ButtonSelect(options, renderer, valueBinding, out var width).WrapVerticallyCentered(Constants.UI.Form.InputHeight),
                width));
    }

    public static FormFactories.Builder AddDropdownSelectRow(
        this FormFactories.Builder builder,
        string label,
        IEnumerable<string> options,
        Binding<string> valueBinding) =>
        AddDropdownSelectRow(builder, label, options, s => s, valueBinding);

    public static FormFactories.Builder AddDropdownSelectRow<T>(
        this FormFactories.Builder builder,
        string label,
        IEnumerable<T> options,
        Func<T, string> renderer,
        Binding<T> valueBinding)
    {
        return builder.AddDropdownSelectRow(
            label,
            Binding.Create(options),
            renderer,
            valueBinding);
    }

    public static FormFactories.Builder AddDropdownSelectRow<T>(
        this FormFactories.Builder builder,
        string label,
        Binding<IEnumerable<T>> optionsBinding,
        Func<T, string> renderer,
        Binding<T> valueBinding)
    {
        return builder.AddFormRow(
            label,
            layout => layout.DockFixedSizeToRight(
                DropdownSelect(optionsBinding, renderer, valueBinding).WrapVerticallyCentered(Constants.UI.Form.InputHeight),
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
