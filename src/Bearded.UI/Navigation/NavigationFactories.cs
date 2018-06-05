using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.UI.Controls;

namespace Bearded.UI.Navigation
{
    public static class NavigationFactories
    {
        public static ModelFactoryAccumulator ForModels() => new ModelFactoryAccumulator();
        public static ViewFactoryAccumulator ForViews() => new ViewFactoryAccumulator();

        public static (ModelFactoryAccumulator models, ViewFactoryAccumulator views) ForBoth()
            => (ForModels(), ForViews());

        public static (ModelFactoryAccumulator models, ViewFactoryAccumulator views) Add<T, TParams>(
            this (ModelFactoryAccumulator models, ViewFactoryAccumulator views) accumulators,
            Func<T, Control> createView)
            where T : NavigationNode<TParams>, new()
        {
            return accumulators.Add<T, TParams>(() => new T(), createView);
        }

        public static (ModelFactoryAccumulator models, ViewFactoryAccumulator views) Add<T, TParams>(
            this (ModelFactoryAccumulator models, ViewFactoryAccumulator views) accumulators,
            Func<T> createNode,
            Func<T, Control> createView)
            where T : NavigationNode<TParams>
        {
            return (accumulators.models.Add<T, TParams>(createNode), accumulators.views.Add<T, TParams>(createView));
        }

        public static (ModelFactoryAccumulator models, ViewFactoryAccumulator views) AddModel<T, TParams>(
            this (ModelFactoryAccumulator models, ViewFactoryAccumulator views) accumulators,
            Func<T> createNode)
            where T : NavigationNode<TParams>
        {
            return (accumulators.models.Add<T, TParams>(createNode), accumulators.views);
        }

        public static (ModelFactoryAccumulator models, ViewFactoryAccumulator views) AddView<T, TParams>(
            this (ModelFactoryAccumulator models, ViewFactoryAccumulator views) accumulators,
            Func<T, Control> createView)
            where T : NavigationNode<TParams>
        {
            return (accumulators.models, accumulators.views.Add<T, TParams>(createView));
        }

        public static (IDictionary<Type, object> models, IDictionary<Type, object> views) ToDictionaries(
            this (ModelFactoryAccumulator models, ViewFactoryAccumulator views) accumulators)
        {
            return (accumulators.models.ToDictionary, accumulators.views.ToDictionary);
        }

        public class ModelFactoryAccumulator
        {
            private readonly Dictionary<Type, object> dict = new Dictionary<Type, object>();

            internal ModelFactoryAccumulator() {}

            public ModelFactoryAccumulator Add<T, TParams>(Func<T> createNode)
                where T : NavigationNode<TParams>
            {
                dict.Add(typeof(T), createNode);
                return this;
            }

            public IDictionary<Type, object> ToDictionary => new ReadOnlyDictionary<Type, object>(dict);
        }

        public class ViewFactoryAccumulator
        {
            private readonly Dictionary<Type, object> dict = new Dictionary<Type, object>();

            internal ViewFactoryAccumulator() {}

            public ViewFactoryAccumulator Add<T, TParams>(Func<T, Control> createView)
                where T : NavigationNode<TParams>
            {
                dict.Add(typeof(T), createView);
                return this;
            }

            public IDictionary<Type, object> ToDictionary => new ReadOnlyDictionary<Type, object>(dict);
        }
    }
}
