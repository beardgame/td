using Bearded.Utilities;

namespace Bearded.TD.Utilities
{
    sealed class Binding<T>
    {
        private T innerValue;

        public T Value
        {
            get => innerValue;
            set
            {
                if (value.Equals(innerValue)) return;
                innerValue = value;
                ValueChanged?.Invoke(value);
            }
        }

        public event GenericEventHandler<T>? ValueChanged;

        public Binding() : this(default) { }

        public Binding(T initialValue)
        {
            innerValue = initialValue;
        }
    }

    static class Binding
    {
        public static Binding<T> Create<T>(T initialValue) => new Binding<T>(initialValue);

        public static Binding<T> Create<T>(T initialValue, GenericEventHandler<T> syncBack)
        {
            var binding = new Binding<T>(initialValue);
            binding.ValueChanged += syncBack;
            return binding;
        }
    }
}
