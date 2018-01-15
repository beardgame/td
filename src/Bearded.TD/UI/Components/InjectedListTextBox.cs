using System;
using System.Collections.Generic;
using amulware.Graphics;

namespace Bearded.TD.UI.Components
{
    class InjectedListTextBox<T> : ListTextBox<T>
    {
        private readonly Func<IReadOnlyList<T>> itemProvider;
        private readonly Func<T, (string, Color)> itemTransformer;

        public InjectedListTextBox(Bounds bounds, Func<IReadOnlyList<T>> itemProvider, Func<T, (string, Color)> itemTransformer)
            : base(bounds)
        {
            this.itemProvider = itemProvider;
            this.itemTransformer = itemTransformer;
        }

        protected override IReadOnlyList<T> GetItems() => itemProvider();

        protected override (string, Color) Format(T item) => itemTransformer(item);
    }
}