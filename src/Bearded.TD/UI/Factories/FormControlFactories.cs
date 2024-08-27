using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.Utilities;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.Text;
using TextInput = Bearded.TD.UI.Controls.TextInput;

namespace Bearded.TD.UI.Factories;

static class FormControlFactories
{
    public static Control Checkbox(this UIFactories factories, Binding<bool> valueBinding)
    {
        var checkbox = factories.StandaloneIconButton(Constants.Content.CoreUI.Sprites.CheckMark);
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
        this UIFactories factories,
        IEnumerable<T> options,
        Func<T, string> renderer,
        Binding<T> valueBinding,
        out double width)
    {
        var control = new CompositeControl();
        var row = control.BuildFixedRow();
        foreach (var o in options)
        {
            var button = factories.Button(b => b
                .WithLabel(renderer(o))
                .WithActive(valueBinding.Transform(v => Equals(v, o)))
                .WithOnClick(() => valueBinding.SetFromControl(o)));
            row.AddLeft(button, Constants.UI.Form.InputWidth);
        }
        width = row.Width;
        return control;
    }

    public static Control SliderSelect<T>(
        this UIFactories factories,
        IEnumerable<T> options,
        Func<T, string> renderer,
        Binding<T> valueBinding,
        out double width)
    {
        var control = new CompositeControl();
        var row = control.BuildFixedRow();
        var optionsList = options.ToList();
        foreach (var o in optionsList)
        {
            var label = TextFactories.Label(
                text: Binding.Constant(renderer(o)),
                color: valueBinding.Transform(t => t?.Equals(o) ?? false ? TextColor : DisabledTextColor)
            );
            row.AddLeft(label, Constants.UI.Form.InputWidth);
        }

        control.Add(
            factories.SliderFactory.Create(b => b
                .WithHandle(
                    new ComplexBox
                        { Components = Edge.Outer(2, Color.White), CornerRadius = Constants.UI.Form.InputHeight },
                    new Vector2d(Constants.UI.Form.InputWidth, Constants.UI.Form.InputHeight)
                )
                .WithHorizontalValue(
                    valueBinding.Transform(t => (double)optionsList.IndexOf(t), d => optionsList[(int)d]),
                    Interval.FromStartAndSize(0, optionsList.Count - 1), 1)
                .WithCommitMode(DragCommitMode.OnRelease)
            ));

        width = row.Width;
        return control;
    }

    public static Control DropdownSelect<T>(
        this UIFactories factories, IEnumerable<T> options, Func<T, string> renderer, Binding<T> valueBinding) =>
        DropdownSelect(factories, Binding.Create(options), renderer, valueBinding);

    public static Control DropdownSelect<T>(
        this UIFactories factories, Binding<IEnumerable<T>> options, Func<T, string> renderer, Binding<T> valueBinding)
    {
        // TODO: implement an actual dropdown select

        List<T> optionsList;
        var errorBinding = Binding.Create(false);

        updateOptions(options.Value);
        options.SourceUpdated += updateOptions;
        valueBinding.SourceUpdated += _ => updateValidity();

        var button = factories.Button(b => b
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

    public static Control NumberSelect(
        this UIFactories factories, int minValue, int maxValue, Binding<int> valueBinding)
    {
        var numericInput = new NumericInput(factories, valueBinding.Value)
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
            FontSize = FontSize,
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
                builder.Factories.Checkbox(valueBinding).WrapVerticallyCentered(Constants.UI.Checkbox.Size),
                Constants.UI.Checkbox.Size));
    }

    public static FormFactories.Builder AddButtonRow(
        this FormFactories.Builder builder,
        string label,
        VoidEventHandler onClick) =>
        AddButtonRow(builder, b => b.WithLabel(label).WithOnClick(onClick));

    public static FormFactories.Builder AddButtonRow(
        this FormFactories.Builder builder,
        BuilderFunc<ButtonFactory.TextButtonBuilder> builderFunc)
    {
        return builder.AddFormRow(null, layout => layout.DockFixedSizeToRight(
            builder.Factories.Button(builderFunc).WrapVerticallyCentered(Constants.UI.Button.Height),
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
                ButtonSelect(builder.Factories, options, renderer, valueBinding, out var width)
                    .WrapVerticallyCentered(Constants.UI.Form.InputHeight),
                width));
    }
    
    public static FormFactories.Builder AddSliderSelectRow<T>(
        this FormFactories.Builder builder,
        string label, IEnumerable<T> options,
        Func<T, string> renderer,
        Binding<T> valueBinding)
    {
        return builder.AddFormRow(
            label,
            layout => layout.DockFixedSizeToRight(
                SliderSelect(builder.Factories, options, renderer, valueBinding, out var width)
                    .WrapVerticallyCentered(Constants.UI.Form.InputHeight),
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
                DropdownSelect(builder.Factories, optionsBinding, renderer, valueBinding)
                    .WrapVerticallyCentered(Constants.UI.Form.InputHeight),
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
                NumberSelect(builder.Factories, minValue, maxValue, valueBinding)
                    .WrapVerticallyCentered(Constants.UI.Form.InputHeight),
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
