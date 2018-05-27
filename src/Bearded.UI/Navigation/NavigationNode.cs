namespace Bearded.UI.Navigation
{
    public abstract class NavigationNode<T>
    {
        protected NavigationController Navigation { get; private set; }

        internal void Initialize(NavigationContext<T> context)
        {
            Navigation = context.Navigation;
            Initialize(context.Dependencies, context.Parameters);
        }

        protected abstract void Initialize(DependencyResolver dependencies, T parameters);
    }
}
