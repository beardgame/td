using System;
using System.Collections.Generic;
using Bearded.UI.Controls;
using Void = Bearded.Utilities.Void;

namespace Bearded.UI.Navigation
{
    public sealed class NavigationController
    {
        private readonly RootControl root;
        private readonly DependencyResolver dependencyResolver;
        private readonly IDictionary<Type, object> modelFactories;
        private readonly IDictionary<Type, object> viewFactories;

        public NavigationController(
            RootControl root,
            DependencyResolver dependencyResolver,
            IDictionary<Type, object> modelFactories,
            IDictionary<Type, object> viewFactories)
        {
            this.root = root;
            this.dependencyResolver = dependencyResolver;
            this.modelFactories = modelFactories;
            this.viewFactories = viewFactories;
        }

        public void GoTo<TModel>()
            where TModel : NavigationNode<Void>
        {
            GoTo<TModel, Void>(default(Void));
        }

        public void GoTo<TModel, TParameters>(TParameters parameters)
            where TModel : NavigationNode<TParameters>
        {
            var model = findModelFactory<TModel>()();
            model.Initialize(createNavigationContext(parameters));
            var view = findViewFactory<TModel>()(model);
            root.Add(view);
        }

        private Func<T> findModelFactory<T>()
            => (Func<T>) modelFactories[typeof(T)];

        private Func<T, Control> findViewFactory<T>()
            => (Func<T, Control>) viewFactories[typeof(T)];

        private NavigationContext<T> createNavigationContext<T>(T parameters)
            => new NavigationContext<T>(this, dependencyResolver, parameters);
    }
}
