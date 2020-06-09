using Bearded.Utilities;

namespace Bearded.TD.Utilities
{
    sealed class Binding<T>
    {
        public T Value { get; private set; }

        public event GenericEventHandler<T>? ControlUpdated;
        public event GenericEventHandler<T>? SourceUpdated;

        public Binding() : this(default) { }

        public Binding(T initialValue)
        {
            Value = initialValue;
        }

        public void SetFromControl(T value)
        {
            if (value.Equals(Value)) return;
            Value = value;
            ControlUpdated?.Invoke(value);
        }

        public void SetFromSource(T value)
        {
            if (value.Equals(Value)) return;
            Value = value;
            SourceUpdated?.Invoke(value);
        }
    }

    static class Binding
    {
        public static Binding<T> Create<T>(T initialValue) => new Binding<T>(initialValue);

        public static Binding<T> Create<T>(T initialValue, GenericEventHandler<T> syncBack)
        {
            var binding = new Binding<T>(initialValue);
            binding.ControlUpdated += syncBack;
            return binding;
        }
    }
}
