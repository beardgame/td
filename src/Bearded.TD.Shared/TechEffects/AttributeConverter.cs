using System;

namespace Bearded.TD.Shared.TechEffects
{
    public sealed class AttributeConverter<T>
    {
        private readonly Func<double, T> rawToWrapped;
        private readonly Func<T, double> wrappedToRaw;

        public AttributeConverter(Func<double, T> rawToWrapped, Func<T, double> wrappedToRaw)
        {
            this.rawToWrapped = rawToWrapped;
            this.wrappedToRaw = wrappedToRaw;
        }

        public T ToWrapped(double d) => rawToWrapped.Invoke(d);
        public double ToRaw(T wrapped) => wrappedToRaw.Invoke(wrapped);
    }
}
