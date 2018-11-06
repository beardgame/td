using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class NumericInput : CompositeControl
    {
        public event GenericEventHandler<int> ValueChanged; 

        private readonly TextInput textInput;
        private int value = -1;
        private bool supressTextChangedEvent;
        private int minValue = 0;
        private int maxValue = 100;

        public int Value
        {
            get => value;
            set
            {
                var clampedValue = value.Clamped(MinValue, MaxValue);
                if (clampedValue == this.value)
                    return;

                this.value = clampedValue;
                supressTextChangedEvent = true;
                textInput.Text = clampedValue.ToString();

                onValueChanged();
            }
        }

        public int MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                validateBounds();
            }
        }

        public int MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                validateBounds();
            }
        }

        public int StepSize { get; set; } = 10;

        public double FontSize
        {
            get => textInput.FontSize;
            set => textInput.FontSize = value;
        }

        public NumericInput(int value = 0)
        {
            textInput = new TextInput
            {
                AllowSpecialCharacters = false,
                AllowLetters = false
            };
            var buttonUp = new Button { new Label("+") };
            var buttonDown = new Button { new Label("-") };

            buttonUp.Clicked += stepUp;
            buttonDown.Clicked += stepDown;
            textInput.TextChanged += textChanged;

            Add(textInput.Anchor(a => a.Right(20)));
            Add(buttonUp.Anchor(a => a.Right(0, 20).Bottom(0, null, 0.5)));
            Add(buttonDown.Anchor(a => a.Right(0, 20).Top(0, null, 0.5)));

            Value = value;
        }

        private void textChanged()
        {
            if (supressTextChangedEvent)
            {
                supressTextChangedEvent = false;
                return;
            }

            Value = int.TryParse(textInput.Text, out var v) ? v : 0;
        }

        private void stepUp() => Value += StepSize;
        private void stepDown() => Value -= StepSize;
        
        private void onValueChanged()
        {
            ValueChanged?.Invoke(value);
        }

        private void validateBounds()
        {
            Value = value;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
